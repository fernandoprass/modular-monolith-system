# Docker & Containerization Guide

This guide explains how Docker is used in this project.

It covers both infrastructure containers and application containers.

The goal is to let a developer run the system locally with the same basic shape used in production.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **Image** | A read-only blueprint for a container. It includes the OS, runtime, libraries, and code. |
| **Container** | A running instance of an image. It is isolated from your host machine and other containers. |
| **Volume** | Persistent storage outside the container lifecycle. Used for databases and Redis data. |
| **Network** | A private Docker communication space where containers can find each other by service name. |
| **Docker Engine** | The background service that builds images and runs containers. |
| **Docker Compose** | A tool for defining and running multiple containers from YAML files. |
| **Health Check** | A command Docker runs to know if a container is ready and healthy. |

Simple mental model:

```text
Image -> Container -> Runs the service
Volume -> Keeps data
Network -> Lets containers talk
Compose -> Starts everything together
```

---

## 2. What We Run With Docker

Infrastructure:

| Service | Purpose |
| :--- | :--- |
| PostgreSQL | Main relational database. |
| pgAdmin | Web UI to inspect PostgreSQL. |
| MongoDB | Sentinel document/log database. |
| Mongo Express | Web UI to inspect MongoDB. |
| Redis | Messaging, distributed cache, and stream processing. |

Applications:

| Service | Purpose |
| :--- | :--- |
| Core API | Modular monolith API host for IAM, Sentinel, and Courier. |
| IAM API | Identity and access management API. |
| Sentinel API | Logging and monitoring API plus background consumers. |
| Courier API | Communication and notification API plus background workers. |

You can run only the infrastructure and start APIs from Visual Studio.

You can also run the infrastructure and APIs all inside Docker.

---

## 3. Files

Docker files live mainly in `infra`.

| File | Purpose |
| :--- | :--- |
| `infra/docker-compose.postgresql.yaml` | PostgreSQL and pgAdmin. |
| `infra/docker-compose.mongodb.yaml` | MongoDB and Mongo Express. |
| `infra/docker-compose.redis.yaml` | Redis. |
| `infra/docker-compose.core.yaml` | Core API modular monolith host. |
| `infra/docker-compose.modules.yaml` | Standalone IAM API, Sentinel API, and Courier API. |
| `infra/.env` | Local environment values used by Compose. |
| `infra/start-infra.ps1` | Starts infrastructure containers. |
| `infra/start-modules.ps1` | Builds and starts API containers. |

Application Dockerfiles:

| File | Purpose |
| :--- | :--- |
| `back/src/00.Core/Core.API/Dockerfile` | Builds Core API image. |
| `back/src/03.IAM/IAM.API/Dockerfile` | Builds IAM API image. |
| `back/src/02.Sentinel/Sentinel.API/Dockerfile` | Builds Sentinel API image. |
| `back/src/04.Courier/Courier.API/Dockerfile` | Builds Courier API image. |

---

## 4. The `.env` File

`infra/.env` stores local configuration for Compose.

Examples:
- Database user.
- Database password.
- Database name.
- Redis connection.
- MongoDB connection.
- API host ports.

Compose reads values using this syntax:

```yaml
environment:
  POSTGRES_USER: ${POSTGRES_USER}
```

This means:

```text
Read POSTGRES_USER from infra/.env
Pass it to the container as POSTGRES_USER
```

Do not commit real secrets.

For local development, `.env` is acceptable.

For production, use a secret manager or deployment environment variables.

---

## 5. Docker Compose

Compose starts multiple containers as one local system.

This project uses separate compose files.

Reason:
- You can start only what you need.
- Infrastructure can run without rebuilding APIs.
- APIs can be rebuilt separately.

Typical flow:

```powershell
.\infra\start-infra.ps1
.\infra\start-modules.ps1
```

`start-infra.ps1` starts databases and Redis.

`start-modules.ps1` builds and starts the standalone module APIs.

`docker-compose.core.yaml` starts Core API.

Use Core API for modular monolith mode.

Use `docker-compose.modules.yaml` for standalone module API mode.

---

## 6. Multi-Stage Dockerfiles

Core, IAM, Sentinel, and Courier APIs use multi-stage Dockerfiles.

The normal flow:

1. Use the .NET SDK image to restore, build, and publish.
2. Copy the published output into an ASP.NET runtime image.
3. Run the app from the smaller runtime image.

Why this is good:
- Final image is smaller.
- SDK is not shipped in the runtime image.
- Build tools stay out of production-like containers.

Conceptual example:

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk AS build
RUN dotnet publish ...

