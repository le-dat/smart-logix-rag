from app.services.rag_service import rag_service, RAGService
from app.services.risk_service import risk_service, RiskService

def get_rag_service() -> RAGService:
    """
    Dependency provider for RAGService.
    Decoupled via Depends to allow service mocking in testing.
    """
    return rag_service

def get_risk_service() -> RiskService:
    """
    Dependency provider for RiskService.
    Decoupled via Depends to allow service mocking in testing.
    """
    return risk_service
