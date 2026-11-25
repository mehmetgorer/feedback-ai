# Customer Feedback Management API

ASP.NET Core (.NET 10) Web API for creating, listing, updating, and deleting customer feedback. Data is stored in SQLite via Entity Framework Core.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+ (for the React dashboard in `frontend/`)
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

## Frontend dashboard

The React + TypeScript + Vite app in `frontend/` provides a dashboard (totals, average rating, distribution), CRUD for feedback, rating filter, and pagination.

### Run both apps (recommended local setup)

1. Start the API (listens on `http://localhost:5047` in Development):

   ```bash
   dotnet run --project CustomerFeedback.Api
   ```

2. In another terminal, install dependencies once, then start the dev server:

   ```bash
   cd frontend
   npm install
   npm run dev
   ```

3. Open the URL shown by Vite (default `http://localhost:5173`).

The Vite dev server proxies `/api` to `http://127.0.0.1:5047`, so the browser talks only to the frontend origin and the API is not exposed via wide CORS. Override the proxy target with `VITE_DEV_API_PROXY` if your API runs elsewhere (see `frontend/.env.example`).

### Production build

```bash
cd frontend
npm run build
npm run preview
```

`vite preview` uses the same `/api` proxy as dev; keep the API running on port **5047** (or adjust `VITE_DEV_API_PROXY`).

## API endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/api/feedback?page=1&pageSize=20` | List feedback (newest first) |
| GET | `/api/feedback/{id}` | Get by id |
| POST | `/api/feedback` | Create feedback |
| PUT | `/api/feedback/{id}` | Update feedback |
| DELETE | `/api/feedback/{id}` | Delete feedback |
| POST | `/api/feedback/{id}/analyze` | AI analysis (sentiment, category, summary) |

Request bodies are validated with FluentValidation. Invalid input returns `400 Bad Request`. Missing resources return `404 Not Found`. Unhandled errors return `500` with RFC 7807 problem details.

## AI feedback analysis (Groq)

The API can analyze a feedback **comment only** (no customer name or email is sent to Groq). Results are stored on the feedback record and returned in the `analysis` field on list/get responses.

Configure a Groq API key on the **server** — never in the frontend or in source control:

| Method | Example |
|--------|---------|
| Environment variable | `Groq__ApiKey=your-key` |
| User secrets (Development) | `dotnet user-secrets set "Groq:ApiKey" "your-key" --project CustomerFeedback.Api` |

Optional settings in `appsettings.json` under `Groq` (or via `Groq__Model`, `Groq__TimeoutSeconds`):

- `Model` — default `openai/gpt-oss-20b` (supports Groq [JSON Object Mode](https://console.groq.com/docs/structured-outputs); the API uses `response_format: { "type": "json_object" }` with a JSON-only system prompt)
- `TimeoutSeconds` — default `30`
- `BaseUrl` — default `https://api.groq.com/openai/v1`

Call `POST /api/feedback/{id}/analyze` or use **Analyze** in the dashboard. If the key is missing, analysis returns `503`. Provider or parsing errors return `502`; timeouts return `504`. CRUD works without configuring Groq.

Editing a feedback comment clears any stored analysis (re-run **Analyze** after edits).

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
- `frontend/` — React dashboard (Vite) for feedback management