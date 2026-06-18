# Shared Module

Shared is the common foundation used by the other modules.

It contains reusable domain contracts, base entities, repositories, unit of work, parameter management, Redis messaging, Redis cache, authorization helpers, user context, and exception handling support.

The module lives in:

```text
src/01.Shared
```

---

## 1. Purpose

Shared exists to avoid duplicating cross-module code.

Examples:
- Every module needs base entities.
- Every module needs repositories and unit of work patterns.
- Every module needs access to the current user context.
- Multiple modules need to publish audit or system log events.
- Multiple modules need authorization helpers.
- Multiple modules need to read system parameters.

Shared should contain generic infrastructure and contracts.

Shared should not contain IAM-specific or Sentinel-specific business rules.

Good Shared code:
- `Entity`
- `EntityAudited`
- `IUserContext`
- `IEventPublisher`
- `BaseRepository`
- `UnitOfWork`
- `ParameterService`
- `DistributedRolePermissionCache`
- `RedisEventPublisher`

Bad Shared code:
- `UserService`
- `RoleService`
- `IamPermission`
- Sentinel log entity rules

---

## 2. Project Structure

```text
src/01.Shared/
+-- Shared.Application/
+-- Shared.Domain/
+-- Shared.Infrastructure/
```

| Project | Responsibility |
| :--- | :--- |
| `Shared.Domain` | Base entities, DTOs, enums, events, contracts, constants, messages, pure mappers. |
| `Shared.Application` | Base service, parameter service, validators, user context contract, cache contracts. |
| `Shared.Infrastructure` | EF Core, repositories, Redis, authorization handler, exception helpers, user context implementation. |

Dependency direction:

```text
Shared.Application -> Shared.Domain
Shared.Infrastructure -> Shared.Application / Shared.Domain
Other modules -> Shared projects
```

Shared Domain should stay free of infrastructure dependencies.

---

## 3. Shared.Domain

`Shared.Domain` contains common domain objects and contracts.

Main folders:

```text
Shared.Domain/
+-- DTOs/
+-- Entities/
+-- Enums/
+-- Events/
+-- Interfaces/
+-- Mappers/
+-- Messages/
+-- ParameterKey.cs
+-- SharedConst.cs
+-- SharedParam.cs
```

### Entities

Shared defines the base entity hierarchy.

```text
Entity<TId>
  -> Entity

EntityAudited<TId>
  -> EntityAudited
```

Use `Entity` when the record only needs an ID.

Use `EntityAudited` when the record needs:
- `CreatedAt`
- `CreatedBy`
- `UpdatedAt`
- `UpdatedBy`

The unit of work automatically fills audit fields for `EntityAudited`.

### Parameter Entities

Shared owns the parameter system.

Entities:
- `Parameter`
- `ParameterOverride`

`Parameter` stores the default value.

`ParameterOverride` stores a specific value for a user owner or user.

This lets the system have global settings with optional overrides.

Example:

```text
Parameter: UI.Theme = Blue
Override for owner A = Black
Override for user B = Green
```

### DTOs

Shared DTOs include:
- `ParameterDto`
- `ParameterLiteDto`
- `ParameterValueDto`
- `ExceptionResponseDto`

Request DTOs include:
- `ParameterCreateRequest`
- `ParameterUpdateRequest`
- `ParameterOwnerUpdateRequest`
- `ParameterSearchRequest`

DTO rule:
- Use `Dto` for data transfer.
- Use `Request` for input commands/searches.
- Use `Response` only for wrapper responses.

### Enums

Important enums:
- `ParameterType`
- `ParameterOverrideType`
- `AuditPrivacyLevel`
- `SystemLogLevel`
- `SystemLogStatus`

`ParameterType` tells the system how to interpret a string parameter value.

`ParameterOverrideType` tells the system if a parameter can be overridden by owner or user.

Audit/system log enums are shared because modules publish log events.

### Events

Shared event contracts are used for module communication.

Events:
- `AuditLogEvent`
- `SystemLogEvent`
- `NotificationEvent`

These are contracts only.

They do not persist anything by themselves.

Persistence belongs to Sentinel.

### Interfaces

Important interfaces:
- `IBaseRepository`
- `IUnitOfWork`
- `ISharedUnitOfWork`
- `IEventPublisher`
- `IExceptionSystemLogPublisher`
- `IParameterRepository`
- `IParameterOverrideRepository`
- `IParameterQueryRepository`
- `IParameterCacheRespository`

These allow Application code to depend on abstractions.

Infrastructure provides implementations.

### Constants

`SharedConst` centralizes shared constant values.

Examples:
- Database schema.
- Redis stream names.
- Redis cache key prefixes.
- Shared JWT claim names.
- Shared entity names.

`SharedParam` centralizes shared parameter keys.

---

## 4. Shared.Application

`Shared.Application` contains reusable application logic.

Main folders:

```text
Shared.Application/
+-- Contracts/
+-- Services/
+-- Validators/
```

### BaseService

`BaseService` contains shared service helpers.

