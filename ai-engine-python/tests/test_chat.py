import json
import pytest

def test_ask_chatbot_success(client):
    """Verifies that non-streaming chat requests succeed and return structured citations."""
    response = client.post(
        "/api/v1/chat/",
        json={"prompt": "What is the policy for customs calculation?", "provider": "Claude"}
    )
    assert response.status_code == 200
    data = response.json()
    
    assert "response" in data
    assert "provider_used" in data
    assert "citations" in data
    assert isinstance(data["citations"], list)
    
    # Confirm fallback or retrieval-guided answer is present
    assert len(data["response"]) > 0
    
    # Confirm that citations matches the schema
    assert len(data["citations"]) > 0
    for citation in data["citations"]:
        assert "source" in citation
        assert "content_snippet" in citation
        assert len(citation["source"]) > 0
        assert len(citation["content_snippet"]) > 0


def test_ask_chatbot_validation_error(client):
    """Verifies that queries shorter than 2 characters fail query validation rules."""
    response = client.post(
        "/api/v1/chat/",
        json={"prompt": "A", "provider": "Claude"}
    )
    assert response.status_code == 422


def test_stream_chatbot_success(client):
    """Verifies that SSE streaming chat requests emit correct citations and token chunks."""
    response = client.post(
        "/api/v1/chat/stream",
        json={"prompt": "What is the policy for customs calculation?", "provider": "Claude"}
    )
    assert response.status_code == 200
    assert "text/event-stream" in response.headers["content-type"].lower()
    
    # Parse emitted SSE lines
    lines = response.text.split("\n")
    events = [line for line in lines if line.startswith("data: ")]
    
    assert len(events) >= 3 # citations, at least one token, done
    
    has_citations = False
    has_tokens = False
    has_done = False
    
    for event in events:
        raw_json = event[6:].strip() # Strip "data: "
        chunk = json.loads(raw_json)
        chunk_type = chunk.get("type")
        
        if chunk_type == "citations":
            has_citations = True
            assert isinstance(chunk["citations"], list)
            assert len(chunk["citations"]) > 0
            for cit in chunk["citations"]:
                assert "source" in cit
                assert "content_snippet" in cit
        elif chunk_type == "token":
            has_tokens = True
            assert "token" in chunk
            assert "provider" in chunk
        elif chunk_type == "done":
            has_done = True
            
    assert has_citations, "Stream failed to emit citations chunk"
    assert has_tokens, "Stream failed to emit text tokens"
    assert has_done, "Stream failed to emit done chunk"
