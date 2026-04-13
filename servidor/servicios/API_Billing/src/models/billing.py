from datetime import datetime
from typing import Optional, List, Literal
from pydantic import BaseModel, Field

class MetadataModel(BaseModel):
    created_at: datetime = Field(default_factory=datetime.now)
    updated_at: datetime = Field(default_factory=datetime.now)
    source: Literal["web_app", "mobile_app", "email_sync"]

class IssuerModel(BaseModel):
    tax_id: str
    name: str
    tax_regime: str

class ReceiverModel(BaseModel):
    tax_id: str
    name: str
    tax_usage: str

class FiscalDataModel(BaseModel):
    uuid: Optional[str] = None
    invoice_series: Optional[str] = None
    invoice_folio: Optional[str] = None
    issue_date: datetime
    certification_date: Optional[datetime] = None
    cfdi_version: Optional[str] = None

class FinancialsModel(BaseModel):
    currency: str
    exchange_rate: float = 1.0
    subtotal: float
    discount: float = 0.0
    tax_total: float
    total: float
    payment_method: Literal["PUE", "PPD"]
    payment_form: str

class TaxModel(BaseModel):
    type: str
    rate: float
    amount: float

class ItemModel(BaseModel):
    product_code: str
    description: str
    quantity: float
    unit_price: float
    amount: float
    taxes: List[TaxModel] = []

class AttachmentsModel(BaseModel):
    xml_url: Optional[str] = None
    pdf_url: Optional[str] = None
    image_url: Optional[str] = None
    thumbnail_url: Optional[str] = None

class Billing(BaseModel):
    id: Optional[str] = Field(None, alias='_id')
    upload_type: Literal["DIGITAL", "PHYSICAL"]
    metadata: MetadataModel
    issuer: IssuerModel
    receiver: ReceiverModel
    fiscal_data: FiscalDataModel
    financials: FinancialsModel
    items: List[ItemModel]
    attachments: AttachmentsModel
    status: Literal["VALID", "CANCELLED", "PENDING_APPROVAL"]
    activo: bool = True
    
    class Config:
        populate_by_name = True
        arbitrary_types_allowed = True
        json_encoders = {
            datetime: lambda v: v.isoformat()
        }
