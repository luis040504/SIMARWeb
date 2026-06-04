from pydantic import BaseModel, Field
from typing import Optional

class PacSettingsSchema(BaseModel):
    pac_mode: str = Field(..., description="Modo del PAC: PAID o SIMULATED")
    timbres_used: int = Field(0, description="Timbres consumidos en modo de pago")
    timbres_limit: int = Field(5, description="Límite máximo de timbres en modo de pago")

class PacSettingsUpdateSchema(BaseModel):
    pac_mode: str
    timbres_limit: int
    timbres_used: Optional[int] = None
