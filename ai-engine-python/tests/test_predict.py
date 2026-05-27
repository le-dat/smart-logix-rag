import pytest

def test_predict_risk_success(client):
    """Verifies that early delay risk predictions succeed and yield correct metrics and factors."""
    payload = {
        "shipment_id": 89212,
        "route_id": 3,
        "carrier": "Express Ocean",
        "weight": 2500.0,
        "weather_index": 2.4,
        "historical_delay_ratio": 0.12
    }
    response = client.post("/api/v1/predict/", json=payload)
    assert response.status_code == 200
    data = response.json()
    
    assert data["shipment_id"] == 89212
    assert "risk_score" in data
    assert "risk_level" in data
    assert "contributing_factors" in data
    assert "is_fallback" in data
    
    # Assert type schemas and output bounds
    assert isinstance(data["risk_score"], float)
    assert 0.0 <= data["risk_score"] <= 100.0
    assert data["risk_level"] in ["Low", "Medium", "High"]
    
    # Assert contributing factors format
    factors = data["contributing_factors"]
    assert isinstance(factors, dict)
    assert len(factors) > 0
    
    # Confirm fallback or active model output
    assert "Weather severity index" in factors
    assert "Baseline route operational risk" in factors
    assert "Cargo total weight factor" in factors


def test_predict_risk_validation_error(client):
    """Verifies that prediction requests fail when required fields are missing."""
    payload = {
        "shipment_id": 89212,
        "route_id": 3,
        # missing carrier, weather_index
    }
    response = client.post("/api/v1/predict/", json=payload)
    assert response.status_code == 422
