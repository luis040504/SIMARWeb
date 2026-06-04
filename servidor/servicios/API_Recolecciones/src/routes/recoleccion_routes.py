from fastapi import APIRouter, Query, Depends, HTTPException
from typing import Optional
from datetime import datetime
from ..controllers.recoleccion_controller import RecoleccionController
from ..schemas.recoleccion_schema import RecoleccionCreate, RecoleccionUpdate, RecoleccionFilter
from pydantic import ValidationError

router = APIRouter()

@router.get("/")
async def get_all(
    idContrato: Optional[int] = Query(None, description="Filtrar por ID de contrato"),
    cliente: Optional[str] = Query(None),
    fechaInicio: Optional[datetime] = Query(None),
    fechaFin: Optional[datetime] = Query(None),
    vehiculo: Optional[str] = Query(None, description="Filtrar por vehículo"),
    chofer: Optional[str] = Query(None, description="Filtrar por chofer"),
    tecnico: Optional[str] = Query(None, description="Filtrar por técnico"),
    estado: Optional[str] = Query(None),
    wasteTypeId: Optional[int] = Query(None, description="Filtrar por tipo de residuo")
):
    """Obtener todas las recolecciones con filtros"""
    filtro = RecoleccionFilter(
        idContrato=idContrato,
        cliente=cliente,
        fechaInicio=fechaInicio,
        fechaFin=fechaFin,
        vehiculo=vehiculo,
        chofer=chofer,
        tecnico=tecnico,
        estado=estado,
        wasteTypeId=wasteTypeId
    )
    recolecciones = await RecoleccionController.get_all(filtro)
    return {
        "success": True,
        "data": [r.model_dump(by_alias=True) for r in recolecciones],
        "count": len(recolecciones)
    }

@router.get("/contrato/{idContrato}")
async def get_by_contrato(idContrato: int):
    """Obtener recolecciones por ID de contrato"""
    recolecciones = await RecoleccionController.get_by_contrato(idContrato)
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
    try:
        print("=== DATOS RECIBIDOS EN EL BACKEND ===")
        print(f"idContrato: {recoleccion.idContrato}")
        print(f"cliente: {recoleccion.cliente}")
        print(f"fecha: {recoleccion.fecha}")
        print(f"direccion: {recoleccion.direccion}")
        print(f"estado: {recoleccion.estado}")
        print(f"vehiculos: {recoleccion.vehiculos}")
        print(f"tiposResiduo: {recoleccion.tiposResiduo}")
        print(f"observaciones: {recoleccion.observaciones}")
        print("=====================================")
        
        new_recoleccion = await RecoleccionController.create(recoleccion)
        return {"success": True, "data": new_recoleccion.model_dump(by_alias=True)}
    except HTTPException as e:
        raise e
    except ValidationError as e:
        print(f"Error de validación Pydantic: {e.errors()}")
        raise HTTPException(
            status_code=422, 
            detail={
                "success": False, 
                "message": "Error de validación", 
                "errors": e.errors()
            }
        )
    except Exception as e:
        print(f"Error inesperado: {str(e)}")
        import traceback
        traceback.print_exc()
        raise HTTPException(
            status_code=500, 
            detail={
                "success": False, 
                "message": f"Error interno: {str(e)}"
            }
        )

@router.put("/{recoleccion_id}")
async def update(recoleccion_id: str, recoleccion: RecoleccionUpdate):
    """Actualizar recolección"""
    try:
        updated = await RecoleccionController.update(recoleccion_id, recoleccion)
        return {"success": True, "data": updated.model_dump(by_alias=True)}
    except Exception as e:
        print(f"Error en update: {str(e)}")
        raise HTTPException(status_code=500, detail={"success": False, "message": str(e)})

@router.delete("/{recoleccion_id}")
async def delete(recoleccion_id: str):
    """Eliminar recolección"""
    result = await RecoleccionController.delete(recoleccion_id)
    return {"success": True, **result}