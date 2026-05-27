import os
import glob
import logging
import asyncio
import atexit
from concurrent.futures import ThreadPoolExecutor
from typing import List, Tuple, Dict, Any, AsyncGenerator, Optional

from app.core.config import settings
from app.schemas.chat import DocumentCitation

# Setup logging
logger = logging.getLogger("smartlogix.rag")

# LangChain & ChromaDB
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_community.document_loaders import TextLoader
from langchain_chroma import Chroma


class SafeMockEmbeddings:
    """Safe fallback embedding model if network / caching fails in offline environments."""

    def embed_documents(self, texts: List[str]) -> List[List[float]]:
        return [[0.1] * 384 for _ in texts]

    def embed_query(self, text: str) -> List[float]:
        return [0.1] * 384


class EmbeddingManager:
    """Responsible for loading, caching, and failing back embedding models."""

    def __init__(self):
        self._embeddings = None

    def get_embeddings(self):
        """Initializes and returns the appropriate embedding model based on settings."""
        if self._embeddings is not None:
            return self._embeddings

        # Always use local HuggingFace embeddings for RAG to remain free and offline-capable for search
        try:
            from langchain_community.embeddings import HuggingFaceEmbeddings
            logger.info(f"[RAG] Loading local HuggingFace embeddings using model: {settings.EMBEDDING_MODEL}")
            self._embeddings = HuggingFaceEmbeddings(model_name=settings.EMBEDDING_MODEL)
        except Exception as e:
            logger.warning(
                f"[RAG] Warning loading HuggingFace local embeddings: {e}. "
                f"Falling back to lightweight dummy embedding model."
            )
            self._embeddings = SafeMockEmbeddings()

        return self._embeddings


class LLMFactory:
    """Responsible for creating LangChain LLM instances using MiniMax."""

    @staticmethod
    def get_llm(provider: str):
        from langchain_openai import ChatOpenAI

        provider_name = provider.strip().lower()

        # Support MiniMax online LLM synthesis
        if provider_name == "minimax":
            if settings.MINIMAX_API_KEY:
                logger.info("[RAG] Initializing MiniMax-M2.5 LLM.")
                return ChatOpenAI(
                    model="MiniMax-M2.5",
                    openai_api_key=settings.MINIMAX_API_KEY,
                    openai_api_base="https://api.minimax.io/v1",
                    timeout=15.0
                )
            else:
                raise ValueError("MINIMAX_API_KEY is not configured for online synthesis.")
        else:
            # If user requested another provider but we only have MiniMax configured,
            # gracefully warn and fallback to MiniMax instead of failing.
            logger.warning(
                f"[RAG] Requested provider '{provider}' is not supported under the current local + MiniMax configuration. "
                f"Gracefully falling back to MiniMax."
            )
            if settings.MINIMAX_API_KEY:
                return ChatOpenAI(
                    model="MiniMax-M2.5",
                    openai_api_key=settings.MINIMAX_API_KEY,
                    openai_api_base="https://api.minimax.io/v1",
                    timeout=15.0
                )
            else:
                raise ValueError(f"Requested provider '{provider}' is not supported and MINIMAX_API_KEY is missing.")


