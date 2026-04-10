from fastapi import HTTPException, status
from datetime import datetime
from bson import ObjectId
from ..config.database import recolecciones_collection
from ..models.recoleccion import Recoleccion
from ..schemas.recoleccion_schema import RecoleccionCreate, RecoleccionUpdate, RecoleccionFilter

class RecoleccionController:
    
    @staticmethod
    async def get_all(filtro: RecoleccionFilter = None):
        """Obtener todas las recolecciones con filtros"""
        query = {"activo": True}
        
        if filtro:
            if filtro.cliente:
                query["cliente"] = {"$regex": filtro.cliente, "$options": "i"}
            if filtro.vehiculo:
                query["vehiculo"] = {"$regex": filtro.vehiculo, "$options": "i"}
            if filtro.tecnico:
                query["tecnico"] = {"$regex": filtro.tecnico, "$options": "i"}
            if filtro.estado:
                query["estado"] = filtro.estado
            if filtro.fechaInicio or filtro.fechaFin:
                query["fecha"] = {}
                if filtro.fechaInicio:
                    query["fecha"]["$gte"] = filtro.fechaInicio
                if filtro.fechaFin:
                    query["fecha"]["$lte"] = filtro.fechaFin
        
        cursor = recolecciones_collection.find(query).sort("fecha", -1)
        recolecciones = await cursor.to_list(length=None)
        
        return [Recoleccion(**rec) for rec in recolecciones]
    
    @staticmethod
    async def get_by_id(recoleccion_id: str):
        """Obtener recolección por ID"""
        if not ObjectId.is_valid(recoleccion_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        recoleccion = await recolecciones_collection.find_one({"_id": ObjectId(recoleccion_id), "activo": True})
        
        if not recoleccion:
            raise HTTPException(status_code=404, detail="Recolección no encontrada")
        
        return Recoleccion(**recoleccion)
    
    @staticmethod
    async def create(recoleccion_data: RecoleccionCreate):
        """Crear nueva recolección"""
        recoleccion_dict = recoleccion_data.model_dump()
        recoleccion_dict["createdAt"] = datetime.now()
        recoleccion_dict["updatedAt"] = datetime.now()
        recoleccion_dict["activo"] = True
        
        result = await recolecciones_collection.insert_one(recoleccion_dict)
        
        new_recoleccion = await recolecciones_collection.find_one({"_id": result.inserted_id})
        
        return Recoleccion(**new_recoleccion)
    
    @staticmethod
    async def update(recoleccion_id: str, recoleccion_data: RecoleccionUpdate):
        """Actualizar recolección"""
        if not ObjectId.is_valid(recoleccion_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        # Verificar si existe
        existing = await recolecciones_collection.find_one({"_id": ObjectId(recoleccion_id), "activo": True})
        if not existing:
            raise HTTPException(status_code=404, detail="Recolección no encontrada")
        
        # Preparar datos de actualización
        update_data = recoleccion_data.model_dump(exclude_unset=True)
        update_data["updatedAt"] = datetime.now()
        
        if update_data:
            await recolecciones_collection.update_one(
                {"_id": ObjectId(recoleccion_id)},
                {"$set": update_data}
            )
        
        updated = await recolecciones_collection.find_one({"_id": ObjectId(recoleccion_id)})
        return Recoleccion(**updated)
    
    @staticmethod
    async def delete(recoleccion_id: str):
        """Eliminar recolección (soft delete)"""
        if not ObjectId.is_valid(recoleccion_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        result = await recolecciones_collection.update_one(
            {"_id": ObjectId(recoleccion_id), "activo": True},
            {"$set": {"activo": False, "updatedAt": datetime.now()}}
        )
        
        if result.matched_count == 0:
            raise HTTPException(status_code=404, detail="Recolección no encontrada")
        
        return {"message": "Recolección eliminada correctamente"}
    
    @staticmethod
    async def get_estados():
        """Obtener lista de estados posibles"""
        return ["Programada", "En ruta", "Completada", "Cancelada"]