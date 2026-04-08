from datetime import datetime
from typing import Optional
from pydantic import BaseModel, Field, validator

class Recoleccion(BaseModel):
    id: Optional[str] = Field(None, alias='_id')
    cliente: str
    fecha: datetime
    direccion: str
    vehiculo: str
    tecnico: str
    estado: str  
    tipoResiduo: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None
    activo: bool = True
    createdAt: datetime = Field(default_factory=datetime.now)
    updatedAt: datetime = Field(default_factory=datetime.now)
    
    @validator('estado')
    def validate_estado(cls, v):
        allowed = ['Programada', 'En ruta', 'Completada', 'Cancelada']
        if v not in allowed:
            raise ValueError(f'Estado debe ser uno de: {allowed}')
        return v
    
    @validator('cantidadEstimada')
    def validate_cantidad(cls, v):
        if v is not None and v <= 0:
            raise ValueError('La cantidad estimada debe ser mayor a 0')
        return v
    
    class Config:
        populate_by_name = True
        arbitrary_types_allowed = True
        json_encoders = {
            datetime: lambda v: v.isoformat()
        }