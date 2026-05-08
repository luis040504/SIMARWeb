from typing import Optional
from pydantic import BaseModel
from datetime import datetime

class ServicioCreate(BaseModel):
    cliente: str
    direccion: str
    fechaServicio: datetime
    estado: str = "Asignado"
    tipoResiduo: str
    contrato: Optional[str] = None
    conductor: Optional[str] = None
    vehiculo: Optional[str] = None
    placa: Optional[str] = None
    tipoVehiculo: Optional[str] = None
    tecnico: Optional[str] = None
    operadorAsignado: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None
    manifiesto: Optional[str] = None
    tipoResiduoTransporte: Optional[str] = None

class ServicioUpdate(BaseModel):
    cliente: Optional[str] = None
    direccion: Optional[str] = None
    fechaServicio: Optional[datetime] = None
    estado: Optional[str] = None
    tipoResiduo: Optional[str] = None
    contrato: Optional[str] = None
    conductor: Optional[str] = None
    vehiculo: Optional[str] = None
    placa: Optional[str] = None
    tipoVehiculo: Optional[str] = None
    tecnico: Optional[str] = None
    operadorAsignado: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None
    manifiesto: Optional[str] = None
    tipoResiduoTransporte: Optional[str] = None
    fechaRecoleccion: Optional[datetime] = None
    fechaTransporte: Optional[datetime] = None
    fechaConclusion: Optional[datetime] = None

class ServicioFilter(BaseModel):
    cliente: Optional[str] = None
    estado: Optional[str] = None
    fechaInicio: Optional[datetime] = None
    fechaFin: Optional[datetime] = None
    contrato: Optional[str] = None
    search: Optional[str] = None