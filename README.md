# OVINSTAL Document Workspace

Web application for managing customers, companies, projects, photovoltaic projects, registers, and generated documents.

## Local Docker deployment

Requirements: Docker Engine and the Docker Compose plugin.

From this directory, start the full stack:

```sh
docker compose up -d --build
```

Open the application at [http://localhost:3000](http://localhost:3000). The API is available at `http://localhost:5000`, and PostgreSQL is exposed on port `5433` for local database tools. On first launch, the backend creates the database schema from the current EF Core model. PostgreSQL and generated files are stored in named Docker volumes.

The first run creates a new local database. It does not import data from the prior server or network shares. To stop the app while preserving data, run `docker compose down`. To remove the database and generated files too, run `docker compose down -v`.

You can set `POSTGRES_USER`, `POSTGRES_PASSWORD`, and `POSTGRES_DB` in a `.env` file in this directory. The defaults are intended for local development.

## ID card OCR

The `id-ocr` service reads Romanian ID cards with PaddleOCR (PP-OCRv6) on an NVIDIA GPU. OpenCV finds the card in the photo, straightens it, and evens out the lighting first. Values from the MRZ (the `<<<` lines) are verified with their check digits and take priority for the series, number, and CNP. The web app calls the service through `/ocr` on port 3000, so it also works from other devices on the network.

Photos can be rotated, upside down, tilted, or taken at an angle: the service levels the text and reads the photo again when needed. Blurry photos get a warning asking the user to check the fields or retake the photo. To see the raw text the OCR read, add `?debug=1` to `/ocr/extract`.

Requirements for the GPU: an NVIDIA driver on the host and the [NVIDIA Container Toolkit](https://docs.nvidia.com/datacenter/cloud-native/container-toolkit/latest/install-guide.html). Check the active device with:

```sh
curl http://localhost:3000/ocr/health   # "device": "gpu:0"
```

On machines without an NVIDIA GPU, build the CPU variant instead. It is slower, about 15–20 s per photo instead of under 1 s:

```sh
docker compose -f docker-compose.yml -f docker-compose.cpu.yml up -d --build
```

The first build downloads PaddlePaddle with its CUDA libraries (about 10 GB). The OCR models are included in the image, so the service starts without internet access.

## Project layout

- `backend/InterfataActe/InterfataActe` — ASP.NET Core API, EF Core models, document generation, and database migrations.
- `react` — React client and responsive workspace UI.
- `docker-compose.yml` — local application, API, and PostgreSQL services.

API JSON property names use lower camelCase to match React form state and table data. C# types and members follow standard .NET PascalCase conventions.
