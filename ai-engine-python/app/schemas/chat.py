from typing import List, Optional
from pydantic import BaseModel, Field

class ChatQuery(BaseModel):
    prompt: str = Field(..., min_length=2, max_length=1000, description="The user question or query regarding logistics policies.")
    provider: Optional[str] = Field("Claude", description="The selected LLM Provider: Claude, GPT, Gemini")

class DocumentCitation(BaseModel):
    source: str = Field(..., description="Filename or identifier of the source document.")
    content_snippet: str = Field(..., description="Extract of relevant text.")

class ChatResponse(BaseModel):
    response: str = Field(..., description="Model response text.")
    provider_used: str = Field(..., description="The LLM provider that processed this request.")
    citations: List[DocumentCitation] = Field(default=[], description="Source material retrieved via RAG.")
