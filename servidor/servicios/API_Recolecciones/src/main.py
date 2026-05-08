from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from .routes import recoleccion_routes
from .middlewares.error_handler import validation_exception_handler, generic_exception_handler
from fastapi.exceptions import RequestValidationError
from .config.database import test_connection
import os

app = FastAPI(
    title="SIMAR - Recolecciones API",
    description="Microservicio para gestión de recolecciones de residuos con múltiples vehículos",
    version="2.0.0"
)

# CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Manejadores de errores
app.add_exception_handler(RequestValidationError, validation_exception_handler)
app.add_exception_handler(Exception, generic_exception_handler)

# Rutas
app.include_router(recoleccion_routes.router, prefix="/api/recolecciones", tags=["Recolecciones"])

# Health check
@app.get("/health")
async def health_check():
    db_connected = await test_connection()
    return {
        "status": "healthy" if db_connected else "degraded",
        "service": "recolecciones-api",
        "database": "connected" if db_connected else "disconnected",
        "version": "2.0.0"
    }

@app.on_event("startup")
async def startup_event():
    print("🚛 Iniciando Recolecciones API v2...")
    await test_connection()
    print(f"📡 Servidor corriendo en puerto {os.getenv('PORT', 8004)}")

@app.on_event("shutdown")
async def shutdown_event():
    print("🛑 Cerrando Recolecciones API...")