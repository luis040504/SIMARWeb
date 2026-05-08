import motor.motor_asyncio
from dotenv import load_dotenv
import os

load_dotenv()

# Configuración de MongoDB
MONGODB_HOST = os.getenv('MONGODB_HOST', 'localhost')
MONGODB_PORT = int(os.getenv('MONGODB_PORT', 27017))
MONGODB_USER = os.getenv('MONGODB_USER', 'root')
MONGODB_PASSWORD = os.getenv('MONGODB_PASSWORD', 'Simar123!')
MONGODB_DB = os.getenv('MONGODB_DB', 'simar_trazabilidad_db')

MONGO_URI = f"mongodb://{MONGODB_USER}:{MONGODB_PASSWORD}@{MONGODB_HOST}:{MONGODB_PORT}"

client = motor.motor_asyncio.AsyncIOMotorClient(MONGO_URI)
database = client[MONGODB_DB]
eventos_collection = database['eventos_trazabilidad']

async def test_connection():
    try:
        await client.admin.command('ping')
        print("✅ MongoDB conectado correctamente")
        return True
    except Exception as e:
        print(f"❌ Error conectando a MongoDB: {e}")
        return False