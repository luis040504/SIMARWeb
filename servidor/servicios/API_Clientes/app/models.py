from sqlalchemy import Boolean, Column, ForeignKey, Integer, String, DateTime, func, LargeBinary, Enum
from sqlalchemy.orm import relationship
from app.database import Base
import enum
from sqlalchemy.dialects.postgresql import UUID


class StatusEnum(enum.Enum):
    activo = "activo"
    inactivo = "inactivo"

class Clientes(Base):
    __tablename__ = "clientes"

    id = Column(Integer, primary_key=True, index=True)
    name = Column(String(100), nullable=False)
    phone = Column(String(100), nullable=True)
    registerDate = Column(DateTime(timezone=True), server_default=func.now())
    address = Column(String(100), nullable=True)
    rfc = Column(String(100), nullable=True)
    urlSatCertificate = Column(String(100), nullable=True)
    semarnatNum = Column(String(100), nullable=True)
    status = Column(Enum(StatusEnum), nullable=False, default=StatusEnum.activo)
    idUser = Column(UUID(as_uuid=True), nullable=False)

    



