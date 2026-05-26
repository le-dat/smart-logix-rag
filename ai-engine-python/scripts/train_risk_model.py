import os
import json
import numpy as np
import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.metrics import accuracy_score, f1_score
import xgboost as xgb

# Core Configuration
RESOURCES_DIR = os.path.join(os.path.dirname(os.path.dirname(__file__)), "app", "resources")
MODEL_PATH = os.path.join(RESOURCES_DIR, "xgb_risk_model.json")
MAPPING_PATH = os.path.join(RESOURCES_DIR, "feature_mappings.json")

def generate_synthetic_logistics_data(n_samples: int = 1000) -> pd.DataFrame:
    """Generates synthetic logistics dataset with realistic operational correlations."""
    np.random.seed(42)
    
    # 1. Base operational features
    route_ids = np.random.randint(1, 6, size=n_samples) # Route IDs 1 to 5
    carriers = np.random.choice(["Dimerco Air", "Express Ocean", "FastLogix", "DirectCarrier"], size=n_samples)
    weights = np.random.uniform(50.0, 5000.0, size=n_samples) # Cargo weight (kg)
    weather_indices = np.random.uniform(0.0, 5.0, size=n_samples) # 0 = Clear, 5 = Severe Storm
    historical_delay_ratios = np.random.uniform(0.02, 0.35, size=n_samples) # Historical delay percentages

    df = pd.DataFrame({
        "route_id": route_ids,
        "carrier": carriers,
        "weight": weights,
        "weather_index": weather_indices,
        "historical_delay_ratio": historical_delay_ratios
    })

    # 2. Correlate feature variables with target 'is_delayed'
    delay_probs = 0.05 + df["historical_delay_ratio"] # Base rate based on historical carrier data
    
    # Severe weather index spikes delay probabilities
    delay_probs += np.where(df["weather_index"] > 3.0, (df["weather_index"] - 3.0) * 0.25, 0.0)
    
    # Extremely heavy cargo adds delay factors
    delay_probs += np.where(df["weight"] > 3000.0, 0.15, 0.0)
    
    # Carrier specific handling inefficiencies
    delay_probs += np.where(df["carrier"] == "DirectCarrier", 0.10, 0.0)
    delay_probs -= np.where(df["carrier"] == "Dimerco Air", 0.05, 0.0)

    # Bound probabilities between 0.0 and 0.95
    delay_probs = np.clip(delay_probs, 0.0, 0.95)
    
    # Draw random outcomes
    df["is_delayed"] = np.random.binomial(1, delay_probs)
    return df

def train_model():
    print("[ML] Generating synthetic shipments dataset...")
    df = generate_synthetic_logistics_data(1000)
    print(f"[ML] Generated {df.shape[0]} records. Delay class ratio: {df['is_delayed'].mean() * 100.0:.1f}%")

    # 1. Preprocessing - Integer Encode Categorical Carrier
    carrier_mapping = {
        "Dimerco Air": 0,
        "Express Ocean": 1,
        "FastLogix": 2,
        "DirectCarrier": 3
    }
    df["carrier_encoded"] = df["carrier"].map(carrier_mapping)
    
    # Select feature set and target
    feature_cols = ["route_id", "carrier_encoded", "weight", "weather_index", "historical_delay_ratio"]
    X = df[feature_cols]
    y = df["is_delayed"]

    # 2. Split into Train & Validation subsets (Early Stopping compliant)
    X_train, X_val, y_train, y_val = train_test_split(X, y, test_size=0.2, random_state=42, stratify=y)
    print(f"[ML] Splitting: {X_train.shape[0]} train rows, {X_val.shape[0]} validation rows.")

    # 3. Train XGBoost Classifier
    model = xgb.XGBClassifier(
        n_estimators=300,
        max_depth=5,
        learning_rate=0.05,
        random_state=42,
        eval_metric="logloss",
        early_stopping_rounds=15
    )
    
    print("[ML] Initiating XGBoost training with validation early stopping...")
    model.fit(
        X_train, y_train,
        eval_set=[(X_val, y_val)],
        verbose=False
    )
    
    # 4. Evaluate metrics
    y_pred = model.predict(X_val)
    acc = accuracy_score(y_val, y_pred)
    f1 = f1_score(y_val, y_pred)
    print(f"[ML] Training completed. Best Iteration: {model.best_iteration}")
    print(f"[ML] Evaluation metrics on validation set: Accuracy = {acc*100.0:.2f}%, F1-Score = {f1*100.0:.2f}%")

    # 5. Export Model & Mappings
    os.makedirs(RESOURCES_DIR, exist_ok=True)
    model.save_model(MODEL_PATH)
    
    # Save categorical mappings to json
    with open(MAPPING_PATH, "w") as f:
        json.dump({"carrier_mapping": carrier_mapping, "features": feature_cols}, f, indent=4)
        
    print(f"[ML] Success! Exported model to {MODEL_PATH}")
    print(f"[ML] Success! Exported feature mappings to {MAPPING_PATH}")

if __name__ == "__main__":
    train_model()
