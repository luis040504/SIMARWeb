from fastapi import APIRouter, Depends, status, File, UploadFile, Form
from typing import List
from ..schemas.billing_schema import BillingCreateSchema, BillingUpdateSchema, BillingResponseSchema, BillingFilterSchema
from ..controller.billing_controller import BillingController

router = APIRouter(prefix="/billing", tags=["Billing"])

@router.get("/", response_model=List[BillingResponseSchema])
async def get_all_billing(filtro: BillingFilterSchema = Depends()):
    """Obtener todas las facturas, opcionalmente incluyendo las inactivas usando filtros"""
    return await BillingController.get_all(filtro)

@router.get("/{billing_id}", response_model=BillingResponseSchema)
async def get_billing(billing_id: str):
    """Obtener una factura específica por su ID"""
    return await BillingController.get_by_id(billing_id)

@router.get("/client/{client_id}", response_model=List[BillingResponseSchema])
async def get_billing_by_client(client_id: str):
    """Obtener todas las facturas de un cliente específico por su ID interno"""
    return await BillingController.get_by_client_id(client_id)

@router.post("/", response_model=BillingResponseSchema, status_code=status.HTTP_201_CREATED)
async def create_billing(billing_data: BillingCreateSchema):
    """Generar/Crear una nueva factura"""
    return await BillingController.create(billing_data)

@router.put("/{billing_id}", response_model=BillingResponseSchema)
async def update_billing(billing_id: str, billing_data: BillingUpdateSchema):
    """Actualizar datos de una factura existente"""
    return await BillingController.update(billing_id, billing_data)

@router.delete("/{billing_id}", status_code=status.HTTP_200_OK)
async def delete_billing(billing_id: str):
    """Soft-delete de una factura dejándola como inactiva y estado CANCELLED"""
    return await BillingController.delete(billing_id)

@router.patch("/{billing_id}/status", response_model=BillingResponseSchema)
async def change_status(billing_id: str, new_status: str, reason: str = None):
    """Cambiar el estado de la factura (ej: PENDING -> ACCEPTED / REJECTED) y anexar razón de rechazo"""
    return await BillingController.change_status(billing_id, new_status, reason)

@router.post("/{billing_id}/upload", response_model=BillingResponseSchema)
async def upload_physical_invoice(billing_id: str, file: UploadFile = File(...)):
    """Subir el PDF de una factura física"""
    return await BillingController.upload_file(billing_id, file)
