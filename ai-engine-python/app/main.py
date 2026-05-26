import sys
import typing
import logging

# Configure structured logging
logging.basicConfig(level=logging.INFO, format="%(asctime)s - %(name)s - %(levelname)s - %(message)s")

# Monkey-patch ForwardRef._evaluate for Python 3.12 compatibility with Pydantic v1 / LangChain
if sys.version_info >= (3, 12):
    try:
        from typing import ForwardRef
        if hasattr(ForwardRef, "_evaluate"):
            original_evaluate = ForwardRef._evaluate
            def patched_evaluate(self, *args, **kwargs):
                if "recursive_guard" in kwargs:
                    return original_evaluate(self, *args, **kwargs)
                if len(args) == 3:
                    return original_evaluate(self, args[0], args[1], None, recursive_guard=args[2])
                return original_evaluate(self, *args, **kwargs)
            ForwardRef._evaluate = patched_evaluate
    except Exception as e:
        print(f"Warning: failed to patch ForwardRef._evaluate: {e}")

from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.core.config import settings
from app.api.v1.router import api_router

app = FastAPI(
    title=settings.PROJECT_NAME,
    description="Python FastAPI Service for RAG (ChromaDB) and ML Risk Predictions (XGBoost)",
    version=settings.VERSION,
    docs_url="/docs",
    redoc_url="/redoc"
)

# Configure CORS
origins = [origin.strip() for origin in settings.CORS_ALLOWED_ORIGINS.split(",") if origin.strip()]
app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Register central routers
app.include_router(api_router, prefix=settings.API_V1_STR)

@app.get("/")
async def root():
    return {
        "status": "healthy",
        "service": settings.PROJECT_NAME,
        "version": settings.VERSION,
        "features_available": [
            "RAG Logistics QA Chatbot (Week 2 - Framework Ready)",
            "XGBoost Delay Risk Classifier (Week 2 - Framework Ready)"
        ]
    }
