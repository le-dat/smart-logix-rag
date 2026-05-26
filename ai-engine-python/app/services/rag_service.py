import os
import glob
import logging
import asyncio
from concurrent.futures import ThreadPoolExecutor
from typing import List, Tuple
from app.core.config import settings
from app.schemas.chat import DocumentCitation

# Setup logging redirect
print = logging.getLogger("smartlogix.rag").info

# LangChain & ChromaDB
from langchain_text_splitters import RecursiveCharacterTextSplitter
from langchain_community.document_loaders import TextLoader
from langchain_chroma import Chroma

class RAGService:
    def __init__(self):
        self.persist_directory = settings.CHROMA_PERSIST_DIR
        self.collection_name = "logistics-faq"
        self._embeddings = None
        self._vectorstore = None
        self._executor = ThreadPoolExecutor(max_workers=4)

    def get_embeddings(self):
        """Initializes and returns the appropriate embedding model based on Settings."""
        if self._embeddings is not None:
            return self._embeddings

        if settings.USE_OFFLINE_MODE:
            try:
                # Load HuggingFace local embedding model
                from langchain_community.embeddings import HuggingFaceEmbeddings
                self._embeddings = HuggingFaceEmbeddings(model_name=settings.EMBEDDING_MODEL)
            except Exception as e:
                print(f"[RAG] Warning loading HuggingFace local embeddings: {e}. Falling back to lightweight dummy embedding model.")
                # Safe fallback if network / caching fails in offline environments
                class SafeMockEmbeddings:
                    def embed_documents(self, texts):
                        return [[0.1] * 384 for _ in texts]
                    def embed_query(self, text):
                        return [0.1] * 384
                self._embeddings = SafeMockEmbeddings()
        else:
            from langchain_openai import OpenAIEmbeddings
            self._embeddings = OpenAIEmbeddings(openai_api_key=settings.OPENAI_API_KEY)

        return self._embeddings

    def get_vectorstore(self) -> Chroma:
        """Loads or creates the Chroma persistent vector database."""
        if self._vectorstore is not None:
            return self._vectorstore

        embeddings = self.get_embeddings()
        self._vectorstore = Chroma(
            persist_directory=self.persist_directory,
            embedding=embeddings,
            collection_name=self.collection_name
        )
        return self._vectorstore

    def ingest_documents(self):
        """Loads logistics FAQs, chunks them, and indexes vector embeddings into ChromaDB."""
        data_dir = settings.DATA_DIR
        if not os.path.exists(data_dir):
            print(f"[RAG] Data directory '{data_dir}' not found. Creating a fresh folder.")
            os.makedirs(data_dir, exist_ok=True)
            return

        txt_files = glob.glob(os.path.join(data_dir, "*.txt"))
        if not txt_files:
            print(f"[RAG] No FAQ txt documents found under '{data_dir}' to ingest.")
            return

        all_splits = []
        splitter = RecursiveCharacterTextSplitter(chunk_size=600, chunk_overlap=80)

        for filepath in txt_files:
            try:
                loader = TextLoader(filepath, encoding="utf-8")
                docs = loader.load()
                splits = splitter.split_documents(docs)
                filename = os.path.basename(filepath)
                for split in splits:
                    split.metadata["source"] = filename
                all_splits.extend(splits)
                print(f"[RAG] Loaded {len(splits)} text chunks from {filename}")
            except Exception as e:
                print(f"[RAG] Error parsing {filepath}: {e}")

        if all_splits:
            embeddings = self.get_embeddings()
            # Overwrite persistent ChromaDB store
            self._vectorstore = Chroma.from_documents(
                documents=all_splits,
                embedding=embeddings,
                persist_directory=self.persist_directory,
                collection_name=self.collection_name
            )
            print(f"[RAG] Ingestion completed. Indexed {len(all_splits)} total splits into persistent ChromaDB.")

    def _run_sync(self, fn, *args):
        """Run a synchronous function in the thread pool."""
        loop = asyncio.new_event_loop()
        asyncio.set_event_loop(loop)
        try:
            return fn(*args)
        finally:
            loop.close()

    def query(self, prompt: str, provider: str = "Claude") -> Tuple[str, List[DocumentCitation]]:
        """Retrieves context from ChromaDB and synthesizes an answer using the selected strategy."""
        vectorstore = self.get_vectorstore()

        # Safe query block with auto-ingestion trigger
        try:
            results_with_score = vectorstore.similarity_search_with_score(prompt, k=3)
            # Auto-ingest if database returns empty
            if not results_with_score:
                print("[RAG] ChromaDB returned 0 entries. Triggering auto-ingestion...")
                self.ingest_documents()
                vectorstore = self.get_vectorstore()
                results_with_score = vectorstore.similarity_search_with_score(prompt, k=3)
        except Exception as e:
            print(f"[RAG] Vector store not initialized or empty ({e}). Running initial auto-ingestion...")
            self.ingest_documents()
            vectorstore = self.get_vectorstore()
            results_with_score = vectorstore.similarity_search_with_score(prompt, k=3)

        citations = []
        context_chunks = []
        for doc, score in results_with_score:
            # Skip noise or empty chunks
            if not doc.page_content.strip():
                continue
            source_file = doc.metadata.get("source", "Unknown Document")
            citations.append(DocumentCitation(source=source_file, content_snippet=doc.page_content))
            context_chunks.append(doc.page_content)

        context_str = "\n\n".join(context_chunks)

        if settings.USE_OFFLINE_MODE:
            # High-quality offline local context synthesis
            if not citations:
                answer = "I'm sorry, I couldn't find any relevant logistics policies in the internal database to answer your question."
            else:
                answer = (
                    f"Based on internal Dimerco operational guidelines, I retrieved the following information:\n\n"
                    f"{context_str}\n\n"
                    f"*(Response synthesized offline in compliance with local logistics policies)*"
                )
        else:
            # Online LLM synthesis
            try:
                from langchain_openai import ChatOpenAI
                from langchain_core.messages import SystemMessage, HumanMessage

                if provider.lower() == "minimax" and settings.MINIMAX_API_KEY:
                    llm = ChatOpenAI(
                        model="MiniMax-M2.5",
                        openai_api_key=settings.MINIMAX_API_KEY,
                        openai_api_base="https://api.minimax.io/v1",
                        timeout=15.0
                    )
                else:
                    llm = ChatOpenAI(model="gpt-4o-mini", openai_api_key=settings.OPENAI_API_KEY, timeout=15.0)

                messages = [
                    SystemMessage(content=(
                        "You are the SmartLogix AI Assistant, a logistics copilot for Dimerco. "
                        "Synthesize an answer using ONLY the retrieved context below. "
                        "Be concise, highly professional, and cite rules accurately. "
                        f"Retrieved Context:\n{context_str}"
                    )),
                    HumanMessage(content=prompt)
                ]
                res = llm.invoke(messages)
                answer = res.content
            except Exception as e:
                answer = (
                    f"Failed to connect to online LLM ({e}). "
                    f"Falling back to local offline synthesis:\n\n"
                    f"{context_str}"
                )

        return answer, citations

    async def stream_query(self, prompt: str, provider: str = "Claude"):
        """
        Async generator that streams the RAG response as SSE chunks.
        Each yielded dict has keys:
          - {"type": "token", "token": "...", "provider": "..."}  — incremental text
          - {"type": "citations", "citations": [...]}              — source citations
          - {"type": "done"}                                       — terminal signal
        Falls back to a local simulated word-by-word stream when offline.
        """
        loop = asyncio.get_event_loop()

        # Run the synchronous vector search in a thread pool to avoid blocking the async event loop
        try:
            results_with_score = await loop.run_in_executor(
                self._executor,
                lambda: self.get_vectorstore().similarity_search_with_score(prompt, k=3)
            )
            if not results_with_score:
                self.ingest_documents()
                results_with_score = await loop.run_in_executor(
                    self._executor,
                    lambda: self.get_vectorstore().similarity_search_with_score(prompt, k=3)
                )
        except Exception as e:
            print(f"[RAG Stream] Vector search failed ({e}), running ingest...")
            self.ingest_documents()
            results_with_score = await loop.run_in_executor(
                self._executor,
                lambda: self.get_vectorstore().similarity_search_with_score(prompt, k=3)
            )

        citations = []
        context_chunks = []
        for doc, score in results_with_score:
            if not doc.page_content.strip():
                continue
            source_file = doc.metadata.get("source", "Unknown Document")
            citations.append({"source": source_file, "content_snippet": doc.page_content})
            context_chunks.append(doc.page_content)

        context_str = "\n\n".join(context_chunks)

        # Build answer text
        if settings.USE_OFFLINE_MODE:
            if not citations:
                answer = "I'm sorry, I couldn't find relevant logistics policies to answer your question."
            else:
                answer = (
                    f"Based on internal Dimerco operational guidelines, I retrieved the following information:\n\n"
                    f"{context_str}\n\n"
                    f"*(Response synthesized offline in compliance with local logistics policies)*"
                )
        else:
            try:
                from langchain_openai import ChatOpenAI
                from langchain_core.messages import SystemMessage, HumanMessage

                if provider.lower() == "minimax" and settings.MINIMAX_API_KEY:
                    llm = ChatOpenAI(
                        model="MiniMax-M2.5",
                        openai_api_key=settings.MINIMAX_API_KEY,
                        openai_api_base="https://api.minimax.io/v1",
                        timeout=15.0
                    )
                else:
                    llm = ChatOpenAI(model="gpt-4o-mini", openai_api_key=settings.OPENAI_API_KEY, timeout=15.0)

                messages = [
                    SystemMessage(content=(
                        "You are the SmartLogix AI Assistant, a logistics copilot for Dimerco. "
                        "Synthesize an answer using ONLY the retrieved context below. "
                        "Be concise, highly professional, and cite rules accurately. "
                        f"Retrieved Context:\n{context_str}"
                    )),
                    HumanMessage(content=prompt)
                ]
                res = await loop.run_in_executor(self._executor, lambda: llm.invoke(messages))
                answer = res.content
            except Exception as e:
                answer = (
                    f"Failed to connect to online LLM ({e}). "
                    f"Falling back to local offline synthesis:\n\n{context_str}"
                )

        # Stream citations first
        yield {"type": "citations", "citations": citations}

        # Simulate streaming word-by-word with a realistic delay
        words = answer.split(" ")
        for i, word in enumerate(words):
            token = word if i == 0 else " " + word
            yield {"type": "token", "token": token, "provider": provider}
            await asyncio.sleep(0.025)  # ~40 words/sec simulated typewriter

        yield {"type": "done"}


rag_service = RAGService()