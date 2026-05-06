from datetime import datetime
from typing import Optional, Dict, Any
from pydantic import BaseModel, Field, validator

class Ubicacion(BaseModel):
    latitud: Optional[float] = None
    longitud: Optional[float] = None
    direccion: Optional[str] = None

class EventoTrazabilidad(BaseModel):
    id: Optional[str] = Field(None, alias='_id')
    servicioId: str
    tipoEvento: str
    fechaEvento: datetime
    usuario: str
    estadoAnterior: Optional[str] = None
    estadoNuevo: Optional[str] = None
    ubicacion: Optional[Ubicacion] = None
    observaciones: Optional[str] = None
    metadata: Optional[Dict[str, Any]] = None
    activo: bool = True
    createdAt: datetime = Field(default_factory=datetime.now)
    
    @validator('tipoEvento')
    def validate_tipo_evento(cls, v):
        allowed = ['SERVICIO_CREADO', 'RECOLECCION_CONFIRMADA', 'TRANSPORTE_INICIADO', 'LLEGADA_REGISTRADA', 'ESTADO_CAMBIADO']
        if v not in allowed:
            raise ValueError(f'Tipo de evento debe ser uno de: {allowed}')
        return v
    
    class Config:
        populate_by_name = True
        arbitrary_types_allowed = True
        json_encoders = {
            datetime: lambda v: v.isoformat()
        }