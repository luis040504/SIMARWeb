from uuid import UUID

from fastapi import FastAPI, Depends, HTTPException, status

from app import schemas
from app.dependencies.dependencies import get_current_user
from app.services.auth_service import AuthService, get_auth_service

app = FastAPI(title="Simar")
# --- Authentication helpers ---

# --- API Endpoints ---
    
@app.post("/login", response_model=schemas.Token)
def login(
    form_data: schemas.UserLogin,
    auth_service: AuthService = Depends(get_auth_service)
):
    try: 
        return auth_service.login_user(form_data.identifier, form_data.password)
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_401_UNAUTHORIZED,
            detail=str(e),
            headers={"WWW-Authenticate": "Bearer"},
        )
    except Exception as e:
        #raise HTTPException(
        #    status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
        #    detail="Service Unavailable, please try again later"
        #)
        print("ERROR EN LOGIN:", e)
        raise e

@app.get("/verify-token")
def verify_token(current_user: schemas.User = Depends(get_current_user)):
    return current_user

@app.get("/health")
def health_check():
    return {"status": "ok"}

    
@app.get("/users/{user_id}", response_model=schemas.User)
def get_user(
    user_id: UUID,
    current_user: schemas.User = Depends(get_current_user),
    auth_service: AuthService = Depends(get_auth_service)
):
    try:
        return auth_service.get_user_by_id(user_id)
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=str(e)
        )

@app.get("/users/username/{username}", response_model=schemas.User)
def get_user_username(
        username: str,
        auth_service: AuthService = Depends(get_auth_service)
):
    try:
        return auth_service.get_user_by_username(username)
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail=str(e)
        )
