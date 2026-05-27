from typing import List, Optional
from pydantic import BaseModel, Field

class ChatQuery(BaseModel):
    prompt: str = Field(..., min_length=2, max_length=1000, description="The user question or query regarding logistics policies.")
    provider: Optional[str] = Field("Claude", description="The selected LLM Provider: Claude, GPT, Gemini")

class DocumentCitation(BaseModel):
    source: str = Field(..., description="Filename or identifier of the source document.")
    content_snippet: str = Field(..., description="Extract of relevant text.")

class StepInfo(BaseModel):
    id: int = Field(..., description="Step sequence identifier.")
    title: str = Field(..., description="Human-readable step description.")
    icon: str = Field(..., description="Icon key: search | db | insight | llm")
    status: str = Field(..., description="Step status: pending | running | completed")
    citations: Optional[List[DocumentCitation]] = Field(default=None, description="RAG citations matched by this step, if applicable.")

class ChatResponse(BaseModel):
    response: str = Field(..., description="Model response text.")
    provider_used: str = Field(..., description="The LLM provider that processed this request.")
    citations: List[DocumentCitation] = Field(default=[], description="Source material retrieved via RAG.")

