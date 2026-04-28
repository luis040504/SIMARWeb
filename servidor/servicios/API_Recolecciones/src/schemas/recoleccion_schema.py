from typing import Optional, List
from pydantic import BaseModel, validator, Field
from datetime import datetime

class VehiculoAsignadoSchema(BaseModel):
    """Schema para vehículo asignado"""
    vehiculo: str
    chofer: str
    tecnicos: List[str] = Field(default_factory=list)

class RecoleccionCreate(BaseModel):
    cliente: str
    fecha: datetime
    direccion: str
    vehiculos: List[VehiculoAsignadoSchema]
    estado: str = "Programada"
    tipoResiduo: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None
    
    @validator('vehiculos')
    def validate_vehiculos(cls, v):
        if not v or len(v) == 0:
            raise ValueError('Debe haber al menos un vehículo asignado')
        for vehiculo in v:
            if len(vehiculo.tecnicos) > 3:
                raise ValueError(f'Máximo 3 técnicos por vehículo. Vehículo {vehiculo.vehiculo} tiene {len(vehiculo.tecnicos)} técnicos')
        return v

class RecoleccionUpdate(BaseModel):
    cliente: Optional[str] = None
    fecha: Optional[datetime] = None
    direccion: Optional[str] = None
    vehiculos: Optional[List[VehiculoAsignadoSchema]] = None
    estado: Optional[str] = None
    tipoResiduo: Optional[str] = None
    cantidadEstimada: Optional[float] = None
    observaciones: Optional[str] = None
    
    @validator('vehiculos')
    def validate_vehiculos(cls, v):
        if v is not None:
            if not v or len(v) == 0:
                raise ValueError('Debe haber al menos un vehículo asignado')
            for vehiculo in v:
                if len(vehiculo.tecnicos) > 3:
                    raise ValueError(f'Máximo 3 técnicos por vehículo. Vehículo {vehiculo.vehiculo} tiene {len(vehiculo.tecnicos)} técnicos')
        return v

class RecoleccionResponse(BaseModel):
    id: str
    cliente: str
    fecha: datetime
    direccion: str
    vehiculos: List[VehiculoAsignadoSchema]
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
    vehiculo: Optional[str] = None  # Filtro por vehículo (busca en vehiculos[].vehiculo)
    chofer: Optional[str] = None    # Filtro por chofer (busca en vehiculos[].chofer)
    tecnico: Optional[str] = None   # Filtro por técnico (busca en vehiculos[].tecnicos)
    estado: Optional[str] = None