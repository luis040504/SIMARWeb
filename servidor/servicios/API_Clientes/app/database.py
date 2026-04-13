from sqlalchemy import create_engine
from sqlalchemy.orm import declarative_base
from sqlalchemy.orm import sessionmaker
import os

SQLALCHEMY_DATABASE_URL = os.getenv("DATABASE_CLIENT_URL",
    "postgresql+psycopg2://simero:contra@localhost:5432/simar_client_db"
)

if not SQLALCHEMY_DATABASE_URL:
    raise ValueError("No se encontró la variable de entorno DATABASE_CLIENT_URL")

# Create engine and session
engine = create_engine(
    SQLALCHEMY_DATABASE_URL,
    pool_pre_ping=True
)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

Base = declarative_base()

# Dependency to get a DB session for each request
def get_db():
    db = SessionLocal()
    try:
        yield db
    finally:
        db.close()