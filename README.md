# Core Modular System

Core Modular System is a modular SaaS platform.

It has:
- a backend in `back/`
- a frontend admin app in `front/`
- shared docs in `docs/`
- local infrastructure in `infra/`

The backend is built with .NET, Clean Architecture, PostgreSQL, MongoDB, and Redis.

The frontend is built with React, TypeScript, Vite, and shadcn/ui style components.

---

## Folder Structure

```text
back/
  src/
  tests/
  CoreModularSystem.slnx

front/
  apps/
  agents.md
  backend.md

docs/
infra/
```

---

## Backend

Backend source lives in:

```text
back/src
```

Main modules:

| Module | Purpose |
| :--- | :--- |
| `00.Core` | Modular monolith API host. |
| `01.Shared` | Shared entities, contracts, infrastructure, cache, and messaging. |
| `02.Sentinel` | Audit logs and system logs. |
| `03.IAM` | Identity, access, tenants, roles, permissions, and authentication. |
| `04.Courier` | Email and notification workflows. |
| `99.Seeder` | Default database data. |

Backend tests live in:

```text
back/tests
```

Test folders:
- `back/tests/unit`
- `back/tests/e2e`
- `back/tests/bruno`

Build example:

```powershell
dotnet build back/src/03.IAM/IAM.API/IAM.API.csproj --no-restore
```

Test example:

```powershell
dotnet test back/tests/unit/03.IAM/IAM.Application.Tests/IAM.Application.Tests.csproj
```

---

## Frontend

Frontend source lives in:

```text
front/apps
```

It is the admin UI for the Core API backend.

Install dependencies:

```powershell
cd front/apps
npm install
```

Build:

```powershell
npm run build
```

Run locally:

```powershell
npm run dev
```

---

## Documentation

Start here:

```text
docs/readme.md
```

Useful docs:
- `docs/folder-structure.md`
- `docs/deployment-modes.md`
- `docs/docker.md`
- `docs/migrations.md`
- `docs/00.core.e2e-tests.md`

---

## Infrastructure

Infrastructure files live in:

```text
infra/
```

Use it for local Docker, databases, Redis, and environment setup.

---

## Agent Rules

Root agent guide:

```text
agents.md
```

Backend rules:

```text
back/agents.md
```

Frontend rules:

```text
front/agents.md
```

If a task touches both backend and frontend, read both rule files.
