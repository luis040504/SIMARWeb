from sqlalchemy import create_engine
from sqlalchemy.ext.declarative import declarative_base
from sqlalchemy.orm import sessionmaker
import os

POSTGRES_USER = os.getenv("DB_USERS_USER")
POSTGRES_PASSWORD = os.getenv("DB_USERS_PASSWORD")
POSTGRES_DB = os.getenv("DB_USERS_NAME")

DATABASE_URL = f"postgresql://{POSTGRES_USER}:{POSTGRES_PASSWORD}@db_usuarios:5432/{POSTGRES_DB}"

# Create engine and session
engine = create_engine(DATABASE_URL)
SessionLocal = sessionmaker(autocommit=False, autoflush=False, bind=engine)

Base = declarative_base()

# Dependency to get a DB session for each request
def get_db():
    db = SessionLocal()
    try:
        yield db
    except: 
        db.rollback()
        raise
    finally:
        db.close()