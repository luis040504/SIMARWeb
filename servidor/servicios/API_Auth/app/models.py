from sqlalchemy import Boolean, Column, Enum, ForeignKey, Integer, String, DateTime, func
from app.database import Base
from sqlalchemy.dialects.postgresql import UUID
import uuid

class User(Base):
    __tablename__ = "users"

    id_user = Column(UUID(as_uuid=True), primary_key=True, default=uuid.uuid4)
    email = Column(String(100),unique=True, index=True, nullable=False)
    password_hash = Column(String(255), nullable=False)
    username = Column(String(50), unique=True, nullable=False) 
    is_active = Column(Boolean, default=True)
    role = Column(Enum("empleado", "cliente", name="role_enum"), nullable=False)
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), server_default=func.now(), onupdate=func.now())


