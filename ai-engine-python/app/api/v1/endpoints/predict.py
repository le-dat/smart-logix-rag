from fastapi import APIRouter, HTTPException
from app.schemas.risk import RiskPredictionQuery, RiskPredictionResponse

router = APIRouter()

@router.post("/", response_model=RiskPredictionResponse)
async def predict_risk(query: RiskPredictionQuery):
    """
    XGBoost ML Early Delay Risk Assessment.
    Analyzes routing features, carrier history, cargo weight, and weather indices to infer delay risk probability.
    """
    try:
        # Mocking XGBoost inference calculations for modular framework
        # Calculate a mock risk probability based on features to show system interactivity
        base_probability = 15.0 # baseline
        
        # Weather adds up to 50% risk
        weather_contribution = min(query.weather_index * 10.0, 50.0)
        
        # High weight adds up to 15% risk
        weight_contribution = min(query.weight / 1000.0, 15.0)
        
        # Historical carrier delays add up to 20% risk
        delay_contribution = query.historical_delay_ratio * 20.0
        
        calculated_probability = min(base_probability + weather_contribution + weight_contribution + delay_contribution, 100.0)
        calculated_probability = round(calculated_probability, 2)
        
        if calculated_probability < 35.0:
            risk_tier = "Low"
        elif calculated_probability < 70.0:
            risk_tier = "Medium"
        else:
            risk_tier = "High"
            
        # Feature Importance / Contributing factors for Explainable AI (XAI)
        total_contributions = weather_contribution + weight_contribution + delay_contribution + base_probability
        factors = {
            "Weather severity index": round((weather_contribution / total_contributions) * 100.0, 1) if total_contributions > 0 else 0.0,
            "Chosen Carrier historical delay ratio": round((delay_contribution / total_contributions) * 100.0, 1) if total_contributions > 0 else 0.0,
            "Cargo total weight factor": round((weight_contribution / total_contributions) * 100.0, 1) if total_contributions > 0 else 0.0,
            "Baseline route operational risk": round((base_probability / total_contributions) * 100.0, 1) if total_contributions > 0 else 0.0
        }
        
        return RiskPredictionResponse(
            shipment_id=query.shipment_id,
            risk_score=calculated_probability,
            risk_level=risk_tier,
            contributing_factors=factors
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
