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

## Project layout

- `backend/InterfataActe/InterfataActe` — ASP.NET Core API, EF Core models, document generation, and database migrations.
- `react` — React client and responsive workspace UI.
- `docker-compose.yml` — local application, API, and PostgreSQL services.

API JSON property names use lower camelCase to match React form state and table data. C# types and members follow standard .NET PascalCase conventions.
