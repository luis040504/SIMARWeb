from sqlalchemy.orm import Session
from app import models
from sqlalchemy.exc import SQLAlchemyError, IntegrityError, OperationalError
from uuid import UUID

def get_user_by_email(db: Session, email: str):
    """Find a user by their email address."""
    return db.query(models.User).filter(models.User.email == email).first()

def get_user_by_username(db: Session, username: str):
    """Find a user by their username"""
    return db.query(models.User).filter(models.User.username == username).first()


def get_user_by_id(db: Session, user_id: UUID):
    """Find a user by their ID."""
    try:
        return db.query(models.User).filter(models.User.id_user == user_id).first()
    except OperationalError as e:
        raise ConnectionError("Database connection error - please try again later")
    except Exception as e:
        raise e


def get_all_users(db: Session, limit : int = 100, offset: int = 0):
    try:
        safe_limit = min(limit, 1000)
        return (
            db.query(models.User)
            .offset(offset)
            .limit(safe_limit)
            .all()
        )
    except OperationalError as e:
        raise ConnectionError("Database connection error, please try again later")
    except Exception as e:
        raise e
    