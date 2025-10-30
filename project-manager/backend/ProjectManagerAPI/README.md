# ProjectManagerAPI

- .NET 8 Minimal API with JWT and SQLite.
- Set `Jwt:Secret` via environment variable or user-secrets. Do not commit secrets.
- CORS origins from `AllowedOrigins` in appsettings.
- Database created on first run; a dev-time patch ensures `Tasks.DueDate` exists.

## Environment

- PowerShell example:
  - `$Env:Jwt__Secret = 'replace-with-strong-secret'`

## Run

- Launch profile `http` listens on http://localhost:5067.
- Swagger UI available in Development at `/swagger`.