from fastapi import APIRouter
from app.api.v1.endpoints import chat, predict

api_router = APIRouter()
api_router.include_router(chat.router, prefix="/chat", tags=["RAG Chatbot"])
api_router.include_router(predict.router, prefix="/predict", tags=["XGBoost Risk Predictor"])
