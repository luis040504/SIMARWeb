from fastapi import FastAPI, Depends, HTTPException, status
from fastapi.staticfiles import StaticFiles
from fastapi import UploadFile, File
from pathlib import Path
from app.models import StatusEnum

from app.database import get_db
from app.security import get_current_user
from app.services.client_service import ClientService, get_client_service
from app.repositories.client_repository import get_client_by_id
from app import schemas as client_schemas
import os
from uuid import UUID
import uuid

app = FastAPI(title="Client Service")

STATIC_CLIENTS_DIR = Path("/app/static/clientes")
STATIC_CLIENTS_DIR.mkdir(parents=True, exist_ok=True)

app.mount("/static", StaticFiles(directory="/app/static"), name="static")

# --- API Endpoints ---
@app.post("/client", response_model=client_schemas.Client)
def create_client(
    client: client_schemas.ClientCreate,
    current_user = Depends(get_current_user),
    client_service: ClientService = Depends(get_client_service)
):
    try:
        return client_service.create_client(client)
    except ValueError as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))


@app.put("/client/{client_id}", response_model=client_schemas.Client)
def update_client(
    client_id: int,
    client_update: client_schemas.ClientUpdate,
    current_user = Depends(get_current_user),
    client_service: ClientService = Depends(get_client_service)
):
    try:
        return client_service.update_client(client_id, client_update)
    except ValueError as e:
        raise HTTPException(status_code=status.HTTP_400_BAD_REQUEST, detail=str(e))


@app.patch("/client/{client_id}/deactivate", response_model=client_schemas.Client)
def deactivate_client(
    client_id: int,
    current_user = Depends(get_current_user),
    client_service: ClientService = Depends(get_client_service)
):
    try:
        return client_service.deactivate_client(client_id)
    except ValueError as e:
        raise HTTPException(status_code=404, detail=str(e))
    
    
@app.patch("/client/{client_id}/activate", response_model=client_schemas.Client)
def activate_client(
    client_id: int,
    current_user = Depends(get_current_user),
    client_service: ClientService = Depends(get_client_service)
):
    try:
        return client_service.activate_client(client_id)
    except ValueError as e:
        raise HTTPException(status_code=404, detail=str(e))
    

@app.get("/client/active", response_model=list[client_schemas.Client])
def get_active_clients(
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_active_clients()


@app.get("/client/inactive", response_model=list[client_schemas.Client])
def get_inactive_clients(
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_inactive_clients()


@app.get("/client/all", response_model=list[client_schemas.Client])
def get_all_clients(
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_all_clients()


@app.get("/client/user/{user_id}/active", response_model=list[client_schemas.Client])
def get_active_client_by_user(
    user_id: UUID,
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_active_client_by_user(user_id)


@app.get("/client/user/{user_id}/inactive", response_model=list[client_schemas.Client])
def get_inactive_client_by_user(
    user_id: UUID,
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_inactive_client_by_user(user_id)


@app.get("/client/id/{client_id}", response_model=client_schemas.Client)
def get_client_id_endpoint(
    client_id: int,
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_client_by_id(client_id)


@app.get("/client/user/{user_id}", response_model=client_schemas.Client)
def get_client_by_user(
    user_id: UUID,
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_client_by_user(user_id)


@app.get("/client/active/name/{name}", response_model=list[client_schemas.Client])
def get_active_clients_by_name(
    name: str,
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_active_clients_by_name(name)


@app.get("/client/inactive/name/{name}", response_model=list[client_schemas.Client])
def get_inactive_clients_by_name(
    name: str,
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_inactive_clients_by_name(name)


@app.get("/client/name/{name}", response_model=list[client_schemas.Client])
def get_clients_by_name(
    name: str,
    client_service: ClientService = Depends(get_client_service)
):
    return client_service.get_clients_by_name(name)


@app.post("/client/id/{client_id}/certificate", response_model=client_schemas.Client)
async def upload_sat_certificate(
    client_id: int,
    current_user = Depends(get_current_user),
    file: UploadFile = File(...),
    client_service: ClientService = Depends(get_client_service)
):
    # 1) Verificar existencia
    client = client_service.get_client_by_id(client_id)

    # 2) Validar tipo de archivo
    allowed_types = ["application/pdf", "application/xml"]
    if file.content_type not in allowed_types:
        raise HTTPException(
            status_code=400,
            detail="Only PDF or XML files are allowed"
        )

    # 3) Generar nombre único
    _, ext = os.path.splitext(file.filename)
    if not ext:
        ext = ".pdf"

    filename = f"{uuid.uuid4().hex}{ext}"
    save_path = STATIC_CLIENTS_DIR / filename

    # 4) Guardar archivo
    try:
        content = await file.read()
        save_path.write_bytes(content)
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error saving file: {e}")

    # 5) (Opcional) borrar anterior
    try:
        old_file = client.urlSatCertificate
        if old_file:
            old_filename = old_file.split("/")[-1]
            old_path = STATIC_CLIENTS_DIR / old_file
            if old_path.exists() and old_filename != filename:
                old_path.unlink()
    except Exception:
        pass

    # 6) Guardar path
    file_path = f"/static/clientes/{filename}"

    updated_client = client_service.update_client(
        client_id,
        client_schemas.ClientUpdate(urlSatCertificate=file_path)
    )

    return updated_client
    

