from fastapi import APIRouter, HTTPException, Depends

from app.schemas.risk import RiskPredictionQuery, RiskPredictionResponse
from app.services.risk_service import RiskService
from app.api.dependencies import get_risk_service

router = APIRouter()


@router.post("/", response_model=RiskPredictionResponse)
async def predict_risk(
    query: RiskPredictionQuery,
    risk_service: RiskService = Depends(get_risk_service)
):
    """
    XGBoost ML Early Delay Risk Assessment.
    Analyzes routing features, carrier history, cargo weight, and weather indices to infer delay risk probability.
    """
    try:
        response = risk_service.predict(query)
        return response
    except Exception as e:
        raise HTTPException(status_code=500, detail="An internal error occurred. Please try again later.")
