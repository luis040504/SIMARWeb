import os
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from dotenv import load_dotenv

def setup_cors(app: FastAPI):
    load_dotenv()
    origins_str = os.getenv("ALLOWED_ORIGINS", "http://localhost:5181,https://localhost:7034")
    origins = [origin.strip() for origin in origins_str.split(",") if origin.strip()]

    app.add_middleware(
        CORSMiddleware,
        allow_origins=origins,
        allow_credentials=True,
        allow_methods=["*"],
        allow_headers=["*"],
    )
