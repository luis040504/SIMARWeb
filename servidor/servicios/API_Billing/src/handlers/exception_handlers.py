from fastapi import FastAPI, Request
from fastapi.responses import JSONResponse
from fastapi.exceptions import RequestValidationError
from starlette.exceptions import HTTPException as StarletteHTTPException
from src.handlers.exceptions import AppException

def setup_exception_handlers(app: FastAPI):

    @app.exception_handler(AppException)
    async def app_exception_handler(request: Request, exc: AppException):
        return JSONResponse(
            status_code=exc.status_code,
            content={
                "status": exc.status_code,
                "code": exc.code,
                "message": exc.message,
                "details": exc.details
            }
        )

    @app.exception_handler(StarletteHTTPException)
    async def http_exception_handler(request: Request, exc: StarletteHTTPException):
        return JSONResponse(
            status_code=exc.status_code,
            content={
                "status": exc.status_code,
                "code": "HTTP_ERROR",
                "message": exc.detail,
                "details": None
            }
        )

    @app.exception_handler(RequestValidationError)
    async def validation_exception_handler(request: Request, exc: RequestValidationError):
        return JSONResponse(
            status_code=422,
            content={
                "status": 422,
                "code": "VALIDATION_ERROR",
                "message": "Error de validación en los datos de entrada",
                "details": exc.errors()
            }
        )

    @app.exception_handler(Exception)
    async def general_exception_handler(request: Request, exc: Exception):
        return JSONResponse(
            status_code=500,
            content={
                "status": 500,
                "code": "INTERNAL_SERVER_ERROR",
                "message": "Ha ocurrido un error interno en el servidor.",
                "details": str(exc)
            }
        )
