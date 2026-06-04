# IAM Module

IAM means Identity and Access Management.

This module owns:
- Organizations.
- Users.
- Roles.
- Permissions.
- Authentication.
- Authorization data.
- IAM audit logging.

The module lives in:

```text
src/03.IAM
```

---

## 1. Purpose

IAM controls who can access the system and what they can do.

It answers questions like:
- Who is this user?
- Is the user active?
- Which organization owns this user?
- Which roles does the user have?
- Which permissions does each role have?
- Can this user execute this endpoint?

IAM is a business module.

It should own IAM domain rules.

It should not own cross-module infrastructure like Redis publisher implementation or Sentinel log persistence.

---

## 2. Project Structure

```text
src/03.IAM/
+-- IAM.API/
+-- IAM.Application/
+-- IAM.Domain/
+-- IAM.Infrastructure/
```

| Project | Responsibility |
| :--- | :--- |
| `IAM.API` | Controllers, API startup, JWT setup, middleware, dependency injection. |
| `IAM.Application` | Services, orchestrators, validators, audit logger helper, application contracts. |
| `IAM.Domain` | Entities, DTOs, constants, permissions, repository interfaces, query repository interfaces. |
| `IAM.Infrastructure` | EF Core DbContext, repositories, query repositories, unit of work, migrations. |

Dependency direction:

```text
IAM.API -> IAM.Application -> IAM.Domain
IAM.Infrastructure -> IAM.Domain
IAM.API -> IAM.Infrastructure
```

Domain should not depend on Infrastructure or API.

---

## 3. Main Entities

IAM owns these main entities:

| Entity | Purpose |
| :--- | :--- |
| `Organization` | Owner of users and organization-specific roles. |
| `User` | Person/account that can authenticate and use the system. |
| `Role` | Group of permissions. Can be global or organization-specific. |
| `Permission` | Allowed action represented by a code. |
| `UserRole` | Link between user and role. |
| `RolePermission` | Link between role and permission. |

Relationship summary:

```text
Organization 1 -> many Users
User many -> many Roles through UserRole
Role many -> many Permissions through RolePermission
```

Entities live in:

```text
src/03.IAM/IAM.Domain/Entities
```

See also:
- `docs/03.iam.entities.md`

---

## 4. Authentication

Authentication is handled by `AuthService`.

Location:

```text
src/03.IAM/IAM.Application/Services/AuthService.cs
```

Login flow:

```text
Login request
  -> find user by email with password data
  -> validate lockout
  -> verify password with Argon2
  -> check user and organization active
  -> update last successful login
  -> hydrate role permission cache
  -> generate JWT
  -> publish audit log
```

Password hashing:
- Uses Argon2.
- Passwords are never stored as plain text.

Failed login:
- Updates failed login count.
- Can lock the account using security parameters.
- Publishes failed login audit log.

Successful login:
- Resets failed login count.
- Updates `LastLoginAt`.
- Publishes successful login audit log.

JWT includes:
- User ID.
- Email.
- Name.
- System admin flag.
- User owner ID.
- Role IDs.

Permissions are not stored in the JWT.

Only role IDs are stored.

Permissions are loaded and cached by role.

---

## 5. Authorization

IAM uses permission-based authorization.

Permission codes follow this pattern:

```text
module.resource.action
```

Examples:

```text
iam.users.create
iam.roles.assign
iam.permissions.update
```

Permission constants live in:

```text
src/03.IAM/IAM.Domain/IamPermission.cs
```

Controllers should use constants.

Do not hardcode permission strings.

Example:

```csharp
[RequirePermission(IamPermission.Users.Update)]
```

Authorization support comes from Shared:
- `RequirePermissionAttribute`
- `PermissionAuthorizationHandler`
- `DistributedRolePermissionCache`

Authorization flow:

```text
Request reaches endpoint
  -> RequirePermissionAttribute provides required permission
  -> Authorization handler reads user claims
  -> system admin bypass check
  -> role IDs are read from JWT
  -> role permissions are loaded from cache/database
  -> required permission is checked
```

System admin users bypass permission checks.

---

## 6. Role Permission Cache

Role permissions are cached in Redis.

Why:
- Permissions are checked often.
- Role permissions change less often.
- Multiple API instances need the same permission data.

Cache key:

```text
role:{roleId}
```

When role permissions change:
- Cache for that role is invalidated.

Example changes:
- Permission assigned to role.
- Permission unassigned from role.

See also:
- `docs/redis.cache.md`

---

## 7. Application Services

Main services:

| Service | Responsibility |
| :--- | :--- |
| `AuthService` | Login, JWT generation, permission cache hydration. |
| `UserService` | User lifecycle, password updates, failed login, last login, delete. |
| `OrganizationService` | Organization lifecycle and code updates. |
| `RoleService` | Role lifecycle and role assignment to users. |
| `PermissionService` | Permission updates and permission assignment to roles. |
| `PermissionAuthorizationService` | Permission lookup for authorization support. |

