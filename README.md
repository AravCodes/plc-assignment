# PLC Assignment (Monorepo)

Two small apps live here:
- Assignment 1: Task Manager – .NET 8 Minimal API (in-memory) + React
- Assignment 2: Project Manager – .NET 8 Minimal API (EF Core SQLite + JWT) + React

This README is optimized for Windows PowerShell. Adjust commands as needed for your shell.

## Repo layout

plc-assignment/
- project-manager/
  - backend/ProjectManagerAPI (API, SQLite)
  - project-manager-frontend (React + TS)
- task-manager/
  - backend/TaskManagerAPI (API, in-memory)
  - task-manager-frontend (React + TS)

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- Git (for cloning/pushing)

Ports used: Project Manager API 5067, Task Manager API 5091, CRA frontends 3000/3001.

## Quick start

1) Project Manager API (Assignment 2)
- Set a dev JWT secret (don’t use in production):
  - PowerShell: `$Env:Jwt__Secret = 'dev-secret-change-me'`
- Start API from repo root:
  - `dotnet run --project .\project-manager\backend\ProjectManagerAPI\ProjectManagerAPI.csproj`
- Swagger (Development only): http://localhost:5067/swagger
- Health: http://localhost:5067/health

2) Project Manager Frontend
- Copy `.env.example` to `.env` in `project-manager/project-manager-frontend` and ensure `REACT_APP_API_URL=http://localhost:5067`.
- Start:
  - `npm --prefix .\project-manager\project-manager-frontend start`
- App opens at http://localhost:3000 (or 3001 if 3000 is busy).

3) Task Manager API (Assignment 1) – optional showcase
- Start:
  - `dotnet run --project .\task-manager\backend\TaskManagerAPI\TaskManagerAPI.csproj`
- Health: (if implemented) check API base.

4) Task Manager Frontend – optional
- Start:
  - `npm --prefix .\task-manager\task-manager-frontend start`

## Configuration

- CORS: `AllowedOrigins` in appsettings.
- JWT: Provide `Jwt:Secret` via environment variable or user-secrets. Do not commit secrets.
- Frontend: `REACT_APP_API_URL` points to API base URL.
- Database: EF Core uses SQLite (file is created on first run). DB files are ignored by Git.

## Seeding / smoke tests

- Project Manager seed/smoke: `project-manager/seed-project-manager.ps1`
  - Example (PowerShell):
    - `$base = 'http://localhost:5067'`
    - `powershell -File .\project-manager\seed-project-manager.ps1 -BaseUrl $base`
- Task Manager seed and view: `task-manager/seed-and-view.ps1`

## Troubleshooting

- Port busy errors: stop any apps using 5067/5091/3000/3001 and retry.
- SQLite schema errors like `no such column: DueDate`: the API includes a dev-time patch; restart the API once to apply.

## Push to GitHub

Using GitHub CLI (recommended):
1. Initialize and commit
   - `git init`
   - `git add -A`
   - `git commit -m "Initial commit: PLC Assignment monorepo"`
   - `git branch -M main`
2. Create and push repo (authenticated with `gh auth login`)
   - `gh repo create plc-assignment --public --source . --remote origin --push`

Manual (via website):
1. Create an empty repo named `plc-assignment` on GitHub.
2. Add remote and push:
   - `git remote add origin https://github.com/<your-username>/plc-assignment.git`
   - `git push -u origin main`

## Notes

- Keep secrets out of source control; provide via env vars or secret stores.
- For production, prefer EF Core migrations rather than dev-time schema patches.