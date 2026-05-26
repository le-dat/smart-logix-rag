import os
from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    PROJECT_NAME: str = "SmartLogix AI Engine"
    VERSION: str = "1.0.0"
    API_V1_STR: str = "/api/v1"
    
    # RAG Settings
    CHROMA_PERSIST_DIR: str = os.getenv("CHROMA_PERSIST_DIR", "./chroma_db")
    EMBEDDING_MODEL: str = os.getenv("EMBEDDING_MODEL", "sentence-transformers/all-MiniLM-L6-v2") # Default to local offline embedding
    
    class Config:
        case_sensitive = True

settings = Settings()
