from typing import Optional, Dict, Any
from pydantic import BaseModel
from datetime import datetime
from ..models.evento import Ubicacion

class EventoCreate(BaseModel):
    servicioId: str
    tipoEvento: str
    usuario: str
    estadoAnterior: Optional[str] = None
    estadoNuevo: Optional[str] = None
    ubicacion: Optional[Ubicacion] = None
    observaciones: Optional[str] = None
    metadata: Optional[Dict[str, Any]] = None

class EventoFilter(BaseModel):
    servicioId: Optional[str] = None
    tipoEvento: Optional[str] = None
    usuario: Optional[str] = None
    fechaInicio: Optional[datetime] = None
    fechaFin: Optional[datetime] = None