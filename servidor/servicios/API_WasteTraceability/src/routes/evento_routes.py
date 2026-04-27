from fastapi import APIRouter, Query
from typing import Optional
from datetime import datetime
from ..controllers.evento_controller import EventoController
from ..schemas.evento_schema import EventoCreate, EventoFilter

router = APIRouter()

@router.get("/")
async def get_all(
    servicioId: Optional[str] = Query(None),
    tipoEvento: Optional[str] = Query(None),
    usuario: Optional[str] = Query(None),
    fechaInicio: Optional[datetime] = Query(None),
    fechaFin: Optional[datetime] = Query(None)
):
    filtro = EventoFilter(
        servicioId=servicioId,
        tipoEvento=tipoEvento,
        usuario=usuario,
        fechaInicio=fechaInicio,
        fechaFin=fechaFin
    )
    eventos = await EventoController.get_all(filtro)
    return {
        "success": True,
        "data": [e.model_dump(by_alias=True) for e in eventos],
        "count": len(eventos)
    }

@router.get("/tipos")
async def get_tipos_evento():
    tipos = await EventoController.get_tipos_evento()
    return {"success": True, "data": tipos}

@router.get("/servicio/{servicioId}")
async def get_by_servicio(servicioId: str):
    eventos = await EventoController.get_by_servicio(servicioId)
    return {
        "success": True,
        "data": [e.model_dump(by_alias=True) for e in eventos],
        "count": len(eventos)
    }

@router.get("/servicio/{servicioId}/resumen")
async def get_resumen_servicio(servicioId: str):
    resumen = await EventoController.get_resumen_servicio(servicioId)
    return {"success": True, "data": resumen}

@router.get("/{evento_id}")
async def get_by_id(evento_id: str):
    evento = await EventoController.get_by_id(evento_id)
    return {"success": True, "data": evento.model_dump(by_alias=True)}

@router.post("/")
async def create(evento: EventoCreate):
    new_evento = await EventoController.create(evento)
    return {"success": True, "data": new_evento.model_dump(by_alias=True)}