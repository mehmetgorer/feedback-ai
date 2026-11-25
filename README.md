# Customer Feedback Management API

ASP.NET Core (.NET 10) Web API for creating, listing, updating, and deleting customer feedback. Data is stored in SQLite via Entity Framework Core.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Optional: [dotnet-ef](https://learn.microsoft.com/ef/core/cli/dotnet) global tool for manual migrations

```bash
dotnet tool install dotnet-ef --version 10.0.0
```

## Run the API

```bash
cd CustomerFeedback.Api
dotnet run
```

In Development, pending EF migrations are applied automatically on startup. The SQLite database file is `feedback.db` in the API project directory.

OpenAPI is available in Development at `/openapi/v1.json`.

## API endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/feedback?page=1&pageSize=20` | List feedback (newest first) |
| GET | `/api/feedback/{id}` | Get by id |
| POST | `/api/feedback` | Create feedback |
| PUT | `/api/feedback/{id}` | Update feedback |
| DELETE | `/api/feedback/{id}` | Delete feedback |

Request bodies are validated with FluentValidation. Invalid input returns `400 Bad Request`. Missing resources return `404 Not Found`. Unhandled errors return `500` with RFC 7807 problem details.

## Database migrations

```bash
dotnet ef database update --project CustomerFeedback.Api
dotnet ef migrations add <Name> --project CustomerFeedback.Api --output-dir Migrations
```

## Tests

```bash
dotnet test
```

Unit tests cover validators. Integration tests use `WebApplicationFactory` with an isolated SQLite database.

## Docker

The API image uses multi-stage builds on official `mcr.microsoft.com/dotnet` **.NET 10** images, listens on **8080**, and runs the process as the non-root **app** user (via `docker-entrypoint.sh`).

SQLite data is stored on a named volume at `/data/feedback.db`.

### Database migrations in containers

| Approach | When to use |
|----------|-------------|
| **Startup migrate (default in Compose)** | Set `Database__MigrateOnStartup=true`. On boot, EF Core runs pending migrations before serving requests (same as Development auto-migrate). Suitable for single-instance demos and local Docker. |
| **Manual / CI migrate** | Build migrations into the image and run `dotnet ef database update` from a SDK container or release pipeline before or during deploy; set `Database__MigrateOnStartup=false` for multi-instance production. |

### Build

```bash
docker build -t customer-feedback-api .
```

### Run (localhost-only port binding)

```bash
docker run --rm \
  -p 127.0.0.1:8080:8080 \
  -e Database__MigrateOnStartup=true \
  -e ConnectionStrings__DefaultConnection="Data Source=/data/feedback.db" \
  -v customer-feedback-data:/data \
  customer-feedback-api
```

### Compose

```bash
docker compose up --build
```

API: `http://127.0.0.1:8080`

## Solution structure

- `CustomerFeedback.Api` — HTTP API, EF Core, validation, error handling
- `CustomerFeedback.Tests` — xUnit unit and integration tests