FROM mcr.microsoft.com/dotnet/aspnet AS runtime
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "IAM.API.dll"]
```

Do not copy this blindly.

Use the real Dockerfiles in the API projects.

---

## 7. Networks

A Docker network lets containers talk to each other.

Containers inside the same network can use the service name as the host.

Example:

```text
IAM API container -> PostgreSQL container
Host = postgres_db
```

This is different from running on your host machine.

From your host machine:

```text
Host=localhost
```

From inside a container:

```text
Host=postgres_db
```

Important:

`localhost` inside a container means the container itself.

It does not mean your Windows machine.

That is why Docker connection strings use service names.

---

## 8. Ports

Port mapping connects your host machine to a container.

Example:

```yaml
ports:
  - "5055:8080"
```

Meaning:

| Side | Meaning |
| :--- | :--- |
| `5055` | Port on your host machine. Browser uses this. |
| `8080` | Port inside the container. ASP.NET listens here. |

Current API ports:

| App | Docker URL | Visual Studio URL |
| :--- | :--- | :--- |
| Core API | `http://localhost:5050` | `https://localhost:4050` |
| IAM API | `http://localhost:5055` | `https://localhost:4055` |
| Sentinel API | `http://localhost:5056` | `https://localhost:4056` |
| Courier API | `http://localhost:5057` | `https://localhost:4057` |

Docker and Visual Studio ports are intentionally different.

This avoids local port conflicts.

Do not change Docker ports to fix Visual Studio conflicts.

Change local launch settings instead.

---

## 9. Volumes

Containers can be deleted and recreated.

Without volumes, database data would be lost when the container is removed.

Volumes store data outside the container lifecycle.

Use volumes for:
- PostgreSQL data.
- MongoDB data.
- Redis data when persistence is enabled.

Example idea:

```text
postgres container removed
postgres volume still exists
new postgres container uses same data
```

---

## 10. Health Checks

Health checks tell Docker if a service is ready.

Examples:
- PostgreSQL can use `pg_isready`.
- Redis can use `redis-cli ping`.
- APIs can use health endpoints.

Why this matters:
- The API should not start before PostgreSQL is ready.
- Sentinel consumers should not start before Redis is ready.
- Compose can wait for healthy dependencies.

Health checks are not user-facing tests.

They are readiness checks for containers.

---

## 11. Running Infrastructure Only

Use this when you want to run APIs from Visual Studio.

From repository root:

```powershell
.\infra\start-infra.ps1
```

This starts:
- PostgreSQL.
- MongoDB.
- Redis.
- pgAdmin.
- Mongo Express.

Then start Core, IAM, Sentinel, or Courier from Visual Studio.

---

## 12. Running Applications in Docker

First start infrastructure:

```powershell
.\infra\start-infra.ps1
```

Then start applications:

```powershell
.\infra\start-modules.ps1
```

This builds and starts standalone module APIs:
- `iam_api`
- `sentinel_api`
- `courier_api`

First build can take a few minutes.

After build, open:

```text
http://localhost:5055
http://localhost:5056
http://localhost:5057
```

Use the real controller routes for API calls.

To run Core API in Docker, use:

```powershell
docker compose --env-file infra/.env -f infra/docker-compose.core.yaml up -d --build
```

This builds and starts:
- `core_api`

Open:

```text
http://localhost:5050
```

---

## 13. Connecting pgAdmin

If pgAdmin does not auto-register the PostgreSQL server, add it manually.

Steps:

1. Open pgAdmin.
2. Click **Add New Server**.
3. In **General**, set name to `Local-Modular-System`.
4. In **Connection**, set host to the PostgreSQL service name.

Current host:

```text
postgres_db
```

Use:
- Port from the compose file.
- Database from `infra/.env`.
- Username from `infra/.env`.
- Password from `infra/.env`.

Remember:

pgAdmin runs inside Docker.

So it must use `postgres_db`, not `localhost`.

---

## 14. Common Commands

Start containers:

```powershell
docker compose up -d
```

Stop and remove containers:

```powershell
docker compose down
```

View logs:

```powershell
docker compose logs -f
```

Validate final compose config:

```powershell
docker compose config
```

List running containers:

```powershell
docker ps
```

---

## 15. Troubleshooting

### Port Already In Use

Cause:
- Another process uses the host port.

Fix:
- Check the process using the port.
- Keep Docker and Visual Studio ports different.
- Change local Visual Studio port if needed.

### API Cannot Connect to Database

Check where the API is running.

If API runs on host:

```text
Host=localhost
```

If API runs in Docker:

```text
Host=postgres_db
```

### API Container Starts But Route Returns Not Found

Check:
- Correct host port.
- Correct controller route.
- API logs.
- Health endpoint path.

### Container Keeps Restarting

Check logs:

```powershell
docker logs <container-name>
```

Common causes:
- Wrong connection string.
- Missing environment variable.
- Dependency container not healthy.

---

## 16. Rules For This Project

- Keep Docker ports and Visual Studio ports different.
- Use service names inside Docker networks.
- Use `localhost` only from the host machine.
- Store database data in volumes.
- Keep secrets out of committed files.
- Prefer scripts in `infra` for day-to-day startup.
