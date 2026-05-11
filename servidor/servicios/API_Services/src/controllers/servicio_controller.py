from fastapi import HTTPException, status
from datetime import datetime
from bson import ObjectId
from typing import Optional
from ..config.database import recolecciones_collection
from ..models.servicio import Servicio
from ..schemas.servicio_schema import ServicioCreate, ServicioUpdate, ServicioFilter

class ServicioController:
    ACTIVE_FILTER = {"$or": [{"activo": True}, {"activo": {"$exists": False}}]}

    @staticmethod
    async def get_all(filtro: Optional[ServicioFilter] = None):
        """Obtener todos los servicios con filtros"""
        query = ServicioController.ACTIVE_FILTER.copy()
        
        if filtro:
            if filtro.search:
                query = {
                    "$and": [
                        ServicioController.ACTIVE_FILTER,
                        {
                            "$or": [
                                {"cliente": {"$regex": filtro.search, "$options": "i"}},
                                {"direccion": {"$regex": filtro.search, "$options": "i"}},
                                {"tipoResiduo": {"$regex": filtro.search, "$options": "i"}},
                                {"observaciones": {"$regex": filtro.search, "$options": "i"}}
                            ]
                        }
                    ]
                }
            else:
                if filtro.cliente:
                    query["cliente"] = {"$regex": filtro.cliente, "$options": "i"}
                if filtro.estado:
                    query["estado"] = filtro.estado
                if filtro.contrato:
                    query["contrato"] = {"$regex": filtro.contrato, "$options": "i"}
                elem_match = {}
                if filtro.vehiculo:
                    elem_match["vehiculo"] = {"$regex": filtro.vehiculo, "$options": "i"}
                if filtro.chofer:
                    elem_match["chofer"] = {"$regex": filtro.chofer, "$options": "i"}
                if filtro.tecnico:
                    elem_match["tecnicos"] = {"$regex": filtro.tecnico, "$options": "i"}
                if elem_match:
                    query["vehiculos"] = {"$elemMatch": elem_match}
            
            if filtro.fechaInicio or filtro.fechaFin:
                query["fecha"] = {}
                if filtro.fechaInicio:
                    query["fecha"]["$gte"] = filtro.fechaInicio
                if filtro.fechaFin:
                    query["fecha"]["$lte"] = filtro.fechaFin
        
        cursor = recolecciones_collection.find(query).sort("fecha", -1)
        servicios = await cursor.to_list(length=None)
        return [Servicio(**s) for s in servicios]
    
    @staticmethod
    async def get_by_id(servicio_id: str):
        """Obtener servicio por ID"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        servicio = await recolecciones_collection.find_one({"_id": ObjectId(servicio_id), **ServicioController.ACTIVE_FILTER})
        if not servicio:
            raise HTTPException(status_code=404, detail="Servicio no encontrado")
        
        return Servicio(**servicio)
    
    @staticmethod
    async def get_by_cliente(cliente: str, estado: Optional[str] = None):
        """Obtener servicios por cliente"""
        query = {**ServicioController.ACTIVE_FILTER, "cliente": {"$regex": cliente, "$options": "i"}}
        if estado:
            query["estado"] = estado
        
        cursor = recolecciones_collection.find(query).sort("fecha", -1)
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
            count = await recolecciones_collection.count_documents({})
            servicio_dict["manifiesto"] = f"MAN-{año}-{count+1:03d}"
        
        result = await recolecciones_collection.insert_one(servicio_dict)
        new_servicio = await recolecciones_collection.find_one({"_id": result.inserted_id})
        return Servicio(**new_servicio)
    
    @staticmethod
    async def update(servicio_id: str, servicio_data: ServicioUpdate):
        """Actualizar servicio"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        existing = await recolecciones_collection.find_one({"_id": ObjectId(servicio_id), **ServicioController.ACTIVE_FILTER})
        if not existing:
            raise HTTPException(status_code=404, detail="Servicio no encontrado")
        
        update_data = servicio_data.model_dump(exclude_unset=True)
        update_data["updatedAt"] = datetime.now()
        
        if update_data:
            await recolecciones_collection.update_one(
                {"_id": ObjectId(servicio_id)},
                {"$set": update_data}
            )
        
        updated = await recolecciones_collection.find_one({"_id": ObjectId(servicio_id)})
        return Servicio(**updated)
    
    @staticmethod
    async def delete(servicio_id: str):
        """Eliminar servicio (soft delete)"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        result = await recolecciones_collection.update_one(
            {"_id": ObjectId(servicio_id), **ServicioController.ACTIVE_FILTER},
            {"$set": {"activo": False, "updatedAt": datetime.now()}}
        )
        
        if result.matched_count == 0:
            raise HTTPException(status_code=404, detail="Servicio no encontrado")
        
        return {"message": "Servicio eliminado correctamente"}
    
    @staticmethod
    async def get_estados():
        """Obtener lista de estados posibles"""
        return ["Programada", "En ruta", "Completada", "Cancelada"]
    
    @staticmethod
    async def cancelar_recoleccion(servicio_id: str):
        """Cambiar estado a Cancelada"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        await recolecciones_collection.update_one(
            {"_id": ObjectId(servicio_id)},
            {"$set": {
                "estado": "Cancelada", 
                "updatedAt": datetime.now()
            }}
        )
        
        updated = await recolecciones_collection.find_one({"_id": ObjectId(servicio_id)})
        return Servicio(**updated)
    
    @staticmethod
    async def iniciar_transporte(servicio_id: str):
        """Cambiar estado a En ruta"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        await recolecciones_collection.update_one(
            {"_id": ObjectId(servicio_id)},
            {"$set": {
                "estado": "En ruta", 
                "updatedAt": datetime.now()
            }}
        )
        
        updated = await recolecciones_collection.find_one({"_id": ObjectId(servicio_id)})
        return Servicio(**updated)
    
    @staticmethod
    async def registrar_llegada(servicio_id: str):
        """Cambiar estado a Completada"""
        if not ObjectId.is_valid(servicio_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        await recolecciones_collection.update_one(
            {"_id": ObjectId(servicio_id)},
            {"$set": {
                "estado": "Completada", 
                "updatedAt": datetime.now()
            }}
        )
        
        updated = await recolecciones_collection.find_one({"_id": ObjectId(servicio_id)})
        return Servicio(**updated)