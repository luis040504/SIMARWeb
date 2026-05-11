from fastapi import HTTPException, status, UploadFile
from datetime import datetime
from bson import ObjectId
import os
import httpx
import re
from ..config.database import facturas_collection
from ..models.billing import Billing
from ..schemas.billing_schema import BillingCreateSchema, BillingUpdateSchema, BillingFilterSchema
from ..schemas.aggregator_schema import ReadyToBillSchema, ClientSummarySchema, ContractSummarySchema, ResidueDetailSchema

class BillingController:
    
    @staticmethod
    async def get_all(filtro: BillingFilterSchema = None):
        """Obtener todas las facturas con filtros"""
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
        """Obtener facturas por ID de cliente"""
        cursor = facturas_collection.find({"receiver.client_id": client_id, "activo": True}).sort("metadata.created_at", -1)
        facturas = await cursor.to_list(length=None)
        return [Billing(**fac) for fac in facturas]
    
    @staticmethod
    async def get_by_id(billing_id: str):
        """Obtener factura por ID (incluso inactivas si se sabe el ID)"""
        if not ObjectId.is_valid(billing_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        factura = await facturas_collection.find_one({"_id": ObjectId(billing_id)})
        
        if not factura:
            raise HTTPException(status_code=404, detail="Factura no encontrada")
        
        return Billing(**factura)
    
    @staticmethod
    async def create(billing_data: BillingCreateSchema):
        """Crear nueva factura"""
        
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
        
        new_billing = await facturas_collection.find_one({"_id": result.inserted_id})
        
        return Billing(**new_billing)
    
    @staticmethod
    async def update(billing_id: str, billing_data: BillingUpdateSchema):
        """Actualizar factura"""
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
        
        # Manejo robusto de metadata para asegurar que updated_at siempre se actualice
        if update_data.get("metadata") is None:
            # Si no viene metadata o es null, usamos la existente y actualizamos el timestamp
            current_metadata = existing.get("metadata", {})
            current_metadata["updated_at"] = datetime.now()
            update_data["metadata"] = current_metadata
        else:
            # Si viene metadata, nos aseguramos de refrescar updated_at
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
        """Eliminar factura (soft delete)"""
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
        """Actualizar únicamente el status y la razón (ideal para Aceptar/Rechazar)"""
        if not ObjectId.is_valid(billing_id):
            raise HTTPException(status_code=400, detail="ID inválido")
            
        update_data = {"status": new_status, "metadata.updated_at": datetime.now()}
        if reason is not None:
            update_data["reason"] = reason
            
        if new_status == "Accepted":
            import random
            import uuid
            # Simular timbrado fiscal si no tiene datos
            update_data["fiscal_data.invoice_folio"] = f"A-{random.randint(1000, 9999)}"
            update_data["fiscal_data.uuid"] = str(uuid.uuid4()).upper()
            update_data["fiscal_data.issue_date"] = datetime.now()
            update_data["reason"] = ""  # Limpiar motivo de rechazo previo

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
        """Manejar la subida del documento físico"""
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
    async def get_ready_to_bill():
        """
        Orquesta llamadas a Manifiestos, Clientes y Contratos para obtener
        servicios listos para facturar.
        """
        manifest_url = os.getenv("MANIFEST_API_URL", "http://simar_manifiestos_api:8007")
        client_url = os.getenv("CLIENTS_API_URL", "http://simar_clientes_api:8005")
        contract_url = os.getenv("CONTRACTS_API_URL", "http://simar_contratos_api:8006")

        async with httpx.AsyncClient() as client:
            # 1. Obtener manifiestos completados
            try:
                resp = await client.get(f"{manifest_url}/api/manifiestos?estado=completado")
                if resp.status_code != 200:
                    return []
                manifests_data = resp.json().get("data", [])
            except Exception:
                return []
            
            results = []
            for m in manifests_data:
                razon_social = m.get("razon_social")
                
                # 2. Buscar cliente por razón social
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
                        postal_code=m.get("codigo_postal") or (re.search(r'(\d{5})(?!\d)', m.get("domicilio", "")).group(1) if re.search(r'(\d{5})(?!\d)', m.get("domicilio", "")) else None)
                    )

                # 3. Obtener detalle del manifiesto para tener contrato_id y residuos
                m_detail = {}
                try:
                    detail_resp = await client.get(f"{manifest_url}/api/manifiestos/{m.get('id')}")
                    if detail_resp.status_code == 200:
                        m_detail = detail_resp.json().get("data", {})
                except Exception:
                    pass

                # 4. Buscar contrato para precios (usando contrato_id si existe, si no por nombre de cliente)
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
                        # Fallback: buscar por nombre de cliente
                        c_list_resp = await client.get(f"{contract_url}/api/contracts")
                        if c_list_resp.status_code == 200:
                            contracts = c_list_resp.json()
                            # El campo es clientName en la lista de contratos
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
                except Exception:
                    pass

                # 5. Mapear residuos y calcular subtotales usando el contrato
                residues = []
                total_estimated = 0.0
                raw_residues = m_detail.get("residuos") or m_detail.get("residuos_especiales") or m_detail.get("residuos_peligrosos") or []
                
                for r in raw_residues:
                    nombre_residuo = r.get("nombre_residuo")
                    cantidad = float(r.get("peso") or r.get("cantidad_kg") or 0)
                    unidad = r.get("unidad") or r.get("capacidad") or r.get("capacidad_envase") or "kg"
                    
                    # Buscar precio en el contrato para este tipo de residuo
                    unit_price = 0.0
                    if contract_services:
                        # Coincidencia por nombre de residuo
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

                results.append(ReadyToBillSchema(
                    manifest_id=m.get("id"),
                    numero_manifiesto=m.get("numero_manifiesto"),
                    fecha_servicio=m.get("fecha_manifiesto"),
                    tipo_residuo=m.get("tipo"),
                    cliente=client_info,
                    contrato=contract_info,
                    detalles_servicio=residues,
                    total_estimado=total_estimated,
                    source="manifest"
                ))

            # 5. Obtener servicios directos de contratos activos/aceptados
            try:
                contract_services = await BillingController._get_services_from_contracts()
                results.extend(contract_services)
            except Exception:
                pass

            return results

    @staticmethod
    async def _get_services_from_contracts():
        """Recupera servicios de contratos activos o aceptados"""
        contract_url = os.getenv("CONTRACTS_API_URL", "http://simar_contratos_api:8006")
        results = []
        
        async with httpx.AsyncClient() as client:
            try:
                # Obtener contratos con status Activo o Aceptado
                statuses = ["Activo", "Aceptado"]
                all_contracts_data = []
                
                for status in statuses:
                    resp = await client.get(f"{contract_url}/api/contracts?status={status}")
                    if resp.status_code == 200:
                        all_contracts_data.extend(resp.json())
                
                for c_summary in all_contracts_data:
                    contract_id = c_summary.get("id")
                    
                    # Obtener detalle completo para tener los servicios
                    detail_resp = await client.get(f"{contract_url}/api/contracts/{contract_id}/detail")
                    if detail_resp.status_code != 200:
                        continue
                        
                    c_detail = detail_resp.json()
                    
                    client_info = ClientSummarySchema(
                        id=c_detail.get("clientId"),
                        razon_social=c_detail.get("clientName"),
                        rfc=c_detail.get("clientRfc"),
                        direccion_fiscal=c_detail.get("clientAddress"),
                        postal_code=re.search(r'(\d{5})(?!\d)', c_detail.get("clientAddress", "")).group(1) if c_detail.get("clientAddress") and re.search(r'(\d{5})(?!\d)', c_detail.get("clientAddress", "")) else None
                    )
                    
                    contract_info = ContractSummarySchema(
                        folio=c_detail.get("folio"),
                        precio_unitario=float(c_detail.get("totalBasePrice") or 0),
                        metodo_pago="PPD", # Valor por defecto común para contratos
                        condiciones=c_detail.get("contractDuration")
                    )
                    
                    # Mapear los servicios definidos en el contrato
                    for s in c_detail.get("services", []):
                        results.append(ReadyToBillSchema(
                            manifest_id=0,
                            numero_manifiesto=f"CONTRATO: {c_detail.get('folio')}",
                            fecha_servicio=c_detail.get("firstServiceDate") or datetime.now().date(),
                            tipo_residuo=s.get("wasteType"),
                            cliente=client_info,
                            contrato=contract_info,
                            detalles_servicio=[
                                ResidueDetailSchema(
                                    residuo=s.get("wasteType"),
                                    cantidad=1.0,
                                    unidad=s.get("wasteUnit")
                                )
                            ],
                            total_estimado=float(s.get("subtotal") or 0),
                            source="contract"
                        ))
            except Exception as e:
                print(f"Error recuperando servicios de contratos: {e}")
                
        return results
