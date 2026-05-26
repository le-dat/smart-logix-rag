import json
from fastapi import APIRouter, HTTPException
from fastapi.responses import StreamingResponse
from app.schemas.chat import ChatQuery, ChatResponse
from app.services.rag_service import rag_service

router = APIRouter()

@router.post("/", response_model=ChatResponse)
async def ask_chatbot(query: ChatQuery):
    """
    RAG-powered Logistics Policy Assistant Chatbot (non-streaming).
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
        raise HTTPException(status_code=500, detail="An internal error occurred. Please try again later.")


@router.post("/stream")
async def stream_chatbot(query: ChatQuery):
    """
    SSE streaming chat endpoint. Yields JSON chunks:
    - {"type": "token", "token": "<chunk>", "provider": "<name>"}
    - {"type": "citations", "citations": [...]}
    - {"type": "done"}
    """
    async def event_generator():
        try:
            async for chunk in rag_service.stream_query(query.prompt, query.provider or "Claude"):
                yield f"data: {json.dumps(chunk)}\n\n"
        except Exception as e:
            yield f"data: {json.dumps({'type': 'error', 'message': 'An internal error occurred. Please try again later.'})}\n\n"

    return StreamingResponse(
        event_generator(),
        media_type="text/event-stream",
        headers={
            "Cache-Control": "no-cache",
            "Connection": "keep-alive",
            "X-Accel-Buffering": "no",
        }
    )

