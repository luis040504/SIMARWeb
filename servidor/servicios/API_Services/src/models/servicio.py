from datetime import datetime
from typing import Optional, List
from pydantic import BaseModel, Field, validator
from bson import ObjectId

class PyObjectId(ObjectId):
    """Validador personalizado para ObjectId de MongoDB"""
    @classmethod
    def __get_validators__(cls):
        yield cls.validate

    @classmethod
    def validate(cls, v, handler=None):
        if isinstance(v, ObjectId):
            return str(v)
        if isinstance(v, str) and ObjectId.is_valid(v):
            return v
        raise ValueError(f"ObjectId inválido: {v}")
    
    @classmethod
    def __get_pydantic_json_schema__(cls, field_schema):
        field_schema.update(type="string")

class VehiculoAsignadoSchema(BaseModel):
    vehiculo: str
    chofer: str
    tecnicos: List[str] = Field(default_factory=list)

    @validator('tecnicos', pre=True, always=True)
    def validate_tecnicos(cls, v):
        return v or []

class Servicio(BaseModel):
    id: Optional[str] = Field(None, alias='_id')
    cliente: str
    direccion: str
    fecha: datetime
    estado: str
    tipoResiduo: Optional[str] = None
    vehiculos: Optional[List[VehiculoAsignadoSchema]] = Field(default_factory=list)
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None
    activo: Optional[bool] = None
    createdAt: Optional[datetime] = None
    updatedAt: Optional[datetime] = None

    @validator('id', pre=True)
    def convert_objectid(cls, v):
        """Convierte ObjectId a string automáticamente"""
        if isinstance(v, ObjectId):
            return str(v)
        return v
    
    @validator('estado')
    def validate_estado(cls, v):
        allowed = ['Programada', 'En ruta', 'Completada', 'Cancelada']
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
        extra = 'allow'
        json_encoders = {
            ObjectId: str,
            datetime: lambda v: v.isoformat()
        }