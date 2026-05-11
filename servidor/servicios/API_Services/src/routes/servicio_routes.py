from fastapi import APIRouter, Query
from typing import Optional
from datetime import datetime
from ..controllers.servicio_controller import ServicioController
from ..schemas.servicio_schema import ServicioCreate, ServicioUpdate, ServicioFilter

router = APIRouter()

def normalize_servicio_dict(servicio_dict: dict) -> dict:
    if 'fecha' in servicio_dict and 'fechaServicio' not in servicio_dict:
        servicio_dict['fechaServicio'] = servicio_dict['fecha']
    if 'vehiculos' in servicio_dict and servicio_dict.get('vehiculo') is None:
        first = servicio_dict['vehiculos'][0] if servicio_dict['vehiculos'] else None
        if first:
            servicio_dict['vehiculo'] = first.get('vehiculo')
            servicio_dict['conductor'] = first.get('chofer')
            tecnico = first.get('tecnicos')
            if tecnico:
                servicio_dict['tecnico'] = ', '.join(tecnico)
    return servicio_dict

@router.get("/")
async def get_all(
    cliente: Optional[str] = Query(None),
    estado: Optional[str] = Query(None),
    fechaInicio: Optional[datetime] = Query(None),
    fechaFin: Optional[datetime] = Query(None),
    contrato: Optional[str] = Query(None),
    vehiculo: Optional[str] = Query(None),
    chofer: Optional[str] = Query(None),
    tecnico: Optional[str] = Query(None),
    search: Optional[str] = Query(None)
):
    """Obtener todos los servicios con filtros"""
    filtro = ServicioFilter(
        cliente=cliente,
        estado=estado,
        fechaInicio=fechaInicio,
        fechaFin=fechaFin,
        contrato=contrato,
        vehiculo=vehiculo,
        chofer=chofer,
        tecnico=tecnico,
        search=search
    )
    servicios = await ServicioController.get_all(filtro)
    return {
        "success": True,
        "data": [normalize_servicio_dict(s.model_dump(by_alias=True)) for s in servicios],
        "count": len(servicios)
    }

@router.get("/estados")
async def get_estados():
    """Obtener lista de estados posibles"""
    estados = await ServicioController.get_estados()
    return {"success": True, "data": estados}

@router.get("/cliente/{cliente}")
async def get_by_cliente(
    cliente: str,
    estado: Optional[str] = Query(None)
):
    """Obtener servicios por cliente"""
    servicios = await ServicioController.get_by_cliente(cliente, estado)
    return {
        "success": True,
        "data": [normalize_servicio_dict(s.model_dump(by_alias=True)) for s in servicios],
        "count": len(servicios)
    }

@router.get("/{servicio_id}")
async def get_by_id(servicio_id: str):
    """Obtener servicio por ID"""
    servicio = await ServicioController.get_by_id(servicio_id)
    data = servicio.model_dump(by_alias=True)
    return {"success": True, "data": normalize_servicio_dict(data)}

@router.post("/")
async def create(servicio: ServicioCreate):
    """Crear nuevo servicio"""
    new_servicio = await ServicioController.create(servicio)
    data = new_servicio.model_dump(by_alias=True)
    return {"success": True, "data": normalize_servicio_dict(data)}

@router.put("/{servicio_id}")
async def update(servicio_id: str, servicio: ServicioUpdate):
    """Actualizar servicio"""
    updated = await ServicioController.update(servicio_id, servicio)
    data = updated.model_dump(by_alias=True)
    return {"success": True, "data": normalize_servicio_dict(data)}

@router.delete("/{servicio_id}")
async def delete(servicio_id: str):
    """Eliminar servicio"""
    result = await ServicioController.delete(servicio_id)
    return {"success": True, **result}

@router.post("/{servicio_id}/cancelar-recoleccion")
async def cancelar_recoleccion(servicio_id: str):
    """Cancelar recolección - cambia estado a Cancelada"""
    servicio = await ServicioController.cancelar_recoleccion(servicio_id)
    data = servicio.model_dump(by_alias=True)
    return {
        "success": True,
        "message": "Recolección cancelada exitosamente",
        "data": normalize_servicio_dict(data)
    }

@router.post("/{servicio_id}/iniciar-transporte")
async def iniciar_transporte(servicio_id: str):
    """Iniciar transporte - cambia estado a En ruta"""
    servicio = await ServicioController.iniciar_transporte(servicio_id)
    data = servicio.model_dump(by_alias=True)
    return {
        "success": True,
        "message": "Transporte iniciado exitosamente",
        "data": normalize_servicio_dict(data)
    }

@router.post("/{servicio_id}/registrar-llegada")
async def registrar_llegada(servicio_id: str):
    """Registrar llegada - cambia estado a Completada"""
    servicio = await ServicioController.registrar_llegada(servicio_id)
    data = servicio.model_dump(by_alias=True)
    return {
        "success": True,
        "message": "Llegada registrada exitosamente",
        "data": normalize_servicio_dict(data)
    }

# ============================================
# CATÁLOGOS
# ============================================
@router.get("/catalogos/tipos-desecho")
async def get_tipos_desecho():
    """Obtener lista de tipos de desecho"""
    tipos = [
        "Residuos simples (papel, cartón)",
        "Residuos peligrosos",
        "Residuos biológicos",
        "Residuos electrónicos",
        "Residuos de construcción",
        "Residuos industriales",
        "Residuos orgánicos",
        "Aceites y grasas",
        "Escombros",
        "Plásticos",
        "Residuos varios"
    ]
    return {"success": True, "data": tipos}

@router.get("/catalogos/tipos-gasolina")
async def get_tipos_gasolina():
    """Obtener lista de tipos de gasolina"""
    tipos = ["Magna", "Premium", "Diesel", "Eléctrico", "Híbrido", "Gas LP"]
    return {"success": True, "data": tipos}

@router.get("/catalogos/tipos-licencia")
async def get_tipos_licencia():
    """Obtener lista de tipos de licencia"""
    tipos = [
        {"value": "A", "label": "A - Automóviles"},
        {"value": "B", "label": "B - Camiones ligeros"},
        {"value": "C", "label": "C - Camiones pesados"},
        {"value": "D", "label": "D - Autobuses"},
        {"value": "E", "label": "E - Tractocamiones"}
    ]
    return {"success": True, "data": tipos}