Main orchestrator:

| Orchestrator | Responsibility |
| :--- | :--- |
| `ResgisterOrchestrator` | Coordinates registration flows across organization, user, and role operations. |

Application services should:
- Validate using validators.
- Use repositories through interfaces.
- Use unit of work for saving.
- Call domain entity methods.
- Enforce ownership where required.
- Publish audit logs after successful business actions.
- Return `Result` for business failures.

Application services should not:
- Access HTTP context directly.
- Use DbContext directly.
- Return EF entities from query endpoints.

---

## 8. Audit Logging

IAM publishes audit logs for important business and security actions.

IAM does not write directly to Sentinel tables.

Instead:

```text
IAM service
  -> IamAuditLogger
    -> IEventPublisher
      -> Redis audit-log-events stream
        -> Sentinel AuditLogConsumer
          -> Sentinel database
```

IAM audit helper:

```text
src/03.IAM/IAM.Application/Services/IamAuditLogger.cs
```

Contract:

```text
src/03.IAM/IAM.Application/Contracts/IIamAuditLogger.cs
```

Use audit logs for:
- Login success.
- Login failure.
- User update.
- User password update.
- User delete.
- Organization update.
- Role create/update.
- Role assign/unassign.
- Permission update.
- Permission assign/unassign.

Logger constants live in:

```text
src/03.IAM/IAM.Domain/IamConst.cs
```

Use constants for feature/action names.

Do not use magic strings.

---

## 9. Parameters

IAM uses Shared parameters for runtime security settings.

Examples:
- JWT expiration.
- Password expiration.
- Max failed login attempts.
- Lockout duration.

Parameter constants live in:

```text
src/03.IAM/IAM.Domain/IamParam.cs
```

Parameter runtime service comes from Shared:

```text
IParameterService
```

This lets IAM change some behavior without code deployment.

See also:
- `src/01.Shared/readme.md`
- `docs/redis.cache.md`

---

## 10. Persistence

IAM uses PostgreSQL through EF Core.

Main context:

```text
src/03.IAM/IAM.Infrastructure/IamDbContext.cs
```

Infrastructure contains:
- Entity configurations.
- Migrations.
- Repositories.
- Query repositories.
- `IamUnitOfWork`.

Write flow:

```text
Service
  -> repository tracks change
  -> unit of work saves
  -> audit fields are applied
```

Read flow:

```text
Service
  -> query repository
    -> DTO projection
```

Controllers should return DTOs, not entities.

---

## 11. API

Main controllers:

| Controller | Purpose |
| :--- | :--- |
| `AuthorizationController` | Login/auth endpoints. |
| `UserController` | User operations. |
| `OrganizationController` | Organization operations. |
| `RoleController` | Role operations and role assignment. |
| `PermissionController` | Permission operations and assignment. |
| `ParameterController` | Shared parameter management through IAM API. |

API project also contains:
- JWT configuration.
- API versioning.
- Global exception handler.
- Dependency injection setup.

Controllers should stay thin.

---

## 12. Docker

IAM API has a Dockerfile:

```text
src/03.IAM/IAM.API/Dockerfile
```

Application compose file:

```text
infra/docker-compose.apps.yaml
```

Docker URL:

```text
http://localhost:5055
```

Visual Studio local URL:

```text
https://localhost:4055
```

Docker and Visual Studio ports are intentionally different.

See also:
- `docs/docker.md`

---

## 13. Seeder

Default IAM data is created by the external seeder project.

Seeder project:

```text
src/00.Seeder/DatabaseSeeder
```

The seeder creates:
- Permissions.
- Roles.
- Role permissions.
- Parameters.
- Organizations.

IAM should not execute its own seeding logic.

See also:
- `docs/99.database.seeder.md`

---

## 14. Design Rules

- Keep IAM business rules in IAM.
- Keep Shared generic.
- Do not write directly to Sentinel.
- Publish audit/system events through Shared abstractions.
- Use permission constants.
- Do not hardcode permission strings.
- Do not store permissions in JWT.
- Store role IDs in JWT.
- Use Argon2 for password hashing.
- Return `Result` for business failures.
- Use repositories and unit of work for persistence.
- Use query repositories for DTO reads.
- Add/update tests when service behavior changes.

---

## 15. Related Docs

- `docs/03.iam.entities.md`
- `docs/folder-structure.md`
- `docs/redis.cache.md`
- `docs/redis.messaging-system.md`
- `docs/99.database.seeder.md`
- `src/01.Shared/readme.md`
- `src/02.Sentinel/readme.md`
