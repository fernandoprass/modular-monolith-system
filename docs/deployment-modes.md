# Deployment Modes

The system supports two API deployment modes.

Both modes use the same module code.

The difference is the host process.

---

## 1. Modular Monolith Mode

Modular monolith mode runs all module APIs from Core API.

Host project:

```text
src/00.Core/Core.API
```

Core API loads module controllers with Application Parts.

It starts:
- IAM endpoints
- Sentinel endpoints
- Courier endpoints

All endpoints use one HTTP port.

Current URLs:

| Environment | URL |
| :--- | :--- |
| Local HTTPS | `https://localhost:4050` |
| Docker | `http://localhost:5050` |

Use this mode when you want one application process.

### Core Docker Publish Notes

Core API references the module API projects.

Those module API projects have their own `appsettings*.json` files.

When Core API is published, only Core API configuration files should be copied to the publish folder.

Module `appsettings*.json` files are excluded during Core publish.

This avoids duplicate publish output files.

Standalone module publish still keeps each module's own configuration files.

Core Docker publish also runs MSBuild with one process:

```text
/p:BuildInParallel=false
/m:1
```

This avoids Linux container file copy races during publish.

---

## 2. Standalone Module API Mode

Standalone mode runs each module API separately.

Host projects:

```text
src/03.IAM/IAM.API
src/02.Sentinel/Sentinel.API
src/04.Courier/Courier.API
```

Each API keeps its own:
- Controllers
- Startup
- Configuration
- Dockerfile
- Port

Current URLs:

| API | Docker URL | Local HTTPS URL |
| :--- | :--- | :--- |
| IAM API | `http://localhost:5055` | `https://localhost:4055` |
| Sentinel API | `http://localhost:5056` | `https://localhost:4056` |
| Courier API | `http://localhost:5057` | `https://localhost:4057` |

Use this mode when you want module-by-module deployment.

---

## 3. Shared Registration

Both modes use the same module registration methods.

Examples:

```csharp
builder.Services.AddIamModule(builder.Configuration);
builder.Services.AddSentinelModule(builder.Configuration);
builder.Services.AddCourierModule(builder.Configuration);
```

Core API calls all module methods.

Standalone APIs call only their own module method.

This keeps dependency registration consistent.

---

## 4. Controller Ownership

Controllers belong to module API projects.

Core API must not copy them.

Example:

```text
IAM.API/Controllers/UserController.cs
```

Core API loads that controller assembly.

It does not create another `UserController`.

---

## 5. Choosing A Mode

Use Core API when:
- You want one running API.
- You want one Swagger page.
- You want one port for all modules.
- You want modular monolith deployment.

Use standalone APIs when:
- You want separate processes.
- You want separate ports.
- You want to deploy one module without the others.
- You want to test a module host in isolation.

Both modes are valid.

Do not remove standalone APIs.
