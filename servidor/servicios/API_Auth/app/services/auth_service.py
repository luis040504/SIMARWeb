from datetime import timedelta

from uuid import UUID
from fastapi import Depends
from fastapi.security import OAuth2PasswordBearer
from sqlalchemy.orm import Session

from app import schemas, security
from app.database import get_db
from app.repositories import auth_repository

class AuthService:
    def __init__(self, db: Session):
        self.db = db
    
    def login_user(self, identifier: str, password: str) -> schemas.Token:
        try:
            user = auth_repository.get_user_by_email(self.db, identifier)
            if not user:
                user = auth_repository.get_user_by_username(self.db, identifier)

            if not user or not security.verify_password(password, user.password_hash):
                raise ValueError("Incorrect credentials")
            
            if not user.is_active:
                raise ValueError("Your account is banned.")

            access_token_expires = timedelta(minutes=security.ACCESS_TOKEN_EXPIRE_MINUTES)
            access_token = security.create_access_token(
                data={"sub": str(user.id_user), "role": user.role},
                expires_delta=access_token_expires
            )

            return schemas.Token(access_token=access_token, token_type="bearer")

        except ValueError as e:
            raise e
        except Exception as e:
            #raise RuntimeError("Login service unavailable - please try again later") from e
            print("ERROR REAL EN AUTH SERVICE:", e) 
            raise e 

    
    def get_user_by_id(self, user_id: UUID) -> schemas.User:
        user = auth_repository.get_user_by_id(self.db, user_id)
        if not user:
            raise ValueError("User not found")
        return user

    def get_user_by_username(self, username: str) -> schemas.User:
        user = auth_repository.get_user_by_username(self.db, username)
        if not user:
            raise ValueError("User not found")
        return user

    def get_all_users(self, limit: int = 100, offset: int = 0):
        users = auth_repository.get_all_users(self.db, limit=limit, offset=offset)
        if not users:
            raise ValueError("No users registered/active")
        return users
        
    def forward_auth_header(self, token: str):
        return {"Authorization": f"Bearer {token}"}

def get_auth_service(db: Session = Depends(get_db)):
    return AuthService(db)