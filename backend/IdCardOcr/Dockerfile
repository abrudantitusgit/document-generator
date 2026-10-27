FROM python:3.12-slim

ENV PYTHONDONTWRITEBYTECODE=1 PYTHONUNBUFFERED=1 PADDLE_PDX_DISABLE_MODEL_SOURCE_CHECK=True

# libgl1 is needed by the opencv-contrib-python build that PaddleX pins.
RUN apt-get update && apt-get install -y --no-install-recommends libgomp1 libglib2.0-0 libgl1 && rm -rf /var/lib/apt/lists/*
# PaddlePaddle GPU (CUDA 12.9) bundles its CUDA/cuDNN libraries; the host only needs the NVIDIA driver.
# The GPU build cannot load without that driver, so CPU-only hosts build with
# PADDLE_PACKAGE=paddlepaddle==3.4.0 and PADDLE_INDEX=.../stable/cpu/ (see docker-compose.cpu.yml).
ARG PADDLE_PACKAGE=paddlepaddle-gpu==3.4.0
ARG PADDLE_INDEX=https://www.paddlepaddle.org.cn/packages/stable/cu129/
RUN pip install --no-cache-dir "$PADDLE_PACKAGE" -i "$PADDLE_INDEX" --extra-index-url https://pypi.org/simple
WORKDIR /app
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
# Bake the models into the image so the service starts offline. Paddle cannot be
# imported without the GPU driver, so fetch them the way PaddleX would.
RUN python -c "from huggingface_hub import snapshot_download; [snapshot_download(f'PaddlePaddle/{name}', local_dir=f'/root/.paddlex/official_models/{name}') for name in ('PP-OCRv6_medium_det', 'PP-OCRv6_medium_rec', 'PP-LCNet_x1_0_doc_ori')]"

COPY app.py .
EXPOSE 5001
# One worker keeps a single model copy on the GPU; threads share it behind a lock.
CMD ["gunicorn", "--bind", "0.0.0.0:5001", "--workers", "1", "--threads", "4", "--timeout", "120", "app:app"]
