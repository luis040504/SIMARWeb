from pydantic import BaseModel, Field
from typing import Optional, List, Literal
from datetime import datetime

class InvoiceMetadata(BaseModel):
    created_at: datetime
    updated_at: datetime
    source: Literal["web_app", "mobile_app", "email_sync"]

class Issuer(BaseModel):
    tax_id: str = Field(..., min_length=12, max_length=13, description="RFC del emisor")
    name: str = Field(..., min_length=3, description="Nombre o razón social")
    tax_regime: str

class Receiver(BaseModel):
    tax_id: str = Field(..., min_length=12, max_length=13, description="RFC del receptor")
    name: str = Field(..., min_length=3, description="Nombre o razón social")
    tax_usage: str
    postal_code: Optional[str] = Field(None, min_length=5, max_length=5, description="Código postal")
    fiscal_regime: Optional[str] = None
    client_id: Optional[str] = None

class FiscalData(BaseModel):
    uuid: Optional[str] = None
    invoice_series: Optional[str] = None
    invoice_folio: Optional[str] = None
    issue_date: datetime
    certification_date: Optional[datetime] = None
    cfdi_version: Optional[str] = None

class Financials(BaseModel):
    currency: str
    exchange_rate: float = Field(..., gt=0, description="Tipo de cambio")
    subtotal: float = Field(..., ge=0, description="Subtotal antes de impuestos y descuentos")
    discount: float = Field(0.0, ge=0, description="Descuento aplicado")
    tax_total: float = Field(..., ge=0, description="Total de impuestos")
    total: float = Field(..., ge=0, description="Total de la factura")
    payment_method: str
    payment_form: str

class Tax(BaseModel):
    type: str
    rate: float = Field(..., ge=0, description="Tasa del impuesto")
    amount: float = Field(..., ge=0, description="Monto del impuesto")

class Item(BaseModel):
    product_code: str
    unit_code: Optional[str] = None
    tax_object: Optional[str] = None
    description: str = Field(..., min_length=1, description="Descripción del concepto")
    quantity: float = Field(..., gt=0, description="Cantidad")
    unit_price: float = Field(..., ge=0, description="Precio unitario")
    amount: float = Field(..., ge=0, description="Importe (cantidad * precio unitario)")
    taxes: List[Tax] = []

class Attachments(BaseModel):
    xml_url: Optional[str] = None
    pdf_url: Optional[str] = None
    image_url: Optional[str] = None
    thumbnail_url: Optional[str] = None

class BillingBase(BaseModel):
    upload_type: Literal["DIGITAL", "PHYSICAL"]
    record_type: Optional[str] = "Invoice"
    service_type: Optional[str] = None
    metadata: InvoiceMetadata
    issuer: Issuer
    receiver: Receiver
    fiscal_data: FiscalData
    financials: Financials
    items: List[Item]
    attachments: Attachments
    status: Literal["VALID", "CANCELLED", "PENDING_APPROVAL", "Pending", "Accepted", "Rejected"]
    reason: Optional[str] = None
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
    status: Optional[Literal["VALID", "CANCELLED", "PENDING_APPROVAL", "Pending", "Accepted", "Rejected"]] = None
    reason: Optional[str] = None
    record_type: Optional[str] = None
    service_type: Optional[str] = None

class BillingResponseSchema(BillingBase):
    id: str = Field(alias="_id")

    class Config:
        populate_by_name = True

class BillingFilterSchema(BaseModel):
    upload_type: Optional[Literal["DIGITAL", "PHYSICAL"]] = None
    status: Optional[Literal["VALID", "CANCELLED", "PENDING_APPROVAL", "Pending", "Accepted", "Rejected"]] = None
    issuer_tax_id: Optional[str] = None
    receiver_tax_id: Optional[str] = None
    record_type: Optional[str] = None
    search_query: Optional[str] = None
    start_date: Optional[datetime] = None
    end_date: Optional[datetime] = None
    include_deleted: Optional[bool] = False