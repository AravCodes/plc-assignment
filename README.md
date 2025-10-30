# PLC Assignment

Two apps are included, aligning to the assignment briefs:
- Home Assignment 1: Basic Task Manager — .NET 8 Minimal API (in-memory) + React (TypeScript)
- Home Assignment 2: Mini Project Manager — .NET 8 Minimal API (EF Core SQLite + JWT) + React (TypeScript)

## Working Demo
<img width="1919" height="987" alt="Screenshot 2025-10-31 021810" src="https://github.com/user-attachments/assets/f9e62ad1-89e7-4281-b600-120f23952e1a" />

<img width="1919" height="990" alt="Screenshot 2025-10-31 021828" src="https://github.com/user-attachments/assets/9fbd5163-791c-4acd-bdfc-8b5f4ff7efb4" />

<img width="1919" height="984" alt="Screenshot 2025-10-31 021854" src="https://github.com/user-attachments/assets/769e410a-8afd-47f2-b4d2-e0e1547b5d99" />

<img width="1918" height="1006" alt="Screenshot 2025-10-31 021912" src="https://github.com/user-attachments/assets/ae6768be-8517-4a58-af7c-0711bf703390" />

<img width="1890" height="969" alt="Screenshot 2025-10-31 030403" src="https://github.com/user-attachments/assets/c4033913-11c4-483a-851d-87e0a1df3ca1" />

<img width="1894" height="1084" alt="Screenshot 2025-10-31 030439" src="https://github.com/user-attachments/assets/d65ff0f4-524a-476a-872f-e5fd7282cc1f" />







## Repo layout

plc-assignment/
- project-manager/
  - backend/ProjectManagerAPI (API, SQLite, JWT)
  - project-manager-frontend (React + TS)
- task-manager/
  - backend/TaskManagerAPI (API, in-memory)
  - task-manager-frontend (React + TS)

## Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- Git

Ports used: PM API 5067, TM API 5091, CRA frontends 3000/3001.

---

## Home Assignment 1 – Basic Task Manager

Objective: Build a simple full-stack app with C# (.NET 8) and React + TypeScript.

Implemented features
- Display a list of tasks
- Add a new task (description)
- Toggle task completion
- Delete a task

Backend (.NET 8, in-memory)
- In-memory repository (no database)
- Endpoints (scoped by projectId to support multi-project demo):
  - GET /api/projects/{projectId}/tasks
  - POST /api/projects/{projectId}/tasks
  - PUT /api/projects/{projectId}/tasks/{id}
  - DELETE /api/projects/{projectId}/tasks/{id}

Frontend (React + TypeScript)
- Single-page app using React Hooks and Axios
- Components (see `task-manager/task-manager-frontend/src/components/`):
  - TaskInput.tsx — add new task
  - TaskList.tsx — render list
  - TaskItem.tsx — toggle/delete

Quality-of-life enhancements
- Optional filtering and basic styling (Bootstrap-ready)
- LocalStorage can be added to persist tasks client-side

How to run (Assignment 1)
```powershell
# Backend
dotnet run --project .\task-manager\backend\TaskManagerAPI\TaskManagerAPI.csproj

# Frontend
npm --prefix .\task-manager\task-manager-frontend start
```

---

## Home Assignment 2 – Mini Project Manager

Objective: A fuller web app with authentication, entity relationships, routing, and modular code.

Core features
- Authentication: Register, Login (JWT); users only see their data
- Projects: title (3–100), description (<= 500), createdAt auto
- Tasks per project: title (required), due date (optional), completion status

Backend (.NET 8 + EF Core SQLite + JWT)
- DataAnnotations for DTO validation
- Minimal APIs with clear separation (DTOs, models, endpoints)
- Endpoints
  - Auth
    - POST /api/auth/register
    - POST /api/auth/login
    - GET /api/auth/me (requires auth)
  - Projects
    - GET /api/projects
    - POST /api/projects
    - GET /api/projects/{id}
    - DELETE /api/projects/{id}
  - Tasks (per-project)
    - GET /api/projects/{projectId}/tasks
    - POST /api/projects/{projectId}/tasks
    - PUT /api/projects/{projectId}/tasks/{taskId}
    - DELETE /api/projects/{projectId}/tasks/{taskId}

Frontend (React + TypeScript)
- Pages (see `project-manager/project-manager-frontend/src/pages/`):
  - Login.tsx / Register.tsx
  - Dashboard.tsx (list projects)
  - ProjectPage.tsx (project detail + task list; add/update/delete/toggle)
  - ScheduleDemo.tsx (example consumer for scheduler)
- AuthContext.tsx stores JWT and attaches it via Axios interceptor
- React Router for navigation; form validation and error states included

Smart Scheduler API (Required Enhancement, Credits 10)
- Endpoint: POST `/api/v1/projects/{projectId}/schedule` (requires auth)
- Input example
```json
{
  "tasks": [
    { "title": "Design", "duration": 2 },
    { "title": "Build", "duration": 3 }
  ],
  "startDate": "2025-11-01"
}
```
- Output example
```json
{
  "schedule": [
    { "title": "Design", "start": "2025-11-01", "end": "2025-11-02" },
    { "title": "Build", "start": "2025-11-03", "end": "2025-11-05" }
  ]
}
```

How to run (Assignment 2)
```powershell
# API (set a dev secret; do not use in production)
$Env:Jwt__Secret = 'dev-secret-change-me'
dotnet run --project .\project-manager\backend\ProjectManagerAPI\ProjectManagerAPI.csproj

# Frontend (ensure REACT_APP_API_URL points to http://localhost:5067)
npm --prefix .\project-manager\project-manager-frontend start
```

Useful URLs
- Swagger (Development): http://localhost:5067/swagger
- Health: http://localhost:5067/health

---

## Configuration

- CORS: `AllowedOrigins` in API appsettings
- JWT secret: set `Jwt:Secret` via environment variable or user-secrets
- Frontend base URL: `REACT_APP_API_URL`
- Database: EF Core SQLite (local file created automatically; ignored by Git)

## Seeding / smoke tests

- Project Manager: `project-manager/seed-project-manager.ps1`
- Task Manager: `task-manager/seed-and-view.ps1`

## Troubleshooting

- If ports 5067/5091/3000/3001 are busy, stop processes and retry



## Deployment (bonus)

- Backends: Render/Fly.io/Azure (enable HTTPS, set `AllowedOrigins` and `Jwt:Secret`)
- Frontends: Vercel/Netlify (set `REACT_APP_API_URL` to your backend URL)

