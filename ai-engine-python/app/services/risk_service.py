import os
import json
import logging
import pandas as pd
import numpy as np
import xgboost as xgb

from app.core.config import settings
from app.schemas.risk import RiskPredictionQuery, RiskPredictionResponse

# Setup standard logger
logger = logging.getLogger("smartlogix.ml")


class RiskService:
    """Service to load XGBoost classifier model and predict logistics delay risk probabilities."""

    def __init__(self):
        self.model_dir = settings.MODEL_RESOURCES_DIR
        self.model_path = os.path.join(self.model_dir, "xgb_risk_model.json")
        self.mapping_path = os.path.join(self.model_dir, "feature_mappings.json")
        self.model = None
        self.carrier_mapping = {}
        self.features = []
        
    def load_model_if_needed(self):
        """Loads the pre-trained XGBoost model and categorical mappings on startup."""
        if self.model is not None:
            return
            
        if not os.path.exists(self.model_path) or not os.path.exists(self.mapping_path):
            logger.warning(f"[ML] Warning: Model file or mappings not found under '{self.model_dir}'. Run training script first.")
            return

        try:
            # 1. Load XGBoost model
            self.model = xgb.XGBClassifier()
            self.model.load_model(self.model_path)
            
            # 2. Load mappings
            with open(self.mapping_path, "r") as f:
                mappings = json.load(f)
                self.carrier_mapping = mappings.get("carrier_mapping", {})
                self.features = mappings.get("features", [])
                
            logger.info(f"[ML] Successfully loaded pre-trained XGBoost model from {self.model_path}")
        except Exception as e:
            logger.error(f"[ML] Error loading model files: {e}")

    def predict(self, query: RiskPredictionQuery) -> RiskPredictionResponse:
        """Infers the delay risk probability of a shipment using the pre-trained XGBoost model."""
        self.load_model_if_needed()
        
        # Fallback if model could not be loaded
        if self.model is None:
            logger.warning("[ML] XGBoost model not loaded. Falling back to rule-based prediction.")
            return self._predict_fallback(query)
            
        try:
            # 1. Encode Carrier String to Categorical Integer
            if query.carrier not in self.carrier_mapping:
                logger.warning(f"[ML] Unknown carrier '{query.carrier}'. Defaulting encoded index to 1 (Express Ocean).")
            carrier_encoded = self.carrier_mapping.get(query.carrier, 1) # Default to Express Ocean index 1
            
            # 2. Build input DataFrame aligning with original training features
            input_df = pd.DataFrame([{
                "route_id": query.route_id,
                "carrier_encoded": carrier_encoded,
                "weight": query.weight,
                "weather_index": query.weather_index,
                "historical_delay_ratio": query.historical_delay_ratio
            }])
            
            # Reorder columns to match exactly
            input_df = input_df[self.features]
            
            # 3. Predict risk probability
            probabilities = self.model.predict_proba(input_df)
            risk_score = float(probabilities[0][1]) * 100.0 # Extract probability of delay (class 1)
            risk_score = round(risk_score, 2)
            
            # 4. Classify risk level
            if risk_score < 35.0:
                risk_tier = "Low"
            elif risk_score < 70.0:
                risk_tier = "Medium"
            else:
                risk_tier = "High"
                
            # 5. Calculate Simplified Heuristic Feature Importance Attributions
            # Note: This is a fast heuristic scaling global feature importances by current feature values,
            # not formal SHAP values, to provide light explainability without dependency overhead.
            importances = self.model.feature_importances_
            
            # Base contributions factoring in feature values
            base = importances[0] * 10.0 # Route
            carrier_val = importances[1] * (2.0 if carrier_encoded == 3 else 1.0) # Carrier
            weight_val = importances[2] * (query.weight / 5000.0) # Weight
            weather_val = importances[3] * (query.weather_index / 5.0) # Weather index
            delay_val = importances[4] * query.historical_delay_ratio # Delay ratio
            
            total = base + carrier_val + weight_val + weather_val + delay_val
            if total > 0:
                factors = {
                    "Weather severity index": round((weather_val / total) * 100.0, 1),
                    "Chosen Carrier handling capability": round((carrier_val / total) * 100.0, 1),
                    "Cargo total weight factor": round((weight_val / total) * 100.0, 1),
                    "Baseline route operational risk": round((base / total) * 100.0, 1),
                    "Carrier historical delay ratio": round((delay_val / total) * 100.0, 1)
                }
            else:
                # All features have zero contribution - use equal distribution as fallback
                factors = {
                    "Weather severity index": 20.0,
                    "Chosen Carrier handling capability": 20.0,
                    "Cargo total weight factor": 20.0,
                    "Baseline route operational risk": 20.0,
                    "Carrier historical delay ratio": 20.0
                }
                logger.warning("[ML] Warning: All feature contributions are zero. Using equal factor distribution.")
                
            return RiskPredictionResponse(
                shipment_id=query.shipment_id,
                risk_score=risk_score,
                risk_level=risk_tier,
                contributing_factors=factors,
                is_fallback=False
            )
            
        except Exception as e:
            logger.error(f"[ML] Inference failed: {e}. Falling back to rule-based prediction.")
            return self._predict_fallback(query)

    def _predict_fallback(self, query: RiskPredictionQuery) -> RiskPredictionResponse:
        """Fallback rule-based prediction if model is missing or fails."""
        base_probability = 15.0
        weather_contribution = min(query.weather_index * 10.0, 50.0)
        weight_contribution = min(query.weight / 1000.0, 15.0)
        delay_contribution = query.historical_delay_ratio * 20.0
        
        calculated_probability = min(base_probability + weather_contribution + weight_contribution + delay_contribution, 100.0)
        calculated_probability = round(calculated_probability, 2)
        
        if calculated_probability < 35.0:
            risk_tier = "Low"
        elif calculated_probability < 70.0:
            risk_tier = "Medium"
        else:
            risk_tier = "High"
            
        total = weather_contribution + weight_contribution + delay_contribution + base_probability
        factors = {
            "Weather severity index": round((weather_contribution / total) * 100.0, 1) if total > 0 else 25.0,
            "Carrier historical delay ratio": round((delay_contribution / total) * 100.0, 1) if total > 0 else 25.0,
            "Cargo total weight factor": round((weight_contribution / total) * 100.0, 1) if total > 0 else 25.0,
            "Baseline route operational risk": round((base_probability / total) * 100.0, 1) if total > 0 else 25.0
        }
        
        return RiskPredictionResponse(
            shipment_id=query.shipment_id,
            risk_score=calculated_probability,
            risk_level=risk_tier,
            contributing_factors=factors,
            is_fallback=True
        )


# Singleton instance
risk_service = RiskService()
