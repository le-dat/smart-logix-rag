import sys
import typing
import logging

from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.core.config import settings
from app.api.v1.router import api_router

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



@asynccontextmanager
async def lifespan(app: FastAPI):
    # Safely preload singletons on startup to prevent thread race conditions under load
    from app.services.risk_service import risk_service
    from app.services.rag_service import rag_service
    
    logger = logging.getLogger("smartlogix.startup")
    
    try:
        risk_service.load_model_if_needed()
    except Exception as e:
        logger.error(f"XGBoost preloading failed: {e}")
        
    try:
        # Pre-ingest faq documents on startup if DB is empty
        rag_service.ingest_documents()
    except Exception as e:
        logger.error(f"RAG auto-ingestion preloading failed: {e}")
        
    yield

app = FastAPI(
    title=settings.PROJECT_NAME,
    description="Python FastAPI Service for RAG (ChromaDB) and ML Risk Predictions (XGBoost)",
    version=settings.VERSION,
    docs_url="/docs",
    redoc_url="/redoc",
    lifespan=lifespan
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
