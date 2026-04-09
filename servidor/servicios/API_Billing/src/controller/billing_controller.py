from fastapi import HTTPException, status
from datetime import datetime
from bson import ObjectId
from ..config.database import facturas_collection
from ..models.billing import Billing
from ..schemas.billing_schema import BillingCreateSchema, BillingUpdateSchema, BillingFilterSchema

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
            if filtro.issuer_tax_id:
                query["issuer.tax_id"] = {"$regex": filtro.issuer_tax_id, "$options": "i"}
            if filtro.receiver_tax_id:
                query["receiver.tax_id"] = {"$regex": filtro.receiver_tax_id, "$options": "i"}
                
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
