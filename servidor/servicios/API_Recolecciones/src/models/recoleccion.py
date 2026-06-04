from datetime import datetime
from typing import Optional, List
from pydantic import BaseModel, Field, validator, ConfigDict, model_validator
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

class TipoResiduoRecoleccion(BaseModel):
    wasteTypeId: int
    wasteTypeCode: str
    wasteTypeName: str
    wasteType: str
    cantidadEstimada: float = Field(gt=0, description="Cantidad debe ser mayor a 0")
    unidad: str = "kg"
    
    @validator('wasteType')
    def validate_waste_type(cls, v):
        allowed = ['peligroso', 'especial']
        if v not in allowed:
            raise ValueError(f'Tipo de residuo debe ser uno de: {allowed}')
        return v

class Recoleccion(BaseModel):
    id: Optional[str] = Field(None, alias='_id')
    idContrato: int
    cliente: str
    fecha: datetime
    direccion: str
    vehiculos: List[VehiculoAsignado]
    estado: str
    tiposResiduo: Optional[List[TipoResiduoRecoleccion]] = Field(default_factory=list)
    observaciones: Optional[str] = None
    activo: bool = True
    createdAt: datetime = Field(default_factory=datetime.now)
    updatedAt: datetime = Field(default_factory=datetime.now)
    
    # Campos legacy
    tipoResiduo: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    
    @validator('estado')
    def validate_estado(cls, v):
        allowed = ['Programada', 'En ruta', 'Completada', 'Cancelada']
        if v not in allowed:
            raise ValueError(f'Estado debe ser uno de: {allowed}')
        return v
    
    @validator('vehiculos')
    def validate_vehiculos(cls, v):
        if not v or len(v) == 0:
            raise ValueError('Debe haber al menos un vehículo asignado')
        return v
    
    @validator('idContrato')
    def validate_id_contrato(cls, v):
        if v <= 0:
            raise ValueError('El ID del contrato debe ser un número positivo')
        return v
    
    @model_validator(mode='after')
    def validate_tipos_residuo(self):
        # Si no hay tiposResiduo pero hay tipoResiduo legacy, migrar
        if (not self.tiposResiduo or len(self.tiposResiduo) == 0) and self.tipoResiduo:
            self.tiposResiduo = [
                TipoResiduoRecoleccion(
                    wasteTypeId=0,
                    wasteTypeCode="LEGACY",
                    wasteTypeName=self.tipoResiduo,
                    wasteType="especial",
                    cantidadEstimada=self.cantidadEstimada or 0,
                    unidad="ton"
                )
            ]
        
        # Validar que haya al menos un tipo de residuo después de la migración
        if not self.tiposResiduo or len(self.tiposResiduo) == 0:
            raise ValueError('Debe haber al menos un tipo de residuo')
        
        return self
    
    model_config = ConfigDict(
        populate_by_name=True,
        arbitrary_types_allowed=True,
        json_encoders={datetime: lambda v: v.isoformat(), ObjectId: str}
    )