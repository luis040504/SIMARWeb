from datetime import datetime
from typing import Optional, List
from pydantic import BaseModel, Field, validator, ConfigDict
from bson import ObjectId

class PyObjectId(ObjectId):
    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, v):
        if not ObjectId.is_valid(v):
            raise ValueError('ID inválido')
        return str(v)

class VehiculoAsignado(BaseModel):
    vehiculo: str
    chofer: str
    tecnicos: List[str] = Field(default_factory=list)
    
    @validator('tecnicos')
    def validate_tecnicos(cls, v):
        if len(v) > 3:
            raise ValueError('Máximo 3 técnicos por vehículo')
        return v

class Recoleccion(BaseModel):
    id: Optional[str] = Field(None, alias='_id')
    cliente: str
    fecha: datetime
    direccion: str
    vehiculos: List[VehiculoAsignado]
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
    
    @validator('vehiculos')
    def validate_vehiculos(cls, v):
        if not v or len(v) == 0:
            raise ValueError('Debe haber al menos un vehículo asignado')
        return v
    
    model_config = ConfigDict(
        populate_by_name=True,
        arbitrary_types_allowed=True,
        json_encoders={datetime: lambda v: v.isoformat(), ObjectId: str}
    )