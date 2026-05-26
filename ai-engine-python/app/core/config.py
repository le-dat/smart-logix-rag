import os
from pydantic_settings import BaseSettings

class Settings(BaseSettings):
    PROJECT_NAME: str = "SmartLogix AI Engine"
    VERSION: str = "1.0.0"
    API_V1_STR: str = "/api/v1"
    
    # RAG settings
    CHROMA_PERSIST_DIR: str = os.getenv("CHROMA_PERSIST_DIR", "./chroma_db")
    EMBEDDING_MODEL: str = os.getenv("EMBEDDING_MODEL", "sentence-transformers/all-MiniLM-L6-v2") # Local offline embedding
    DATA_DIR: str = os.getenv("DATA_DIR", "../docs/data")
    
    # API Keys for online models (optional)
    OPENAI_API_KEY: str | None = os.getenv("OPENAI_API_KEY") or None
    ANTHROPIC_API_KEY: str | None = os.getenv("ANTHROPIC_API_KEY") or None
    
    # CORS settings
    CORS_ALLOWED_ORIGINS: str = os.getenv("CORS_ALLOWED_ORIGINS", "http://localhost:5173")
    
    # Model resources settings
    MODEL_RESOURCES_DIR: str = os.getenv("MODEL_RESOURCES_DIR", "app/resources")

    @property
    def USE_OFFLINE_MODE(self) -> bool:
        """Determines if the service should fall back to local computation if no keys are provided."""
        return not bool(self.OPENAI_API_KEY or self.ANTHROPIC_API_KEY)

    class Config:
        case_sensitive = True

settings = Settings()
