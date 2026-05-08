from pydantic import BaseModel
from typing import List, Optional
from datetime import date

class ClientSummarySchema(BaseModel):
    id: int
    razon_social: str
    rfc: Optional[str] = None
    direccion_fiscal: Optional[str] = None

class ContractSummarySchema(BaseModel):
    folio: str
    precio_unitario: float
    metodo_pago: Optional[str] = None
    condiciones: Optional[str] = None

class ResidueDetailSchema(BaseModel):
    residuo: str
    cantidad: float
    unidad: str

class ReadyToBillSchema(BaseModel):
    manifest_id: int
    numero_manifiesto: str
    fecha_servicio: date
    tipo_residuo: str
    cliente: ClientSummarySchema
    contrato: Optional[ContractSummarySchema] = None
    detalles_servicio: List[ResidueDetailSchema]
    total_estimado: float
