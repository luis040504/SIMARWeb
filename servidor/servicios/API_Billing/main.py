from fastapi import FastAPI
from contextlib import asynccontextmanager
from fastapi.middleware.cors import CORSMiddleware

from src.config.database import test_connection
from src.routes.billing_routes import router as billing_router

@asynccontextmanager
async def lifespan(app: FastAPI):
    await test_connection()
    yield

app = FastAPI(
    title="SIMAR - API Facturación (Billing)",
    description="Microservicio de gestión de facturación para la plataforma SIMAR",
    version="1.0.0",
    lifespan=lifespan
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)


app.include_router(billing_router)

@app.get("/")
def read_root():
    return {"message": "API de Facturación está funcionando"}
