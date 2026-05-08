from fastapi import HTTPException, status, UploadFile
from datetime import datetime
from bson import ObjectId
import os
import httpx
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
            
        immutable_states = ["Accepted", "Rejected", "CANCELLED"]
        if existing.get("status") in immutable_states:
            from ..handlers.exceptions import AppException
            raise AppException(message="No se puede editar una factura que ya ha sido procesada o cancelada", status_code=400, code="IMMUTABLE_STATE")
        
        update_data = billing_data.model_dump(exclude_unset=True)
        if "metadata" not in update_data:
            update_data["metadata"] = existing.get("metadata", {})
            update_data["metadata"]["updated_at"] = datetime.now()
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
                    client_resp = await client.get(f"{client_url}/clientes?nombre={razon_social}")
                    if client_resp.status_code == 200:
                        c_list = client_resp.json()
                        if isinstance(c_list, list) and len(c_list) > 0:
                            c = c_list[0]
                            client_info = ClientSummarySchema(
                                id=c.get("id"),
                                razon_social=c.get("nombre"),
                                rfc=c.get("rfc"),
                                direccion_fiscal=c.get("direccion")
                            )
                except Exception:
                    pass
                
                if not client_info:
                    client_info = ClientSummarySchema(
                        id=0,
                        razon_social=razon_social,
                        rfc=None,
                        direccion_fiscal=m.get("domicilio")
                    )

                # 3. Buscar contrato para precios
                contract_info = None
                try:
                    contract_resp = await client.get(f"{contract_url}/contracts")
                    if contract_resp.status_code == 200:
                        contracts = contract_resp.json()
                        target_contract = next((c for c in contracts if c.get("client") == razon_social), None)
                        if target_contract:
                            contract_info = ContractSummarySchema(
                                folio=target_contract.get("folio"),
                                precio_unitario=target_contract.get("price", 0.0),
                                metodo_pago=target_contract.get("paymentMethod"),
                                condiciones=target_contract.get("serviceConditions")
                            )
                except Exception:
                    pass

                # 4. Obtener detalles de residuos del manifiesto
                residues = []
                try:
                    detail_resp = await client.get(f"{manifest_url}/api/manifiestos/{m.get('id')}")
                    if detail_resp.status_code == 200:
                        m_detail = detail_resp.json().get("data", {})
                        raw_residues = m_detail.get("residuos_especiales") or m_detail.get("residuos_peligrosos") or []
                        for r in raw_residues:
                            residues.append(ResidueDetailSchema(
                                residuo=r.get("nombre_residuo"),
                                cantidad=float(r.get("peso") or r.get("cantidad_kg") or 0),
                                unidad=r.get("unidad", "kg")
                            ))
                except Exception:
                    pass

                # Calcular total estimado
                price = contract_info.precio_unitario if contract_info else 0.0
                total_qty = sum(r.cantidad for r in residues)
                total_estimated = total_qty * price

                results.append(ReadyToBillSchema(
                    manifest_id=m.get("id"),
                    numero_manifiesto=m.get("numero_manifiesto"),
                    fecha_servicio=m.get("fecha_manifiesto"),
                    tipo_residuo=m.get("tipo"),
                    cliente=client_info,
                    contrato=contract_info,
                    detalles_servicio=residues,
                    total_estimated=total_estimated
                ))

            return results
