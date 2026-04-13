from fastapi import HTTPException, status
from datetime import datetime
from bson import ObjectId
from typing import Optional
from ..config.database import eventos_collection
from ..models.evento import EventoTrazabilidad
from ..schemas.evento_schema import EventoCreate, EventoFilter

class EventoController:
    
    @staticmethod
    async def get_all(filtro: Optional[EventoFilter] = None):
        """Obtener todos los eventos con filtros"""
        query = {"activo": True}
        
        if filtro:
            if filtro.servicioId:
                query["servicioId"] = filtro.servicioId
            if filtro.tipoEvento:
                query["tipoEvento"] = filtro.tipoEvento
            if filtro.usuario:
                query["usuario"] = {"$regex": filtro.usuario, "$options": "i"}
            if filtro.fechaInicio or filtro.fechaFin:
                query["fechaEvento"] = {}
                if filtro.fechaInicio:
                    query["fechaEvento"]["$gte"] = filtro.fechaInicio
                if filtro.fechaFin:
                    query["fechaEvento"]["$lte"] = filtro.fechaFin
        
        cursor = eventos_collection.find(query).sort("fechaEvento", -1)
        eventos = await cursor.to_list(length=None)
        return [EventoTrazabilidad(**e) for e in eventos]
    
    @staticmethod
    async def get_by_servicio(servicioId: str):
        """Obtener trazabilidad completa de un servicio"""
        query = {"servicioId": servicioId, "activo": True}
        cursor = eventos_collection.find(query).sort("fechaEvento", 1)
        eventos = await cursor.to_list(length=None)
        return [EventoTrazabilidad(**e) for e in eventos]
    
    @staticmethod
    async def get_by_id(evento_id: str):
        """Obtener evento por ID"""
        if not ObjectId.is_valid(evento_id):
            raise HTTPException(status_code=400, detail="ID inválido")
        
        evento = await eventos_collection.find_one({"_id": ObjectId(evento_id), "activo": True})
        if not evento:
            raise HTTPException(status_code=404, detail="Evento no encontrado")
        
        return EventoTrazabilidad(**evento)
    
    @staticmethod
    async def create(evento_data: EventoCreate):
        """Registrar nuevo evento de trazabilidad"""
        evento_dict = evento_data.model_dump()
        evento_dict["fechaEvento"] = datetime.now()
        evento_dict["createdAt"] = datetime.now()
        evento_dict["activo"] = True
        
        result = await eventos_collection.insert_one(evento_dict)
        new_evento = await eventos_collection.find_one({"_id": result.inserted_id})
        return EventoTrazabilidad(**new_evento)
    
    @staticmethod
    async def get_tipos_evento():
        """Obtener lista de tipos de evento posibles"""
        return [
            "SERVICIO_CREADO",
            "RECOLECCION_CONFIRMADA", 
            "TRANSPORTE_INICIADO", 
            "LLEGADA_REGISTRADA", 
            "ESTADO_CAMBIADO"
        ]
    
    @staticmethod
    async def get_resumen_servicio(servicioId: str):
        """Obtener resumen de trazabilidad de un servicio"""
        eventos = await EventoController.get_by_servicio(servicioId)
        
        resumen = {
            "servicioId": servicioId,
            "totalEventos": len(eventos),
            "primerEvento": eventos[0].fechaEvento if eventos else None,
            "ultimoEvento": eventos[-1].fechaEvento if eventos else None,
            "estadoActual": eventos[-1].estadoNuevo if eventos else None,
            "timeline": [
                {
                    "tipo": e.tipoEvento,
                    "fecha": e.fechaEvento,
                    "usuario": e.usuario,
                    "estado": e.estadoNuevo,
                    "observaciones": e.observaciones
                }
                for e in eventos
            ]
        }
        
        return resumen