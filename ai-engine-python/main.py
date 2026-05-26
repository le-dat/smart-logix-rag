import os
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI(
    title="SmartLogix AI Engine",
    description="Python FastAPI Service for RAG (ChromaDB) and ML Risk Predictions (XGBoost)",
    version="1.0.0"
)

# Configure CORS
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"], # Tighten in production
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

@app.get("/")
async def root():
    return {
        "status": "healthy",
        "service": "SmartLogix AI Engine",
        "features_available": [
            "RAG Logistics QA Chatbot (Week 2)",
            "XGBoost Delay Risk Classifier (Week 2)"
        ]
    }

if __name__ == "__main__":
    import uvicorn
    port = int(os.getenv("PORT", 8000))
    host = os.getenv("HOST", "0.0.0.0")
    uvicorn.run(app, host=host, port=port)
