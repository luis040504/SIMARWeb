from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from .routes import recoleccion_routes
from .middlewares.error_handler import validation_exception_handler, generic_exception_handler
from fastapi.exceptions import RequestValidationError
from .config.database import test_connection
import os

app = FastAPI(
    title="SIMAR - Recolecciones API",
    description="Microservicio para gestión de recolecciones de residuos",
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


app.include_router(recoleccion_routes.router, prefix="/api/recolecciones", tags=["Recolecciones"])


@app.get("/health")
async def health_check():
    db_connected = await test_connection()
    return {
        "status": "healthy",
        "service": "recolecciones-api",
        "database": "connected" if db_connected else "disconnected"
    }

@app.on_event("startup")
async def startup_event():
    print(" Iniciando Recolecciones API...")
    await test_connection()
    print(f" Servidor corriendo en puerto {os.getenv('PORT', 8004)}")

@app.on_event("shutdown")
async def shutdown_event():
    print(" Cerrando Recolecciones API...")