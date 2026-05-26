from typing import Dict, Optional
from pydantic import BaseModel, Field

class RiskPredictionQuery(BaseModel):
    shipment_id: Optional[int] = Field(None, description="The ID of the shipment if existing in SQL DB.")
    route_id: int = Field(..., description="Unique route identifier.")
    carrier: str = Field(..., description="Name of the shipping carrier.")
    weight: float = Field(..., description="Weight of the shipment cargo.")
    weather_index: float = Field(..., description="Scale of weather severity (e.g. 0.0 for clear, 5.0 for extreme storm).")
    historical_delay_ratio: float = Field(0.0, description="Delay ratio of the chosen carrier/route (0.0 to 1.0).")

class RiskPredictionResponse(BaseModel):
    shipment_id: Optional[int] = Field(None)
    risk_score: float = Field(..., description="Inferred delay risk probability (0.0 to 100.0%).")
    risk_level: str = Field(..., description="Risk tier: Low, Medium, High.")
    contributing_factors: Dict[str, float] = Field(..., description="Feature importance mapping showing contributing weights.")
