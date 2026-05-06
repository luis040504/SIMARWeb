from fastapi import Depends, HTTPException
from sqlalchemy.orm import Session

from app import schemas
from app.database import get_db
from app.repositories import client_repository
from uuid import UUID


class ClientService:
    def __init__(self, db: Session):
        self.db = db

    def create_client(self, client_data: schemas.ClientCreate):
        if len(client_data.name.strip()) == 0:
            raise ValueError("Client name required")
        return client_repository.create_client(self.db, client_data)

    def update_client(self, client_id: int, client_update: schemas.ClientUpdate):
        cl = client_repository.update_client(self.db, client_id, client_update)
        if not cl:
            raise HTTPException(status_code=404, detail="Client not found")
        return cl

    def get_active_clients(self):
        return client_repository.get_active_clients(self.db)
    
    def get_inactive_clients(self):
        return client_repository.get_inactive_clients(self.db)
    
    def get_client_by_user(self, user_id: UUID):
        cl = client_repository.get_client_by_user(self.db, user_id)
        if not cl:
            raise HTTPException(status_code=404, detail="Client not found")
        return cl

    def get_active_client_by_user(self, user_id: UUID):
        cl = client_repository.get_active_client_by_user(self.db, user_id)
        if not cl:
            raise HTTPException(status_code=404, detail="Client not found")
        return cl

    def get_inactive_client_by_user(self, user_id: UUID):
        cl = client_repository.get_inactive_client_by_user(self.db, user_id)
        if not cl:
            raise HTTPException(status_code=404, detail="Client not found")
        return cl

    def get_all_clients(self):
        return client_repository.get_all_clients(self.db)

    def get_client_by_id(self, client_id: int):
        cl = client_repository.get_client_by_id(self.db, client_id)
        if not cl:
            raise HTTPException(status_code=404, detail="Client not found")
        return cl
    
    def get_active_clients_by_name(self, name: str):
        return client_repository.get_active_clients_by_name(self.db, name)
    
    def get_inactive_clients_by_name(self, name: str):
        return client_repository.get_inactive_clients_by_name(self.db, name)

    def get_clients_by_name(self, name: str):
        return client_repository.get_clients_by_name(self.db, name)
    
    def search_clients(self, query: str):
        return client_repository.search_clients(self.db, query)
    
    def deactivate_client(self, client_id: int):
        client = client_repository.deactivate_client(self.db, client_id)
        
        if not client:
            raise HTTPException(status_code=404, detail="Client not found")
    
        return client
    
    def activate_client(self, client_id: int):
        client = client_repository.activate_client(self.db, client_id)
        
        if not client:
            raise HTTPException(status_code=404, detail="Client not found")
    
        return client

def get_client_service(
    db: Session = Depends(get_db),
):
    return ClientService(db)

