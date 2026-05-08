from fastapi import HTTPException, status
from datetime import datetime
from bson import ObjectId
from typing import Optional
from ..config.database import servicios_collection
from ..models.servicio import Servicio
from ..schemas.servicio_schema import ServicioCreate, ServicioUpdate, ServicioFilter

class ServicioController:
    
    @staticmethod
    async def get_all(filtro: Optional[ServicioFilter] = None):
        """Obtener todos los servicios con filtros"""
        query = {"activo": True}
        
        if filtro:
            if filtro.search:
                query["$or"] = [
                    {"cliente": {"$regex": filtro.search, "$options": "i"}},
                    {"direccion": {"$regex": filtro.search, "$options": "i"}},
                    {"contrato": {"$regex": filtro.search, "$options": "i"}},
                    {"manifiesto": {"$regex": filtro.search, "$options": "i"}}
                ]
            else:
                if filtro.cliente:
                    query["cliente"] = {"$regex": filtro.cliente, "$options": "i"}
                if filtro.estado:
                    query["estado"] = filtro.estado
                if filtro.contrato:
                    query["contrato"] = {"$regex": filtro.contrato, "$options": "i"}
            
            if filtro.fechaInicio or filtro.fechaFin:
                query["fechaServicio"] = {}
                if filtro.fechaInicio:
                    query["fechaServicio"]["$gte"] = filtro.fechaInicio
                if filtro.fechaFin:
                    query["fechaServicio"]["$lte"] = filtro.fechaFin
        
        cursor = servicios_collection.find(query).sort("fechaServicio", -1)
        servicios = await cursor.to_list(length=None)
        return [Servicio(**s) for s in servicios]
    
    @staticmethod
    async def get_by_id(servicio_id: str):
        """Obtener servicio por ID"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        servicio = await servicios_collection.find_one({"_id": ObjectId(servicio_id), "activo": True})
        if not servicio:
            raise HTTPException(status_code=404, detail="Servicio no encontrado")
        
        return Servicio(**servicio)
    
    @staticmethod
    async def get_by_cliente(cliente: str, estado: Optional[str] = None):
        """Obtener servicios por cliente"""
        query = {"cliente": {"$regex": cliente, "$options": "i"}, "activo": True}
        if estado:
            query["estado"] = estado
        
        cursor = servicios_collection.find(query).sort("fechaServicio", -1)
        servicios = await cursor.to_list(length=None)
        return [Servicio(**s) for s in servicios]
    
    @staticmethod
    async def create(servicio_data: ServicioCreate):
        """Crear nuevo servicio"""
        servicio_dict = servicio_data.model_dump()
        servicio_dict["createdAt"] = datetime.now()
        servicio_dict["updatedAt"] = datetime.now()
        servicio_dict["activo"] = True
        
        # Generar manifiesto si no existe
        if not servicio_dict.get("manifiesto"):
            año = datetime.now().year
            count = await servicios_collection.count_documents({})
            servicio_dict["manifiesto"] = f"MAN-{año}-{count+1:03d}"
        
        result = await servicios_collection.insert_one(servicio_dict)
        new_servicio = await servicios_collection.find_one({"_id": result.inserted_id})
        return Servicio(**new_servicio)
    
    @staticmethod
    async def update(servicio_id: str, servicio_data: ServicioUpdate):
        """Actualizar servicio"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        existing = await servicios_collection.find_one({"_id": ObjectId(servicio_id), "activo": True})
        if not existing:
            raise HTTPException(status_code=404, detail="Servicio no encontrado")
        
        update_data = servicio_data.model_dump(exclude_unset=True)
        update_data["updatedAt"] = datetime.now()
        
        if update_data:
            await servicios_collection.update_one(
                {"_id": ObjectId(servicio_id)},
                {"$set": update_data}
            )
        
        updated = await servicios_collection.find_one({"_id": ObjectId(servicio_id)})
        return Servicio(**updated)
    
    @staticmethod
    async def delete(servicio_id: str):
        """Eliminar servicio (soft delete)"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        result = await servicios_collection.update_one(
            {"_id": ObjectId(servicio_id), "activo": True},
            {"$set": {"activo": False, "updatedAt": datetime.now()}}
        )
        
        if result.matched_count == 0:
            raise HTTPException(status_code=404, detail="Servicio no encontrado")
        
        return {"message": "Servicio eliminado correctamente"}
    
    @staticmethod
    async def get_estados():
        """Obtener lista de estados posibles"""
        return ["Asignado", "Recolectado", "En curso", "Concluido"]
    
    @staticmethod
    async def confirmar_recoleccion(servicio_id: str):
        """Cambiar estado a Recolectado"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        await servicios_collection.update_one(
            {"_id": ObjectId(servicio_id)},
            {"$set": {
                "estado": "Recolectado", 
                "fechaRecoleccion": datetime.now(),
                "updatedAt": datetime.now()
            }}
        )
        
        updated = await servicios_collection.find_one({"_id": ObjectId(servicio_id)})
        return Servicio(**updated)
    
    @staticmethod
    async def iniciar_transporte(servicio_id: str):
        """Cambiar estado a En curso"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        await servicios_collection.update_one(
            {"_id": ObjectId(servicio_id)},
            {"$set": {
                "estado": "En curso", 
                "fechaTransporte": datetime.now(),
                "updatedAt": datetime.now()
            }}
        )
        
        updated = await servicios_collection.find_one({"_id": ObjectId(servicio_id)})
        return Servicio(**updated)
    
    @staticmethod
    async def registrar_llegada(servicio_id: str):
        """Cambiar estado a Concluido"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        await servicios_collection.update_one(
            {"_id": ObjectId(servicio_id)},
            {"$set": {
                "estado": "Concluido", 
                "fechaConclusion": datetime.now(),
                "updatedAt": datetime.now()
            }}
        )
        
        updated = await servicios_collection.find_one({"_id": ObjectId(servicio_id)})
        return Servicio(**updated)