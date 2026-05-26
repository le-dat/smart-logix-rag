# --- Build Stage ---
FROM python:3.12-slim AS builder

WORKDIR /build

# Install system dependencies if compiling is required
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    && rm -rf /var/lib/apt/lists/*

# Copy and install python dependencies
COPY requirements.txt .
RUN pip install --no-cache-dir --user -r requirements.txt

# --- Runtime Stage ---
FROM python:3.12-slim AS runner

WORKDIR /app

# Copy built dependency wheels from builder
COPY --from=builder /root/.local /root/.local
COPY main.py .
COPY app/ app/

# Add python path dependencies to PATH variable
ENV PATH=/root/.local/bin:$PATH
ENV PYTHONUNBUFFERED=1

EXPOSE 8000

# Run FastAPI using python entrypoint
CMD ["python", "main.py"]