class RAGService:
    """Service to load documents, manage vector search, and synthesize answers using RAG."""

    def __init__(self):
        self.persist_directory = settings.CHROMA_PERSIST_DIR
        self.collection_name = "logistics-faq"
        self._embedding_manager = EmbeddingManager()
        self._vectorstore = None
        self._executor = ThreadPoolExecutor(max_workers=4)
        # Register executor shutdown with atexit to prevent thread/resource leaks on process exit
        atexit.register(self._executor.shutdown, wait=False)

    def get_embeddings(self):
        """Returns embedding model (delegated to EmbeddingManager)."""
        return self._embedding_manager.get_embeddings()

    def get_vectorstore(self) -> Chroma:
        """Loads or creates the Chroma persistent vector database."""
        if self._vectorstore is not None:
            return self._vectorstore

        embeddings = self.get_embeddings()
        self._vectorstore = Chroma(
            persist_directory=self.persist_directory,
            embedding_function=embeddings,
            collection_name=self.collection_name
        )
        return self._vectorstore

    def ingest_documents(self):
        """Loads logistics FAQs, chunks them, and indexes vector embeddings into ChromaDB."""
        data_dir = settings.DATA_DIR
        if not os.path.exists(data_dir):
            logger.warning(f"[RAG] Data directory '{data_dir}' not found. Creating a fresh folder.")
            os.makedirs(data_dir, exist_ok=True)
            return

        from langchain_community.document_loaders import DirectoryLoader

        try:
            logger.info(f"[RAG] Loading documents from directory: '{data_dir}'")
            # Load both .txt and .md files
            loader_txt = DirectoryLoader(
                data_dir,
                glob="*.txt",
                loader_cls=TextLoader,
                loader_kwargs={"encoding": "utf-8"}
            )
            loader_md = DirectoryLoader(
                data_dir,
                glob="*.md",
                loader_cls=TextLoader,
                loader_kwargs={"encoding": "utf-8"}
            )
            docs = loader_txt.load() + loader_md.load()
        except Exception as e:
            logger.error(f"[RAG] DirectoryLoader failed: {e}")
            return

        if not docs:
            logger.warning(f"[RAG] No FAQ txt documents found under '{data_dir}' to ingest.")
            return

        # Normalize source metadata to simple filenames for clean citations
        for doc in docs:
            if "source" in doc.metadata:
                doc.metadata["source"] = os.path.basename(doc.metadata["source"])

        splitter = RecursiveCharacterTextSplitter(chunk_size=600, chunk_overlap=80)
        all_splits = splitter.split_documents(docs)

        logger.info(f"[RAG] Loaded {len(all_splits)} text chunks from directory documents.")

        if all_splits:
            embeddings = self.get_embeddings()
            # Overwrite persistent ChromaDB store
            self._vectorstore = Chroma.from_documents(
                documents=all_splits,
                embedding=embeddings,
                persist_directory=self.persist_directory,
                collection_name=self.collection_name
            )
            logger.info(f"[RAG] Ingestion completed. Indexed {len(all_splits)} total splits into persistent ChromaDB.")

    def _extract_chunks_and_citations(self, results_with_score) -> Tuple[str, List[dict]]:
        """Helper to extract clean page contents and citations from vector database search results."""
        citations = []
        context_chunks = []
        for doc, score in results_with_score:
            if not doc.page_content.strip():
                continue
            source_file = doc.metadata.get("source", "Unknown Document")
            citations.append({
                "source": source_file,
                "content_snippet": doc.page_content
            })
            context_chunks.append(doc.page_content)
        
        context_str = "\n\n".join(context_chunks)
        return context_str, citations

    def _synthesize_offline_fallback(self, context_str: str) -> str:
        """Helper to synthesize answers offline when LLM credentials are not provided."""
        if not context_str.strip():
            return "I'm sorry, I couldn't find any relevant logistics policies in the internal database to answer your question."
        
        return (
            f"Based on internal Dimerco operational guidelines, I retrieved the following information:\n\n"
            f"{context_str}\n\n"
            f"*(Response synthesized offline in compliance with local logistics policies)*"
        )

    def _build_llm_messages(self, context_str: str, prompt: str) -> List[Any]:
        """Helper to build LangChain SystemMessage and HumanMessage structure."""
        from langchain_core.messages import SystemMessage, HumanMessage
        return [
            SystemMessage(content=(
                "You are the SmartLogix AI Assistant, a logistics copilot for Dimerco. "
                "Synthesize an answer using ONLY the retrieved context below. "
                "Be concise, highly professional, and cite rules accurately. "
                f"Retrieved Context:\n{context_str}"
            )),
            HumanMessage(content=prompt)
        ]

    def query(self, prompt: str, provider: str = "Claude") -> Tuple[str, List[DocumentCitation]]:
        """Retrieves context from ChromaDB and synthesizes an answer using the selected strategy."""
        vectorstore = self.get_vectorstore()

        # Retrieve documents
        try:
            results_with_score = vectorstore.similarity_search_with_score(prompt, k=3)
            # Auto-ingest if database returns empty
            if not results_with_score:
                logger.info("[RAG] ChromaDB returned 0 entries. Triggering auto-ingestion...")
                self.ingest_documents()
                vectorstore = self.get_vectorstore()
                results_with_score = vectorstore.similarity_search_with_score(prompt, k=3)
        except Exception as e:
            logger.warning(f"[RAG] Vector store not initialized or empty ({e}). Running initial auto-ingestion...")
            self.ingest_documents()
            vectorstore = self.get_vectorstore()
            results_with_score = vectorstore.similarity_search_with_score(prompt, k=3)

        # Prepare context and citations
        context_str, raw_citations = self._extract_chunks_and_citations(results_with_score)
        citations = [
            DocumentCitation(source=c["source"], content_snippet=c["content_snippet"])
            for c in raw_citations
        ]

        # Synthesize answer
        if settings.USE_OFFLINE_MODE:
            answer = self._synthesize_offline_fallback(context_str)
        else:
            try:
                llm = LLMFactory.get_llm(provider)
                messages = self._build_llm_messages(context_str, prompt)
                res = llm.invoke(messages)
                answer = res.content
            except Exception as e:
                logger.error(f"[RAG] Online LLM synthesis failed: {e}. Falling back to local offline synthesis.")
                answer = (
                    f"Failed to connect to online LLM ({e}). "
                    f"Falling back to local offline synthesis:\n\n{context_str}"
                )

        return answer, citations

    async def _stream_simulated(self, answer: str, provider: str) -> AsyncGenerator[dict, None]:
        """Helper generator to stream text word-by-word with delay."""
        words = answer.split(" ")
        for i, word in enumerate(words):
            token = word if i == 0 else " " + word
            yield {"type": "token", "token": token, "provider": provider}
            await asyncio.sleep(0.025)  # ~40 words/sec simulated typewriter

    async def stream_query(self, prompt: str, provider: str = "Claude") -> AsyncGenerator[dict, None]:
        """
        Async generator that streams the RAG response as SSE chunks.
        Each yielded dict has keys:
          - {"type": "citations", "citations": [...]}              — source citations
          - {"type": "token", "token": "...", "provider": "..."}  — incremental text
          - {"type": "step", "step": {...}}                         — agent processing step progress
          - {"type": "done"}                                       — terminal signal
        """
        loop = asyncio.get_event_loop()

        # Step 1: Initialize query processing
        yield {"type": "step", "step": {"id": 1, "title": "Khởi tạo luồng truy vấn dữ liệu SmartLogix", "icon": "search", "status": "running"}}
        yield {"type": "step", "step": {"id": 1, "title": "Khởi tạo luồng truy vấn dữ liệu SmartLogix", "icon": "search", "status": "completed"}}

        # Step 2: Database Query
        yield {"type": "step", "step": {"id": 2, "title": "Đang kết nối và tra cứu dữ liệu ChromaDB", "icon": "db", "status": "running"}}

        # Run vector search in a thread pool to avoid blocking the event loop
        try:
            results_with_score = await loop.run_in_executor(
                self._executor,
                lambda: self.get_vectorstore().similarity_search_with_score(prompt, k=3)
            )
            if not results_with_score:
                logger.info("[RAG Stream] ChromaDB returned 0 entries. Triggering auto-ingestion...")
                self.ingest_documents()
                results_with_score = await loop.run_in_executor(
                    self._executor,
                    lambda: self.get_vectorstore().similarity_search_with_score(prompt, k=3)
                )
        except Exception as e:
            logger.warning(f"[RAG Stream] Vector search failed ({e}), running ingest...")
            self.ingest_documents()
            results_with_score = await loop.run_in_executor(
                self._executor,
                lambda: self.get_vectorstore().similarity_search_with_score(prompt, k=3)
            )

        yield {"type": "step", "step": {"id": 2, "title": "Đang kết nối và tra cứu dữ liệu ChromaDB", "icon": "db", "status": "completed"}}

        # Step 3: Extract relevant context
        yield {"type": "step", "step": {"id": 3, "title": "Đang trích xuất và đối sánh tài liệu liên quan (RAG Match)", "icon": "insight", "status": "running"}}
        context_str, citations = self._extract_chunks_and_citations(results_with_score)
        yield {
            "type": "step",
            "step": {
                "id": 3,
                "title": "Đang trích xuất và đối sánh tài liệu liên quan (RAG Match)",
                "icon": "insight",
                "status": "completed",
                "citations": citations  # Inline RAG matches for this step
            }
        }

        # Step 4: Synthesize query & initialize LLM
        yield {"type": "step", "step": {"id": 4, "title": "Đang phân tích thông tin và khởi chạy mô hình MiniMax-M2.5", "icon": "llm", "status": "running"}}
        yield {"type": "step", "step": {"id": 4, "title": "Đang phân tích thông tin và khởi chạy mô hình MiniMax-M2.5", "icon": "llm", "status": "completed"}}

        # Stream citations first
        yield {"type": "citations", "citations": citations}

        # Synthesize and stream the response
        if settings.USE_OFFLINE_MODE:
            answer = self._synthesize_offline_fallback(context_str)
            async for chunk in self._stream_simulated(answer, provider):
                yield chunk
        else:
            try:
                llm = LLMFactory.get_llm(provider)
                messages = self._build_llm_messages(context_str, prompt)

                # Native async streaming using LangChain's astream API
                async for chunk in llm.astream(messages):
                    if chunk.content:
                        yield {"type": "token", "token": chunk.content, "provider": provider}
            except Exception as e:
                logger.error(f"[RAG Stream] Online streaming failed: {e}. Falling back to simulated stream.")
                answer = (
                    f"Failed to connect to online LLM ({e}). "
                    f"Falling back to local offline synthesis:\n\n{context_str}"
                )
                async for chunk in self._stream_simulated(answer, provider):
                    yield chunk

        yield {"type": "done"}


# Singleton instance
rag_service = RAGService()