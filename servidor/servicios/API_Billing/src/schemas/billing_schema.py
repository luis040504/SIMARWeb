from pydantic import BaseModel, Field
from typing import Optional, List, Literal
from datetime import datetime

class InvoiceMetadata(BaseModel):
    created_at: datetime
    updated_at: datetime
    source: Literal["web_app", "mobile_app", "email_sync"]

class Issuer(BaseModel):
    tax_id: str
    name: str
    tax_regime: str

class Receiver(BaseModel):
    tax_id: str
    name: str
    tax_usage: str

class FiscalData(BaseModel):
    uuid: Optional[str] = None
    invoice_series: Optional[str] = None
    invoice_folio: Optional[str] = None
    issue_date: datetime
    certification_date: Optional[datetime] = None
    cfdi_version: Optional[str] = None

class Financials(BaseModel):
    currency: str
    exchange_rate: float
    subtotal: float
    discount: float = 0.0
    tax_total: float
    total: float
    payment_method: Literal["PUE", "PPD"]
    payment_form: str

class Tax(BaseModel):
    type: str
    rate: float
    amount: float

class Item(BaseModel):
    product_code: str
    description: str
    quantity: float
    unit_price: float
    amount: float
    taxes: List[Tax] = []

class Attachments(BaseModel):
    xml_url: Optional[str] = None
    pdf_url: Optional[str] = None
    image_url: Optional[str] = None
    thumbnail_url: Optional[str] = None

class BillingBase(BaseModel):
    upload_type: Literal["DIGITAL", "PHYSICAL"]
    metadata: InvoiceMetadata
    issuer: Issuer
    receiver: Receiver
    fiscal_data: FiscalData
    financials: Financials
    items: List[Item]
    attachments: Attachments
    status: Literal["VALID", "CANCELLED", "PENDING_APPROVAL"]
    activo: bool = True

class BillingCreateSchema(BillingBase):
    pass

class BillingUpdateSchema(BaseModel):
    upload_type: Optional[Literal["DIGITAL", "PHYSICAL"]] = None
    metadata: Optional[InvoiceMetadata] = None
    issuer: Optional[Issuer] = None
    receiver: Optional[Receiver] = None
    fiscal_data: Optional[FiscalData] = None
    financials: Optional[Financials] = None
    items: Optional[List[Item]] = None
    attachments: Optional[Attachments] = None
    status: Optional[Literal["VALID", "CANCELLED", "PENDING_APPROVAL"]] = None

class BillingResponseSchema(BillingBase):
    id: str = Field(alias="_id")

    class Config:
        populate_by_name = True

class BillingFilterSchema(BaseModel):
    upload_type: Optional[Literal["DIGITAL", "PHYSICAL"]] = None
    status: Optional[Literal["VALID", "CANCELLED", "PENDING_APPROVAL"]] = None
    issuer_tax_id: Optional[str] = None
    receiver_tax_id: Optional[str] = None
    start_date: Optional[datetime] = None
    end_date: Optional[datetime] = None
    include_deleted: Optional[bool] = False