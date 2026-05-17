from pydantic import BaseModel
from typing import List, Optional
from datetime import date

class ClientSummarySchema(BaseModel):
    id: int
    razon_social: str
    rfc: Optional[str] = None
    direccion_fiscal: Optional[str] = None
    postal_code: Optional[str] = None

class ContractSummarySchema(BaseModel):
    folio: str
    precio_unitario: float
    metodo_pago: Optional[str] = None
    condiciones: Optional[str] = None

class ResidueDetailSchema(BaseModel):
    residuo: str
    cantidad: float
    unidad: str
    precio_unitario: float = 0.0
    subtotal: float = 0.0

class ReadyToBillSchema(BaseModel):
    manifest_id: Optional[int] = 0
    numero_manifiesto: Optional[str] = "N/A"
    fecha_servicio: date
    tipo_residuo: str
    cliente: ClientSummarySchema
    contrato: Optional[ContractSummarySchema] = None
    detalles_servicio: List[ResidueDetailSchema]
    total_estimado: float
    source: str = "manifest" # 'manifest' o 'contract'
