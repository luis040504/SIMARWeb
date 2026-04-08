from fastapi import APIRouter, Query, Depends
from typing import Optional
from datetime import datetime
from ..controllers.recoleccion_controller import RecoleccionController
from ..schemas.recoleccion_schema import RecoleccionCreate, RecoleccionUpdate, RecoleccionFilter

router = APIRouter()

@router.get("/")
async def get_all(
    cliente: Optional[str] = Query(None),
    fechaInicio: Optional[datetime] = Query(None),
    fechaFin: Optional[datetime] = Query(None),
    vehiculo: Optional[str] = Query(None),
    tecnico: Optional[str] = Query(None),
    estado: Optional[str] = Query(None)
):
    """Obtener todas las recolecciones con filtros"""
    filtro = RecoleccionFilter(
        cliente=cliente,
        fechaInicio=fechaInicio,
        fechaFin=fechaFin,
        vehiculo=vehiculo,
        tecnico=tecnico,
        estado=estado
    )
    recolecciones = await RecoleccionController.get_all(filtro)
    return {
        "success": True,
        "data": [r.model_dump(by_alias=True) for r in recolecciones],
        "count": len(recolecciones)
    }

@router.get("/estados")
async def get_estados():
    """Obtener lista de estados posibles"""
    estados = await RecoleccionController.get_estados()
    return {"success": True, "data": estados}

@router.get("/{recoleccion_id}")
async def get_by_id(recoleccion_id: str):
    """Obtener recolección por ID"""
    recoleccion = await RecoleccionController.get_by_id(recoleccion_id)
    return {"success": True, "data": recoleccion.model_dump(by_alias=True)}

@router.post("/")
async def create(recoleccion: RecoleccionCreate):
    """Crear nueva recolección"""
    new_recoleccion = await RecoleccionController.create(recoleccion)
    return {"success": True, "data": new_recoleccion.model_dump(by_alias=True)}

@router.put("/{recoleccion_id}")
async def update(recoleccion_id: str, recoleccion: RecoleccionUpdate):
    """Actualizar recolección"""
    updated = await RecoleccionController.update(recoleccion_id, recoleccion)
    return {"success": True, "data": updated.model_dump(by_alias=True)}

@router.delete("/{recoleccion_id}")
async def delete(recoleccion_id: str):
    """Eliminar recolección"""
    result = await RecoleccionController.delete(recoleccion_id)
    return {"success": True, **result}