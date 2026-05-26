from fastapi import APIRouter, HTTPException
from app.schemas.chat import ChatQuery, ChatResponse
from app.services.rag_service import rag_service

router = APIRouter()

@router.post("/", response_model=ChatResponse)
async def ask_chatbot(query: ChatQuery):
    """
    RAG-powered Logistics Policy Assistant Chatbot.
    Retrieves policy context from ChromaDB and invokes the selected LLM Provider.
    """
    try:
        response_text, citations = rag_service.query(query.prompt, query.provider)
        
        return ChatResponse(
            response=response_text,
            provider_used=query.provider or "Claude",
            citations=citations
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