Most important helper:
- `ExecuteIfUserOwnsAsync`

Purpose:
- Enforce ownership checks.
- Keep multi-tenancy checks consistent.
- Prevent users from modifying resources they do not own.

Concept:

```text
If current user owns resource -> execute action
Else -> return forbidden result
```

Use it in Application services before mutations that require tenant ownership.

### IUserContext

`IUserContext` represents the current user.

It provides:
- `UserId`
- `OrganizationId`
- `IsSystemAdmin`
- `IpAddress`
- `UserAgent`

Application services use it for:
- Ownership checks.
- Audit fields.
- Audit log publishing.
- Context-aware parameter resolution.

The ASP.NET implementation lives in Infrastructure.

Seeder has its own user context.

### ParameterService

`ParameterService` is the runtime service for system parameters.

It supports:
- Create parameter.
- Update parameter.
- Delete parameter.
- Search parameters.
- Read parameter value.
- Save override value.
- Delete override value.
- Typed reads like `GetIntAsync`, `GetBoolAsync`, `GetStringAsync`.

Parameter read flow:

```text
Application asks for parameter key
  -> ParameterService checks Redis cache
    -> cache hit returns value
    -> cache miss queries database
      -> cache is updated
      -> value is returned
```

Override resolution:

```text
User override
  -> owner override
    -> default parameter value
```

Audit logging:
- Parameter updates publish audit log events.
- Override saves publish audit log events.
- Override deletes publish audit log events.

Cache invalidation:
- Default parameter update removes the whole parameter cache key.
- Override update/delete removes only the override hash field.

### ParameterValidator

`ParameterValidator` validates parameter commands.

It checks things like:
- Required values.
- Existing keys.
- Valid update request.
- Valid override update request.

Validation returns `Result`.

Business validation should not throw exceptions.

---

## 5. Shared.Infrastructure

`Shared.Infrastructure` contains implementations and external framework integration.

Main folders:

```text
Shared.Infrastructure/
+-- Authorization/
+-- Configurations/
+-- ExceptionHandling/
+-- Messaging/
+-- Migrations/
+-- QueryRepositories/
+-- Repositories/
+-- Security/
+-- UoW/
+-- SharedDbContext.cs
+-- SharedDependencyInjection.cs
```

### SharedDbContext

`SharedDbContext` is the EF Core context for Shared entities.

It owns:
- `Parameter`
- `ParameterOverride`

The schema is defined in `SharedConst.Database.Schema`.

### Configurations

Configurations define EF Core mappings.

Examples:
- `BaseConfiguration`
- `BaseAuditedConfiguration`
- `ParameterConfiguration`
- `ParameterOverrideConfiguration`

Use configurations instead of putting EF Core attributes in domain entities.

This keeps Domain clean.

### Repositories

Shared repositories include:
- `BaseRepository`
- `ParameterRepository`
- `ParameterOverrideRepository`
- `ParameterRedisCacheRepository`
- `ParameterNullCacheRepository`

`BaseRepository` implements common CRUD behavior.

Specific repositories add domain-specific queries.

Repositories should not:
- Publish events.
- Return `Result`.
- Read HTTP context.
- Contain business validation.

### Query Repositories

Query repositories return DTOs.

Current query repository:
- `ParameterQueryRepository`

Rules:
- Use `AsNoTracking()`.
- Project to DTOs.
- Keep read models separate from write entities.

### Unit of Work

Shared provides:
- `UnitOfWork<TContext>`
- `SharedUnitOfWork`

The unit of work:
- Saves tracked changes.
- Applies audit fields to `EntityAudited`.
- Uses `IUserContext` for `CreatedBy` and `UpdatedBy`.

Normal write flow:

```text
Repository tracks change
  -> UnitOfWork.SaveChangesAsync
    -> audit fields are applied
    -> EF Core saves transaction
```

### Security

`AspNetUserContext` reads user information from the ASP.NET request.

It gets:
- User ID from claims.
- User owner ID from claims.
- System admin flag from claims.
- IP address from HTTP context.
- User agent from request headers.

Use `HeaderNames.UserAgent` for the user agent header.

Do not hardcode header names.

### Authorization

Shared contains reusable authorization infrastructure:
- `RequirePermissionAttribute`
- `PermissionAuthorizationHandler`
- `DistributedRolePermissionCache`

The permission handler:
1. Checks if the user is system admin.
2. Reads role IDs from claims.
3. Loads permissions for each role.
4. Checks if the required permission exists.

Role permissions are cached in Redis through `IDistributedCache`.

Cache key:

```text
role:{roleId}
```

Use permission constants in module controllers.

Do not hardcode permission strings in controllers.

### Messaging

Shared Infrastructure implements Redis event publishing.

Class:
- `RedisEventPublisher`

Contract:
- `IEventPublisher`

It publishes:
- `AuditLogEvent` to Redis Stream `audit-log-events`.
- `SystemLogEvent` to Redis Stream `system-log-events`.
- `NotificationEvent` to Redis Pub/Sub channel `notification-events`.

