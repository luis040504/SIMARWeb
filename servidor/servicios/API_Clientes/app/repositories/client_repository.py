from sqlalchemy.orm import Session
from app import models, schemas
from sqlalchemy import or_, cast, String
from uuid import UUID
from app.models import StatusEnum


def get_client_by_id(db: Session, client_id: int):
    return db.query(models.Clientes).filter(models.Clientes.id == client_id).first()


def get_client_by_user(db: Session, user_id: UUID):
    return db.query(models.Clientes).filter(models.Clientes.idUser == user_id).first()


def create_client(db: Session, client: schemas.ClientCreate):
    db_client = models.Clientes(
        name=client.name,
        phone=client.phone,
        address=client.address,
        rfc=client.rfc,
        urlSatCertificate= None,
        semarnatNum =client.semarnatNum, 
        idUser=client.idUser
    )
    db.add(db_client)
    db.commit()
    db.refresh(db_client)
    return db_client


def update_client(db: Session, client_id: int, client_update: schemas.ClientUpdate):
    db_client = get_client_by_id(db, client_id)
    if not db_client:
        return None

    # Solo actualiza los campos que fueron enviados
    if client_update.name is not None:
        db_client.name = client_update.name

    if client_update.phone is not None:
        db_client.phone = client_update.phone

    if client_update.address is not None:
        db_client.address = client_update.address

    if client_update.rfc is not None:
        db_client.rfc = client_update.rfc

    if client_update.urlSatCertificate is not None:
        db_client.urlSatCertificate = client_update.urlSatCertificate

    if client_update.semarnatNum is not None:
        db_client.semarnatNum = client_update.semarnatNum

    db.commit()
    db.refresh(db_client)
    return db_client


def get_active_clients(db: Session):
    return db.query(models.Clientes).filter(models.Clientes.status == StatusEnum.activo).all()

def get_inactive_clients(db: Session):
    return db.query(models.Clientes).filter(models.Clientes.status == StatusEnum.inactivo).all()

def get_active_client_by_user(db: Session, user_id: UUID):
    return (
        db.query(models.Clientes)
        .filter(
            models.Clientes.idUser == user_id,
            models.Clientes.status == StatusEnum.activo
        )
        .first()
    )

def get_inactive_client_by_user(db: Session, user_id: UUID):
    return (
        db.query(models.Clientes)
        .filter(
            models.Clientes.idUser == user_id,
            models.Clientes.status == StatusEnum.inactivo
        )
        .first()
    )

def get_active_clients_by_name(db: Session, name: str):
    return db.query(models.Clientes).filter(
        models.Clientes.status == StatusEnum.activo,
        models.Clientes.name.ilike(f"%{name}%")
    ).all()

def get_inactive_clients_by_name(db: Session, name: str):
    return db.query(models.Clientes).filter(
        models.Clientes.status == StatusEnum.inactivo,
        models.Clientes.name.ilike(f"%{name}%")
    ).all()

def get_clients_by_name(db: Session, name: str):
    return db.query(models.Clientes).filter(
        models.Clientes.name.ilike(f"%{name}%")
    ).all()


def get_all_clients(db: Session):
    return db.query(models.Clientes).all()


def search_clients(db: Session, query: str):
    if not query:
        return []
    
    filters = [
        models.Clientes.name.ilike(f"%{query}%"),
        models.Clientes.phone.ilike(f"%{query}%"),
        models.Clientes.address.ilike(f"%{query}%"),
        models.Clientes.rfc.ilike(f"%{query}%"),
        models.Clientes.semarnatNum.ilike(f"%{query}%"),
    ]

    q = db.query(models.Clientes).filter(or_(*filters))

    return q.all()


def deactivate_client(db: Session, client_id: int):
    client = db.query(models.Clientes)\
        .filter(models.Clientes.id == client_id)\
        .first()

    if not client:
        return None

    client.status = StatusEnum.inactivo
    db.commit()
    db.refresh(client)

    return client


def activate_client(db: Session, client_id: int):
    client = db.query(models.Clientes)\
        .filter(models.Clientes.id == client_id)\
        .first()

    if not client:
        return None

    client.status = StatusEnum.activo
    db.commit()
    db.refresh(client)

    return client