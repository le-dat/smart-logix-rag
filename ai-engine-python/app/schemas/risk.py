from typing import Dict, Optional
from pydantic import BaseModel, Field

class RiskPredictionQuery(BaseModel):
    shipment_id: Optional[int] = Field(None, description="The ID of the shipment if existing in SQL DB.")
    route_id: int = Field(..., ge=1, description="Unique route identifier.")
    carrier: str = Field(..., pattern="^(Dimerco Air|Express Ocean|FastLogix|DirectCarrier)$", description="Name of the shipping carrier (must match trained carriers).")
    weight: float = Field(..., ge=1.0, le=50000.0, description="Weight of the shipment cargo in kg.")
    weather_index: float = Field(..., ge=0.0, le=5.0, description="Scale of weather severity (0.0 for clear, 5.0 for storm).")
    historical_delay_ratio: float = Field(0.0, ge=0.0, le=1.0, description="Delay ratio of the chosen carrier/route (0.0 to 1.0).")

class RiskPredictionResponse(BaseModel):
    shipment_id: Optional[int] = Field(None)
    risk_score: float = Field(..., description="Inferred delay risk probability (0.0 to 100.0%).")
    risk_level: str = Field(..., description="Risk tier: Low, Medium, High.")
    contributing_factors: Dict[str, float] = Field(..., description="Feature importance mapping showing contributing weights.")
    is_fallback: bool = Field(False, description="Flag indicating if the prediction fell back to rules due to missing model.")
