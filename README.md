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

## Solution structure

- `CustomerFeedback.Api` — HTTP API, EF Core, validation, error handling
- `CustomerFeedback.Tests` — xUnit unit and integration tests