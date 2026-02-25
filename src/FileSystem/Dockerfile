FROM python:3.10-slim

WORKDIR /app

COPY requirements.txt .
# Upgrade pip to ensure cache break and latest features
RUN pip install --upgrade pip
RUN pip install --no-cache-dir -r requirements.txt

COPY server.py .

CMD ["python", "server.py"]