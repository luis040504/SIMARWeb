from enum import Enum
from pydantic import BaseModel, EmailStr, Field
from datetime import datetime
from enum import Enum
from uuid import UUID

class StatusEnum(str, Enum):
    activo = "activo"
    inactivo = "inactivo"

# Playlist schemas
class ClientBase(BaseModel):
    name: str = Field(max_length=100)
    phone: str = Field(max_length=100)
    registerDate: datetime | None = None
    address: str = Field(max_length=100)
    rfc: str = Field(max_length=100)
    urlSatCertificate: str = Field(max_length=100)
    semarnatNum: str = Field(max_length=100)
    status: StatusEnum
    idUser: UUID


class ClientCreate(BaseModel):
    name: str
    phone: str | None = None
    address: str | None = None
    rfc: str | None = None
    urlSatCertificate: str | None = None
    semarnatNum: str | None = None
    idUser: UUID


class ClientUpdate(BaseModel):
    name: str | None = Field(default=None, max_length=100)
    phone: str | None = None
    address: str | None = None
    rfc: str | None = None
    urlSatCertificate: str | None = None
    semarnatNum: str | None = None


class Client(ClientBase):
    id: int
    name: str
    phone: str | None = None
    registerDate: datetime
    address: str | None = None
    rfc: str | None = None
    urlSatCertificate: str | None = None
    semarnatNum: str | None = None
    status: StatusEnum
    idUser: UUID

    class Config:
        from_attributes = True

