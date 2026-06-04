from fastapi import HTTPException, status
from datetime import datetime
from bson import ObjectId
from typing import List, Optional
from ..config.database import recolecciones_collection
from ..models.recoleccion import Recoleccion
from ..schemas.recoleccion_schema import RecoleccionCreate, RecoleccionUpdate, RecoleccionFilter
from pydantic import ValidationError

class RecoleccionController:
    
    @staticmethod
    async def get_all(filtro: RecoleccionFilter = None):
        """Obtener todas las recolecciones con filtros"""
        query = {"activo": True}
        
        if filtro:
            if filtro.idContrato is not None:
                query["idContrato"] = filtro.idContrato
            if filtro.cliente:
                query["cliente"] = {"$regex": filtro.cliente, "$options": "i"}
            if filtro.estado:
                query["estado"] = filtro.estado
            if filtro.fechaInicio or filtro.fechaFin:
                query["fecha"] = {}
                if filtro.fechaInicio:
                    query["fecha"]["$gte"] = filtro.fechaInicio
                if filtro.fechaFin:
                    query["fecha"]["$lte"] = filtro.fechaFin
            if filtro.vehiculo:
                query["vehiculos.vehiculo"] = {"$regex": filtro.vehiculo, "$options": "i"}
            if filtro.chofer:
                query["vehiculos.chofer"] = {"$regex": filtro.chofer, "$options": "i"}
            if filtro.tecnico:
                query["vehiculos.tecnicos"] = {"$elemMatch": {"$regex": filtro.tecnico, "$options": "i"}}
            if filtro.wasteTypeId is not None:
                query["tiposResiduo.wasteTypeId"] = filtro.wasteTypeId
        
        cursor = recolecciones_collection.find(query).sort("fecha", -1)
        recolecciones = await cursor.to_list(length=None)
        
        for rec in recolecciones:
            rec['_id'] = str(rec['_id'])
        
        return [Recoleccion(**rec) for rec in recolecciones]
    
    @staticmethod
    async def get_by_id(recoleccion_id: str):
        """Obtener recolección por ID"""
        if not ObjectId.is_valid(recoleccion_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        recoleccion = await recolecciones_collection.find_one({"_id": ObjectId(recoleccion_id), "activo": True})
        
        if not recoleccion:
            raise HTTPException(status_code=404, detail="Recolección no encontrada")
        
        recoleccion['_id'] = str(recoleccion['_id'])
        
        return Recoleccion(**recoleccion)
    
    @staticmethod
    async def get_by_contrato(idContrato: int):
        """Obtener recolecciones por ID de contrato"""
        if idContrato <= 0:
            raise HTTPException(status_code=400, detail="ID de contrato inválido")
        
        cursor = recolecciones_collection.find(
            {"idContrato": idContrato, "activo": True}
        ).sort("fecha", -1)
        
        recolecciones = await cursor.to_list(length=None)
        
        for rec in recolecciones:
            rec['_id'] = str(rec['_id'])
        
        return [Recoleccion(**rec) for rec in recolecciones]
    
    @staticmethod
    async def create(recoleccion_data: RecoleccionCreate):
        """Crear nueva recolección"""
        try:
            print("=== CONTROLLER CREATE ===")
            print(f"Datos recibidos: {recoleccion_data}")
            
            # Validar técnicos por vehículo
            for vehiculo in recoleccion_data.vehiculos:
                if len(vehiculo.tecnicos) > 3:
                    raise HTTPException(
                        status_code=422,
                        detail=f"Máximo 3 técnicos por vehículo. Vehículo {vehiculo.vehiculo} tiene {len(vehiculo.tecnicos)} técnicos"
                    )
            
            # Validar tipos de residuo
            if not recoleccion_data.tiposResiduo or len(recoleccion_data.tiposResiduo) == 0:
                raise HTTPException(
                    status_code=422,
                    detail="Debe especificar al menos un tipo de residuo"
                )
            
            # Validar cantidades
            for residuo in recoleccion_data.tiposResiduo:
                if residuo.cantidadEstimada <= 0:
                    raise HTTPException(
                        status_code=422,
                        detail=f"La cantidad para {residuo.wasteTypeName} debe ser mayor a 0"
                    )
            
            recoleccion_dict = recoleccion_data.model_dump()
            recoleccion_dict["createdAt"] = datetime.now()
            recoleccion_dict["updatedAt"] = datetime.now()
            recoleccion_dict["activo"] = True
            
            print(f"Documento a insertar: {recoleccion_dict}")
            
            result = await recolecciones_collection.insert_one(recoleccion_dict)
            
            new_recoleccion = await recolecciones_collection.find_one({"_id": result.inserted_id})
            new_recoleccion['_id'] = str(new_recoleccion['_id'])
            
            print(f"Recolección creada con ID: {result.inserted_id}")
            
            return Recoleccion(**new_recoleccion)
        except HTTPException:
            raise
        except Exception as e:
            print(f"Error en create: {str(e)}")
            import traceback
            traceback.print_exc()
            raise HTTPException(status_code=500, detail=f"Error interno: {str(e)}")
    
    @staticmethod
    async def update(recoleccion_id: str, recoleccion_data: RecoleccionUpdate):
        """Actualizar recolección"""
        if not ObjectId.is_valid(recoleccion_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        existing = await recolecciones_collection.find_one({"_id": ObjectId(recoleccion_id), "activo": True})
        if not existing:
            raise HTTPException(status_code=404, detail="Recolección no encontrada")
        
        if recoleccion_data.vehiculos is not None:
            for vehiculo in recoleccion_data.vehiculos:
                if len(vehiculo.tecnicos) > 3:
                    raise HTTPException(
                        status_code=422,
                        detail=f"Máximo 3 técnicos por vehículo. Vehículo {vehiculo.vehiculo} tiene {len(vehiculo.tecnicos)} técnicos"
                    )
        
        update_data = recoleccion_data.model_dump(exclude_unset=True)
        update_data["updatedAt"] = datetime.now()
        
        if update_data:
            await recolecciones_collection.update_one(
                {"_id": ObjectId(recoleccion_id)},
                {"$set": update_data}
            )
        
        updated = await recolecciones_collection.find_one({"_id": ObjectId(recoleccion_id)})
        updated['_id'] = str(updated['_id'])
        
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