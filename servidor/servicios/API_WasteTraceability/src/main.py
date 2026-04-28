from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from .routes import evento_routes
from .middlewares.error_handler import validation_exception_handler, generic_exception_handler
from fastapi.exceptions import RequestValidationError
from .config.database import test_connection
import os

app = FastAPI(
    title="SIMAR - Trazabilidad API",
    description="Microservicio para trazabilidad y auditoría de residuos",
    version="1.0.0"
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.add_exception_handler(RequestValidationError, validation_exception_handler)
app.add_exception_handler(Exception, generic_exception_handler)

app.include_router(evento_routes.router, prefix="/api/trazabilidad", tags=["Trazabilidad"])

@app.get("/health")
async def health_check():
    db_connected = await test_connection()
    return {
        "status": "healthy",
        "service": "trazabilidad-api",
        "database": "connected" if db_connected else "disconnected"
    }

@app.on_event("startup")
async def startup_event():
    print("Iniciando Trazabilidad API...")
    await test_connection()
    print(f"Servidor corriendo en puerto {os.getenv('PORT', 8006)}")

@app.on_event("shutdown")
async def shutdown_event():
    print("Cerrando Trazabilidad API...")