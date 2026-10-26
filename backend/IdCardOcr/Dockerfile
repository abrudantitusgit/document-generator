FROM python:3.12-slim

WORKDIR /app
ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1
ENV EASYOCR_MODULE_PATH=/root/.EasyOCR

RUN apt-get update && apt-get install -y --no-install-recommends tesseract-ocr tesseract-ocr-ron tesseract-ocr-eng libglib2.0-0 libjpeg62-turbo && rm -rf /var/lib/apt/lists/*
COPY requirements.txt .
RUN pip install --no-cache-dir torch torchvision --index-url https://download.pytorch.org/whl/cpu
RUN pip install --no-cache-dir -r requirements.txt
COPY app.py .
EXPOSE 5001
CMD ["python", "app.py"]
