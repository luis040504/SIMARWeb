from typing import Optional, List
from pydantic import BaseModel
from datetime import datetime

class RecoleccionCreate(BaseModel):
    cliente: str
    fecha: datetime
    direccion: str
    vehiculo: str
    tecnico: str
    estado: str = "Programada"
    tipoResiduo: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None

class RecoleccionUpdate(BaseModel):
    cliente: Optional[str] = None
    fecha: Optional[datetime] = None
    direccion: Optional[str] = None
    vehiculo: Optional[str] = None
    tecnico: Optional[str] = None
    estado: Optional[str] = None
    tipoResiduo: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None

class RecoleccionResponse(BaseModel):
    id: str
    cliente: str
    fecha: datetime
    direccion: str
    vehiculo: str
    tecnico: str
    estado: str
    tipoResiduo: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None
    createdAt: datetime
    updatedAt: datetime

class RecoleccionFilter(BaseModel):
    cliente: Optional[str] = None
    fechaInicio: Optional[datetime] = None
    fechaFin: Optional[datetime] = None
    vehiculo: Optional[str] = None
    tecnico: Optional[str] = None
    estado: Optional[str] = None