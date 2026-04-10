from sqlalchemy import Boolean, Column, ForeignKey, Integer, String, DateTime, func, LargeBinary, Enum
from sqlalchemy.orm import relationship
from app.database import Base
import enum


class StatusEnum(enum.Enum):
    activo = "activo"
    inactivo = "inactivo"

class Clientes(Base):
    __tablename__ = "clientes"

    id = Column(Integer, primary_key=True, index=True)
    nombre = Column(String(100), nullable=False)
    telefono = Column(String(100), nullable=True)
    fechaRegistro = Column(DateTime(timezone=True), server_default=func.now())
    direccion = Column(String(100), nullable=True)
    rfc = Column(String(100), nullable=True)
    urlCertificadoSat = Column(String(100), nullable=True)
    numeroSemarnat = Column(String(100), nullable=True)
    status = Column(Enum(StatusEnum), nullable=False, default=StatusEnum.activo)
    idUsuario = Column(Integer, nullable=True)

    



