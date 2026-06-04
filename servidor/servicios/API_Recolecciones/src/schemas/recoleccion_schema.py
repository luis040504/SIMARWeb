from typing import Optional, List
from pydantic import BaseModel, validator, Field
from datetime import datetime

class VehiculoAsignadoSchema(BaseModel):
    vehiculo: str
    chofer: str
    tecnicos: List[str] = Field(default_factory=list)

class TipoResiduoRecoleccionSchema(BaseModel):
    wasteTypeId: int
    wasteTypeCode: str
    wasteTypeName: str
    wasteType: str
    cantidadEstimada: float = Field(gt=0)
    unidad: str = "kg"
    
    @validator('wasteType')
    def validate_waste_type(cls, v):
        allowed = ['peligroso', 'especial']
        if v not in allowed:
            raise ValueError(f'Tipo de residuo debe ser uno de: {allowed}')
        return v

class RecoleccionCreate(BaseModel):
    idContrato: int
    cliente: str
    fecha: datetime
    direccion: str
    vehiculos: List[VehiculoAsignadoSchema]
    estado: str = "Programada"
    tiposResiduo: Optional[List[TipoResiduoRecoleccionSchema]] = Field(default_factory=list)
    observaciones: Optional[str] = None
    
    @validator('idContrato')
    def validate_id_contrato(cls, v):
        if v <= 0:
            raise ValueError('El ID del contrato debe ser un número positivo')
        return v
    
    @validator('vehiculos')
    def validate_vehiculos(cls, v):
        if not v or len(v) == 0:
            raise ValueError('Debe haber al menos un vehículo asignado')
        for vehiculo in v:
            if len(vehiculo.tecnicos) > 3:
                raise ValueError(f'Máximo 3 técnicos por vehículo. Vehículo {vehiculo.vehiculo} tiene {len(vehiculo.tecnicos)} técnicos')
        return v
    
    @validator('tiposResiduo')
    def validate_tipos_residuo(cls, v):
        if not v or len(v) == 0:
            raise ValueError('Debe haber al menos un tipo de residuo')
        for residuo in v:
            if residuo.cantidadEstimada <= 0:
                raise ValueError(f'La cantidad para {residuo.wasteTypeName} debe ser mayor a 0')
        return v

class RecoleccionUpdate(BaseModel):
    idContrato: Optional[int] = None
    cliente: Optional[str] = None
    fecha: Optional[datetime] = None
    direccion: Optional[str] = None
    vehiculos: Optional[List[VehiculoAsignadoSchema]] = None
    estado: Optional[str] = None
    tiposResiduo: Optional[List[TipoResiduoRecoleccionSchema]] = None
    observaciones: Optional[str] = None
    
    @validator('idContrato')
    def validate_id_contrato(cls, v):
        if v is not None and v <= 0:
            raise ValueError('El ID del contrato debe ser un número positivo')
        return v
    
    @validator('vehiculos')
    def validate_vehiculos(cls, v):
        if v is not None:
            if not v or len(v) == 0:
                raise ValueError('Debe haber al menos un vehículo asignado')
            for vehiculo in v:
                if len(vehiculo.tecnicos) > 3:
                    raise ValueError(f'Máximo 3 técnicos por vehículo. Vehículo {vehiculo.vehiculo} tiene {len(vehiculo.tecnicos)} técnicos')
        return v
    
    @validator('tiposResiduo')
    def validate_tipos_residuo(cls, v):
        if v is not None and len(v) == 0:
            raise ValueError('Debe haber al menos un tipo de residuo')
        return v

class RecoleccionFilter(BaseModel):
    idContrato: Optional[int] = None
    cliente: Optional[str] = None
    fechaInicio: Optional[datetime] = None
    fechaFin: Optional[datetime] = None
    vehiculo: Optional[str] = None
    chofer: Optional[str] = None
    tecnico: Optional[str] = None
    estado: Optional[str] = None
    wasteTypeId: Optional[int] = None