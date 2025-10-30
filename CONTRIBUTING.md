# Contributing Guide

Thanks for your interest in contributing! This monorepo contains two small apps:

- Assignment 1: Task Manager — .NET 8 Minimal API (in-memory) + React (TypeScript)
- Assignment 2: Project Manager — .NET 8 Minimal API (EF Core SQLite + JWT) + React (TypeScript)

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- Git

## Repository layout

- project-manager/
  - backend/ProjectManagerAPI — API (SQLite, JWT)
  - project-manager-frontend — React + TypeScript
- task-manager/
  - backend/TaskManagerAPI — API (in-memory)
  - task-manager-frontend — React + TypeScript

## Getting started (Windows PowerShell)

1) Project Manager API
- Set a dev JWT secret (don’t use in production):
  - `$Env:Jwt__Secret = 'dev-secret-change-me'`
- Start API:
  - `dotnet run --project .\project-manager\backend\ProjectManagerAPI\ProjectManagerAPI.csproj`
- Swagger (Development): http://localhost:5067/swagger

2) Project Manager Frontend
- Ensure `REACT_APP_API_URL=http://localhost:5067`
- Start: `npm --prefix .\project-manager\project-manager-frontend start`

3) Task Manager API
- Start: `dotnet run --project .\task-manager\backend\TaskManagerAPI\TaskManagerAPI.csproj`

4) Task Manager Frontend
- Start: `npm --prefix .\task-manager\task-manager-frontend start`

## Branches and commits

- Create feature branches from `main` (e.g., `feat/pm-add-scheduler` or `fix/tm-toggle-completion`).
- Commit messages: short imperative summary, optional body and references.
  - Example: `feat(pm): add scheduler endpoint and page`

## Code style

- C#/.NET:
  - Target .NET 8
  - Prefer minimal APIs and DTO validation via DataAnnotations
  - Keep endpoints organized under `Endpoints/`
  - Run `dotnet build` locally before PR
- TypeScript/React:
  - Functional components with hooks
  - Keep API calls in `src/api.ts` (Axios instance adds JWT automatically)
  - Use React Router for navigation (PM)

## Testing and smoke checks

- PM API: run `project-manager/seed-project-manager.ps1` to register/login and create a project as a smoke test.
- TM API: run `task-manager/seed-and-view.ps1` to seed and view tasks.

## Pull requests

- Include a clear description of the change and rationale.
- Link to any related issues or assignment requirements.
- Confirm the following checklist:
  - [ ] Builds succeed: backends (`dotnet build`) and frontends (`npm run build`)
  - [ ] PM API `/health` returns 200 locally
  - [ ] No secrets committed (check `.env`, `app.db` not tracked)

## Environment and secrets

- Do not commit secrets.
- Provide `Jwt:Secret` via environment variables or a secret store.
- Frontend uses `REACT_APP_API_URL` to point to the API base URL.

## CI

- A GitHub Actions workflow builds backends and frontends and runs a basic `/health` smoke test for the PM API.
- See `.github/workflows/ci.yml`.