The publisher does not know Sentinel.

Sentinel consumes and persists log events.

### Exception Handling

Shared contains reusable exception response and system log helpers.

Main classes:
- `ExceptionResponseFactory`
- `ExceptionSystemLogPublisher`
- `SystemLogEventFactory`

Purpose:
- Standardize API error responses.
- Publish unhandled exceptions as `SystemLogEvent`.
- Keep module exception handlers small.

IAM uses the shared publisher path.

Sentinel can persist its own exception logs directly so errors are not lost if Redis is unavailable.

### Dependency Injection

`SharedDependencyInjection` registers Shared services.

It wires:
- Shared DbContext.
- Repositories.
- Query repositories.
- Unit of work.
- Parameter service.
- Validators.
- Redis event publisher.
- Redis parameter cache or null cache.
- Distributed role permission cache.
- Authorization services.

This is the main entry point for other modules to consume Shared infrastructure.

---

## 6. Parameter System

The parameter system is one of the most important parts of Shared.

It allows runtime configuration without code changes.

Example parameters:
- Security max login attempts.
- Lockout duration.
- UI theme.
- Feature flags.

### Parameter Key

The parameter key is built from:

```text
Module.Group.Name
```

Example:

```text
IAM.Security.MaxLoginAttempts
```

`ParameterKey` can split a key into module, group, and name.

### Static Parameter

Static parameter:
- One default value.
- No override.
- Cached as Redis string.

Redis example:

```text
param:IAM.Security.MaxLoginAttempts = 5
```

### Overridable Parameter

Overridable parameter:
- Has a default value.
- Can have owner/user override.
- Cached as Redis hash.

Redis example:

```text
param:UI.Theme
  default = Blue
  owner-id = Black
  user-id = Green
```

Memory rule:
- Do not store user fields when the user uses the default value.
- Store only real overrides.

---

## 7. Redis Cache

Shared uses Redis for:
- Parameter cache.
- Role permission cache.

Parameter cache:
- `ParameterRedisCacheRepository`
- Uses `IConnectionMultiplexer`.
- Supports strings and hashes.

Null cache:
- `ParameterNullCacheRepository`
- Used when Redis parameter cache is disabled.
- Allows app to run without cache.

Role permission cache:
- `DistributedRolePermissionCache`
- Uses `IDistributedCache`.
- Stores JSON permission codes.
- Uses absolute expiration.

See also:
- `docs/redis.cache.md`

---

## 8. Redis Messaging

Shared defines and publishes events.

Sentinel consumes log events.

Flow:

```text
Module service
  -> IEventPublisher
    -> Redis stream
      -> Sentinel consumer
        -> Sentinel database
```

Audit event:
- Business action.
- Example: user updated, role assigned.

System log event:
- Technical event.
- Example: unhandled exception.

Notification event:
- Real-time message.
- Fire-and-forget.

See also:
- `docs/redis.messaging-system.md`

---

## 9. Result Pattern

Shared services follow the `Result` pattern.

Business failures return `Result.Failure`.

They should not throw exceptions for normal validation failures.

Examples:
- Not found.
- Forbidden ownership.
- Validation error.

Exceptions are for unexpected failures.

---

## 10. Multi-Tenancy

Shared supports multi-tenancy through `IUserContext`.

Important IDs:
- `UserId`: current user.
- `OrganizationId`: organization/tenant owner.

Services use these IDs to:
- Apply audit fields.
- Check ownership.
- Resolve parameter overrides.
- Publish audit logs.

Ownership should be enforced in Application services.

Repositories should not enforce tenant rules.

---

## 11. Testing Guidance

Shared tests should cover:
- Parameter service behavior.
- Parameter cache behavior.
- Role permission cache behavior.
- Exception response factory.
- Exception system log publisher.
- Base Redis stream behavior when applicable.

When adding behavior:
- Test the owning layer.
- Mock repositories in application service tests.
- Use cache tests for Redis behavior.
- Verify audit/log publishing on successful actions.

Do not add cross-layer test references without checking ownership.

---

## 12. Design Rules

- Keep Shared generic.
- Do not add module-specific business rules to Shared.
- Keep Domain free from infrastructure dependencies.
- Use interfaces in Domain/Application.
- Implement external details in Infrastructure.
- Use `CancellationToken` in async methods.
- Use `DateTime.UtcNow` for server timestamps.
- Use `Result` for business failures.
- Use repositories for data access.
- Use unit of work for saving.
- Use query repositories for DTO reads.
- Use constants for Redis keys, claim names, entities, and schemas.
- Do not store secrets in audit metadata or system log properties.

---

## 13. Related Docs

Useful documentation:

- `docs/01.shared.entities.md`
- `docs/01.shared.enums.md`
- `docs/01.shared.repositories.md`
- `docs/01.shared.uow.md`
- `docs/redis.cache.md`
- `docs/redis.messaging-system.md`
- `docs/folder-structure.md`

Use this readme as the module overview.

Use the docs folder for deeper topic-specific documentation.
