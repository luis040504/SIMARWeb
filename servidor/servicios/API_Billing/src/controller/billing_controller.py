from fastapi import HTTPException, status, UploadFile
from datetime import datetime
from bson import ObjectId
import os
import httpx
import re
from ..config.database import facturas_collection, pac_settings_collection
from ..models.billing import Billing
from ..schemas.billing_schema import BillingCreateSchema, BillingUpdateSchema, BillingFilterSchema
from ..schemas.aggregator_schema import ReadyToBillSchema, ClientSummarySchema, ContractSummarySchema, ResidueDetailSchema

class BillingController:
    
    @staticmethod
    async def get_all(filtro: BillingFilterSchema = None):
        query = {}
        
        if filtro and filtro.include_deleted:
            pass 
        else:
            query["activo"] = True
            
        if filtro:
            if filtro.status:
                query["status"] = filtro.status
            if filtro.upload_type:
                query["upload_type"] = filtro.upload_type
            if filtro.record_type:
                query["record_type"] = filtro.record_type
            if filtro.issuer_tax_id:
                query["issuer.tax_id"] = {"$regex": filtro.issuer_tax_id, "$options": "i"}
            if filtro.receiver_tax_id:
                query["receiver.tax_id"] = {"$regex": filtro.receiver_tax_id, "$options": "i"}
            
            if filtro.search_query:
                regex_pattern = {"$regex": filtro.search_query, "$options": "i"}
                query["$or"] = [
                    {"receiver.name": regex_pattern},
                    {"receiver.tax_id": regex_pattern},
                    {"fiscal_data.invoice_folio": regex_pattern}
                ]
                
            if filtro.start_date or filtro.end_date:
                query["fiscal_data.issue_date"] = {}
                if filtro.start_date:
                    query["fiscal_data.issue_date"]["$gte"] = filtro.start_date
                if filtro.end_date:
                    query["fiscal_data.issue_date"]["$lte"] = filtro.end_date
        
        cursor = facturas_collection.find(query).sort("metadata.created_at", -1)
        facturas = await cursor.to_list(length=None)
        
        return [Billing(**fac) for fac in facturas]
    
    @staticmethod
    async def get_by_client_id(client_id: str):
        cursor = facturas_collection.find({"receiver.client_id": client_id, "activo": True}).sort("metadata.created_at", -1)
        facturas = await cursor.to_list(length=None)
        return [Billing(**fac) for fac in facturas]
    
    @staticmethod
    async def get_by_id(billing_id: str):
        if not ObjectId.is_valid(billing_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        factura = await facturas_collection.find_one({"_id": ObjectId(billing_id)})
        
        if not factura:
            raise HTTPException(status_code=404, detail="Factura no encontrada")
        
        return Billing(**factura)
    
    @staticmethod
    async def create(billing_data: BillingCreateSchema):
        
        import random
        # Generar un folio preliminar si no viene uno
        if not billing_data.fiscal_data or not billing_data.fiscal_data.invoice_folio:
            if not billing_data.fiscal_data:
                from ..models.billing import FiscalData
                billing_data.fiscal_data = FiscalData(issue_date=datetime.now())
            billing_data.fiscal_data.invoice_folio = f"PRE-{random.randint(1000, 9999)}"

        calculated_total = billing_data.financials.subtotal + billing_data.financials.tax_total - billing_data.financials.discount
        if abs(calculated_total - billing_data.financials.total) > 0.01:
            from ..handlers.exceptions import AppException
            raise AppException(message="El total no cuadra con el subtotal, impuestos y descuento", status_code=400, code="INVALID_TOTAL")
            
        billing_dict = billing_data.model_dump()
        billing_dict["metadata"]["created_at"] = datetime.now()
        billing_dict["metadata"]["updated_at"] = datetime.now()
        billing_dict["activo"] = True
        
        result = await facturas_collection.insert_one(billing_dict)
        
        print(f"DEBUG: Created invoice with service_id: {billing_dict.get('service_id')}")
        
        new_billing = await facturas_collection.find_one({"_id": result.inserted_id})
        
        return Billing(**new_billing)
    
    @staticmethod
    async def update(billing_id: str, billing_data: BillingUpdateSchema):
        if not ObjectId.is_valid(billing_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        existing = await facturas_collection.find_one({"_id": ObjectId(billing_id), "activo": True})
        if not existing:
            raise HTTPException(status_code=404, detail="Factura no encontrada o inactiva")
            
        immutable_states = ["Accepted", "CANCELLED"]
        if existing.get("status") in immutable_states:
            from ..handlers.exceptions import AppException
            raise AppException(message="No se puede editar una factura que ya ha sido procesada o cancelada", status_code=400, code="IMMUTABLE_STATE")
        
        update_data = billing_data.model_dump(exclude_unset=True)
        
        if update_data.get("metadata") is None:
            current_metadata = existing.get("metadata", {})
            current_metadata["updated_at"] = datetime.now()
            update_data["metadata"] = current_metadata
        else:
            update_data["metadata"]["updated_at"] = datetime.now()
        
        if update_data:
            await facturas_collection.update_one(
                {"_id": ObjectId(billing_id)},
                {"$set": update_data}
            )
        
        updated = await facturas_collection.find_one({"_id": ObjectId(billing_id)})
        return Billing(**updated)
    
    @staticmethod
    async def delete(billing_id: str):
        if not ObjectId.is_valid(billing_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        result = await facturas_collection.update_one(
            {"_id": ObjectId(billing_id), "activo": True},
            {"$set": {"activo": False, "status": "CANCELLED", "metadata.updated_at": datetime.now()}}
        )
        
        if result.matched_count == 0:
            raise HTTPException(status_code=404, detail="Factura no encontrada o ya estaba inactiva")
        
        return {"message": "Factura eliminada (soft delete) y marcada como CANCELLED correctamente"}

    @staticmethod
    async def change_status(billing_id: str, new_status: str, reason: str = None):
        if not ObjectId.is_valid(billing_id):
            raise HTTPException(status_code=400, detail="ID inválido")
            
        update_data = {"status": new_status, "metadata.updated_at": datetime.now()}
        if reason is not None:
            update_data["reason"] = reason
            
        if new_status == "Accepted":
            import random
            import uuid
            
            # Obtener estado de PAC de forma interna
            settings = await BillingController.get_pac_settings()
            pac_mode = settings.get("pac_mode", "SIMULATED")
            timbres_used = settings.get("timbres_used", 0)
            timbres_limit = settings.get("timbres_limit", 5)
            
            is_paid_mode = False
            if pac_mode == "PAID" and timbres_used < timbres_limit:
                is_paid_mode = True
                new_timbres_used = timbres_used + 1
                await pac_settings_collection.update_one(
                    {"_id": "current_settings"},
                    {"$set": {"timbres_used": new_timbres_used}}
                )
                print(f"PAC PAID MODE: Timbre consumido ({new_timbres_used}/{timbres_limit})")

            update_data["fiscal_data.issue_date"] = datetime.now()
            update_data["fiscal_data.certification_date"] = datetime.now()
            update_data["fiscal_data.cfdi_version"] = "4.0"
            update_data["fiscal_data.uuid"] = str(uuid.uuid4()).upper()
            update_data["reason"] = ""  # Limpiar motivo de rechazo previo
            
            if is_paid_mode:
                update_data["fiscal_data.invoice_folio"] = f"PAC-{random.randint(1000, 9999)}"
                update_data["fiscal_data.pac_rfc"] = "FIN1203015JA"
                update_data["fiscal_data.sat_certificate_number"] = "00001000000508881234"
                
                dummy_hash = "PAID_PAC_T0Y0eCtSUEkwN0lXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0c"
                update_data["fiscal_data.digital_seal_issuer"] = dummy_hash[:60] + "..."
                update_data["fiscal_data.digital_seal_sat"] = dummy_hash[20:80] + "..."
                update_data["fiscal_data.original_chain"] = f"||1.1|{update_data['fiscal_data.uuid']}|{datetime.now().isoformat()}|{update_data['fiscal_data.pac_rfc']}|{dummy_hash[:40]}...||"
                update_data["pac_type"] = "PAID"
            else:
                update_data["fiscal_data.invoice_folio"] = f"A-{random.randint(1000, 9999)}"
                update_data["fiscal_data.pac_rfc"] = "SAT970701NN3"
                update_data["fiscal_data.sat_certificate_number"] = "00001000000504465028"
                
                dummy_hash = "T0Y0eCtSUEkwN0lXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0clpXU3p0c"
                update_data["fiscal_data.digital_seal_issuer"] = dummy_hash[:60] + "..."
                update_data["fiscal_data.digital_seal_sat"] = dummy_hash[20:80] + "..."
                update_data["fiscal_data.original_chain"] = f"||1.1|{update_data['fiscal_data.uuid']}|{datetime.now().isoformat()}|{update_data['fiscal_data.pac_rfc']}|{dummy_hash[:40]}...||"
                update_data["pac_type"] = "SIMULATED"

        result = await facturas_collection.update_one(
            {"_id": ObjectId(billing_id), "activo": True},
            {"$set": update_data}
        )
        if result.matched_count == 0:
            raise HTTPException(status_code=404, detail="Factura no encontrada")
            
        updated = await facturas_collection.find_one({"_id": ObjectId(billing_id)})
        return Billing(**updated)

    @staticmethod
    async def upload_file(billing_id: str, file: UploadFile):
        if not ObjectId.is_valid(billing_id):
            raise HTTPException(status_code=400, detail="ID inválido")
            
        if not file.filename.endswith('.pdf'):
            raise HTTPException(status_code=400, detail="El archivo físico debe ser un PDF válido")
            
        
        file_url = f"/storage/invoices/{billing_id}_{file.filename}"
        
        result = await facturas_collection.update_one(
            {"_id": ObjectId(billing_id), "activo": True},
            {"$set": {
                "attachments.pdf_url": file_url,
                "upload_type": "PHYSICAL",
                "metadata.updated_at": datetime.now()
            }}
        )
        if result.matched_count == 0:
            raise HTTPException(status_code=404, detail="Factura no encontrada")
            
        updated = await facturas_collection.find_one({"_id": ObjectId(billing_id)})
        return Billing(**updated)

    @staticmethod
    async def get_ready_to_bill(include_billed: bool = False):

        manifest_url = os.getenv("MANIFEST_API_URL", "http://simar_manifiestos_api:8007")
        client_url = os.getenv("CLIENTS_API_URL", "http://simar_clientes_api:8005")
        contract_url = os.getenv("CONTRACTS_API_URL", "http://simar_contratos_api:8006")

        cursor = facturas_collection.find({"activo": True, "service_id": {"$ne": None}}, {"service_id": 1})
        existing_invoices = await cursor.to_list(length=None)
        billed_service_ids = {inv["service_id"] for inv in existing_invoices} if not include_billed else set()

        async with httpx.AsyncClient() as client:
            try:
                resp = await client.get(f"{manifest_url}/api/manifiestos?estado=completado")
                if resp.status_code != 200:
                    return []
                manifests_data = resp.json().get("data", [])
            except Exception:
                return []
            
            results = []
            for m in manifests_data:
                manifest_id = str(m.get("id"))
                if manifest_id in billed_service_ids:
                    continue
                    
                razon_social = m.get("razon_social")
                
                client_info = None
                try:
                    client_resp = await client.get(f"{client_url}/client/name/{razon_social}")
                    if client_resp.status_code == 200:
                        c = client_resp.json()
                        if c and isinstance(c, dict):
                            client_info = ClientSummarySchema(
                                id=c.get("id"),
                                razon_social=c.get("businessName"),
                                rfc=c.get("rfc"),
                                direccion_fiscal=c.get("address")
                            )
                except Exception:
                    pass
                
                if not client_info:
                    client_info = ClientSummarySchema(
                        id=0,
                        razon_social=razon_social,
                        rfc=None,
                        direccion_fiscal=m.get("domicilio"),
                        postal_code=m.get("codigo_postal") or (re.search(r'(\d{5})(?!\d)', m.get("domicilio") or "").group(1) if re.search(r'(\d{5})(?!\d)', m.get("domicilio") or "") else None)
                    )

                m_detail = {}
                try:
                    detail_resp = await client.get(f"{manifest_url}/api/manifiestos/{m.get('id')}")
                    if detail_resp.status_code == 200:
                        m_detail = detail_resp.json().get("data", {})
                except Exception:
                    pass

                contract_info = None
                contract_services = []
                contrato_id = m_detail.get("contrato_id")
                
                try:
                    target_contract = None
                    if contrato_id:
                        c_resp = await client.get(f"{contract_url}/api/contracts/{contrato_id}/detail")
                        if c_resp.status_code == 200:
                            target_contract = c_resp.json()
                    
                    if not target_contract:
                        c_list_resp = await client.get(f"{contract_url}/api/contracts")
                        if c_list_resp.status_code == 200:
                            contracts = c_list_resp.json()
                            summary = next((c for c in contracts if c.get("clientName") == razon_social), None)
                            if summary:
                                c_resp = await client.get(f"{contract_url}/api/contracts/{summary.get('id')}/detail")
                                if c_resp.status_code == 200:
                                    target_contract = c_resp.json()

                    if target_contract:
                        contract_info = ContractSummarySchema(
                            folio=target_contract.get("folio"),
                            precio_unitario=float(target_contract.get("totalBasePrice") or 0),
                            metodo_pago="PPD",
                            condiciones=target_contract.get("contractDuration")
                        )
                        contract_services = target_contract.get("services", [])
                        
                        if target_contract.get("clientRfc"):
                            client_info.rfc = target_contract.get("clientRfc")
                        if target_contract.get("clientAddress"):
                            client_info.direccion_fiscal = target_contract.get("clientAddress")
                except Exception:
                    pass

                residues = []
                total_estimated = 0.0
                raw_residues = m_detail.get("residuos") or m_detail.get("residuos_especiales") or m_detail.get("residuos_peligrosos") or []
                
                for r in raw_residues:
                    nombre_residuo = r.get("nombre_residuo")
                    cantidad = float(r.get("peso") or r.get("cantidad_kg") or 0)
                    unidad = r.get("unidad") or r.get("capacidad") or r.get("capacidad_envase") or "kg"
                    
                    unit_price = 0.0
                    if contract_services:
                        match = next((s for s in contract_services if s.get("wasteType").lower() in nombre_residuo.lower() or nombre_residuo.lower() in s.get("wasteType").lower()), None)
                        if match:
                            unit_price = float(match.get("subtotal") or 0)
                    
                    if unit_price == 0 and contract_info:
                        unit_price = contract_info.precio_unitario

                    subtotal = cantidad * unit_price
                    residues.append(ResidueDetailSchema(
                        residuo=nombre_residuo,
                        cantidad=cantidad,
                        unidad=unidad,
                        precio_unitario=unit_price,
                        subtotal=subtotal
                    ))
                    total_estimated += subtotal

                fm_raw = m.get("fecha_manifiesto")
                fm_date = datetime.now().date()
                if fm_raw:
                    try:
                        if isinstance(fm_raw, str):
                            if 'T' in fm_raw:
                                fm_date = datetime.fromisoformat(fm_raw.split('Z')[0]).date()
                            else:
                                fm_date = datetime.strptime(fm_raw[:10], '%Y-%m-%d').date()
                        elif isinstance(fm_raw, datetime):
                            fm_date = fm_raw.date()
                    except:
                        pass

                results.append(ReadyToBillSchema(
                    manifest_id=m.get("id"),
                    numero_manifiesto=m.get("numero_manifiesto"),
                    fecha_servicio=fm_date,
                    tipo_residuo=m.get("tipo"),
                    cliente=client_info,
                    contrato=contract_info,
                    detalles_servicio=residues,
                    total_estimado=total_estimated,
                    source="manifest"
                ))

            try:
                contract_services = await BillingController._get_services_from_contracts(billed_service_ids)
                results.extend(contract_services)
            except Exception:
                pass

            return results

    @staticmethod
    async def _get_services_from_contracts(billed_service_ids: set = None):
        contract_url = os.getenv("CONTRACTS_API_URL", "http://simar_contratos_api:8006")
        results = []
        
        if billed_service_ids is None:
            billed_service_ids = set()
            
        async with httpx.AsyncClient() as client:
            try:
                statuses = ["Activo", "Aceptado"]
                all_contracts_data = []
                
                for status in statuses:
                    resp = await client.get(f"{contract_url}/api/contracts?status={status}")
                    if resp.status_code == 200:
                        all_contracts_data.extend(resp.json())
                
                for c_summary in all_contracts_data:
                    contract_id = c_summary.get("id")
                    
                    detail_resp = await client.get(f"{contract_url}/api/contracts/{contract_id}/detail")
                    if detail_resp.status_code != 200:
                        continue
                        
                    c_detail = detail_resp.json()
                    folio = c_detail.get('folio')
                    
                    client_info = ClientSummarySchema(
                        id=c_detail.get("clientId"),
                        razon_social=c_detail.get("clientName"),
                        rfc=c_detail.get("clientRfc"),
                        direccion_fiscal=c_detail.get("clientAddress"),
                        postal_code=re.search(r'(\d{5})(?!\d)', c_detail.get("clientAddress", "")).group(1) if c_detail.get("clientAddress") and re.search(r'(\d{5})(?!\d)', c_detail.get("clientAddress", "")) else None
                    )
                    
                    contract_info = ContractSummarySchema(
                        folio=folio,
                        precio_unitario=float(c_detail.get("totalBasePrice") or 0),
                        metodo_pago="PPD", # Valor por defecto común para contratos
                        condiciones=c_detail.get("contractDuration")
                    )
                    
                    for s in c_detail.get("services", []):
                        waste_type = s.get("wasteType")
                        contract_service_id = f"CONTRACT:{folio}:{waste_type}"
                        
                        if contract_service_id in billed_service_ids:
                            continue

                        fs_raw = c_detail.get("firstServiceDate")
                        fs_date = datetime.now().date()
                        if fs_raw:
                            try:
                                if 'T' in fs_raw:
                                    fs_date = datetime.fromisoformat(fs_raw.split('Z')[0]).date()
                                else:
                                    fs_date = datetime.strptime(fs_raw[:10], '%Y-%m-%d').date()
                            except:
                                pass

                        results.append(ReadyToBillSchema(
                            manifest_id=0,
                            numero_manifiesto=contract_service_id, # Usamos el ID generado como numero_manifiesto para que el front lo use
                            fecha_servicio=fs_date,
                            tipo_residuo=waste_type,
                            cliente=client_info,
                            contrato=contract_info,
                            detalles_servicio=[
                                ResidueDetailSchema(
                                    residuo=waste_type,
                                    cantidad=1.0,
                                    unidad=s.get("wasteUnit"),
                                    precio_unitario=float(s.get("subtotal") or 0),
                                    subtotal=float(s.get("subtotal") or 0)
                                )
                            ],
                            total_estimado=float(s.get("subtotal") or 0),
                            source="contract"
                        ))
            except Exception as e:
                print(f"Error recuperando servicios de contratos: {e}")
                
        return results

    @staticmethod
    async def get_pac_settings():
        settings = await pac_settings_collection.find_one({"_id": "current_settings"})
        if not settings:
            settings = {
                "_id": "current_settings",
                "pac_mode": "PAID",
                "timbres_used": 0,
                "timbres_limit": 5
            }
            await pac_settings_collection.insert_one(settings)
        return settings

    @staticmethod
    async def update_pac_settings(pac_mode: str, timbres_limit: int, timbres_used: int = None):
        update_doc = {
            "pac_mode": pac_mode,
            "timbres_limit": timbres_limit
        }
        if timbres_used is not None:
            update_doc["timbres_used"] = timbres_used
            
        await pac_settings_collection.update_one(
            {"_id": "current_settings"},
            {"$set": update_doc},
            upsert=True
        )
        return await BillingController.get_pac_settings()

    @staticmethod
    async def reset_pac_timbres():
        await pac_settings_collection.update_one(
            {"_id": "current_settings"},
            {"$set": {"timbres_used": 0}},
            upsert=True
        )
        return await BillingController.get_pac_settings()
