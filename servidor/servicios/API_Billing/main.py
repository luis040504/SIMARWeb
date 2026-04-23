from fastapi import FastAPI
from contextlib import asynccontextmanager

from src.config.database import test_connection
from src.routes.billing_routes import router as billing_router
from src.middleware.cors_middleware import setup_cors
from src.handlers.exception_handlers import setup_exception_handlers

tags_metadata = [
    {
        "name": "Billing",
        "description": "Operaciones con facturas (creación, consulta, actualización, cambio de estado y eliminación lógica).",
    }
]

@asynccontextmanager
async def lifespan(app: FastAPI):
    await test_connection()
    yield

app = FastAPI(
    title="SIMAR - API Facturación (Billing)",
    description="Microservicio de gestión de facturación para la plataforma SIMAR. Provee endpoints para la integración con el cliente web.",
    version="1.0.0",
    openapi_tags=tags_metadata,
    docs_url="/docs",
    redoc_url="/redoc",
    openapi_url="/openapi.json",
    lifespan=lifespan
)

setup_cors(app)
setup_exception_handlers(app)
app.include_router(billing_router)

@app.get("/", tags=["Health"])
def read_root():
    return {"message": "API de Facturación está funcionando. Visita /docs para ver la documentación de Swagger."}
