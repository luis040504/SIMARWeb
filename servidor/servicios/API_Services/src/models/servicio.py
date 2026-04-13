from datetime import datetime
from typing import Optional
from pydantic import BaseModel, Field, validator

class Servicio(BaseModel):
    id: Optional[str] = Field(None, alias='_id')
    cliente: str
    direccion: str
    fechaServicio: datetime
    estado: str
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
    fechaRecoleccion: Optional[datetime] = None
    fechaTransporte: Optional[datetime] = None
    fechaConclusion: Optional[datetime] = None
    activo: bool = True
    createdAt: datetime = Field(default_factory=datetime.now)
    updatedAt: datetime = Field(default_factory=datetime.now)
    
    @validator('estado')
    def validate_estado(cls, v):
        allowed = ['Asignado', 'Recolectado', 'En curso', 'Concluido']
        if v not in allowed:
            raise ValueError(f'Estado debe ser uno de: {allowed}')
        return v
    
    @validator('cantidadEstimada')
    def validate_cantidad(cls, v):
        if v is not None and v < 0:
            raise ValueError('La cantidad estimada no puede ser negativa')
        return v
    
    class Config:
        populate_by_name = True
        arbitrary_types_allowed = True
        json_encoders = {
            datetime: lambda v: v.isoformat()
        }