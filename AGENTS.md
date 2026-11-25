# Repository Guidelines

## Project Structure

- `CustomerFeedback.slnx` contains the ASP.NET Core API and its test project. Both target .NET 10.
- `CustomerFeedback.Api/` contains the feedback endpoints in `Controllers/`, application logic in `Services/`, request and response types in `Dtos/`, and FluentValidation rules in `Validation/`. EF Core uses SQLite, with the context in `Data/` and schema history in `Migrations/`.
- `CustomerFeedback.Tests/` contains validator unit tests in `Validation/`, HTTP integration tests in `Integration/`, and the test host in `Infrastructure/`. Use `CustomerFeedback.Api/CustomerFeedback.Api.http` for manual requests.

## Build, Test, and Run

Run these commands from the repository root with the .NET 10 SDK installed.

- `dotnet build` restores dependencies and builds the solution.
- `dotnet run --project CustomerFeedback.Api` starts the API. In Development, startup applies pending migrations and serves OpenAPI at `/openapi/v1.json`.
- `dotnet test` runs all tests. Add `--filter FullyQualifiedName~FeedbackApiTests` to run the HTTP integration tests.
- With the `dotnet-ef` tool installed, run `dotnet ef migrations add <Name> --project CustomerFeedback.Api --output-dir Migrations` after a schema change. Keep local SQLite database files out of commits.

## Coding Style

Use four-space indentation, braces on separate lines, and file-scoped namespaces. Use PascalCase for types and methods, camelCase for parameters and locals, and the `Async` suffix for asynchronous service methods. Respect nullable reference annotations and place new types alongside related code.

## Testing Guidelines

Use xUnit and follow the existing `Operation_Condition_Outcome` naming pattern, for example `GetById_UnknownId_ReturnsNotFound`. Add validator tests for rule changes and integration tests for endpoint behavior. Reuse `FeedbackApiFactory` to run against isolated in-memory SQLite. The repository specifies no coverage minimum.

## Commits and Pull Requests

There is no commit history to establish a convention. Use short, imperative commit subjects such as `Validate feedback ratings`. In pull requests, summarize behavior changes, link any related issue, and report test results. Include sample requests and responses when changing the API contract.
