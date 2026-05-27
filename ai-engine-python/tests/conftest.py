import os
import shutil
import pytest
from fastapi.testclient import TestClient

# Force offline configuration and test DB isolation BEFORE importing the application settings
os.environ["USE_OFFLINE_MODE"] = "True"
os.environ["CHROMA_PERSIST_DIR"] = "./chroma_db_test"
os.environ["DATA_DIR"] = "../docs/data"
os.environ["CORS_ALLOWED_ORIGINS"] = "http://localhost:5173"

from app.main import app
from app.services.rag_service import rag_service


@pytest.fixture(scope="session", autouse=True)
def setup_test_db():
    """
    Initializes a fresh, isolated test Chroma database for testing.
    Executes teardown at the end of the test suite run.
    """
    test_db_dir = "./chroma_db_test"
    if os.path.exists(test_db_dir):
        shutil.rmtree(test_db_dir)

    # Ingest standard logistics documents into the isolated test DB
    rag_service.ingest_documents()

    yield

    # Clean up test database directory
    if os.path.exists(test_db_dir):
        try:
            shutil.rmtree(test_db_dir)
        except Exception as e:
            print(f"Warning during test database cleanup: {e}")


@pytest.fixture
def client():
    """
    Returns a FastAPI TestClient.
    Using 'with' statement triggers application lifespan startup and shutdown hooks.
    """
    with TestClient(app) as test_client:
        yield test_client
