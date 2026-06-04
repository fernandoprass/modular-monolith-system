# Sentinel Module

Sentinel is the logging and monitoring module.

It centralizes:
- Audit logs.
- System logs.
- Log search APIs.
- Background log consumers.

Sentinel stores logs in PostgreSQL through EF Core.

The module lives in:

```text
src/02.Sentinel
```

---

## 1. Purpose

Sentinel exists so other modules do not need to store their own logs.

Example:
- IAM publishes an audit event when a role is assigned.
- Sentinel consumes the event.
- Sentinel stores the audit log.
- The UI or API can search the log later.

This keeps logging consistent across modules.

It also keeps modules decoupled.

IAM does not need to know Sentinel database details.

---

## 2. Project Structure

```text
src/02.Sentinel/
+-- Sentinel.API/
+-- Sentinel.Application/
+-- Sentinel.Domain/
+-- Sentinel.Infrastructure/
```

| Project | Responsibility |
| :--- | :--- |
| `Sentinel.API` | HTTP endpoints, middleware, startup, exception handling. |
| `Sentinel.Application` | Log query services and validation. |
| `Sentinel.Domain` | Entities, DTOs, constants, permissions, repository contracts. |
| `Sentinel.Infrastructure` | EF Core, repositories, unit of work, Redis stream consumers. |

---

## 3. Main Flow

Sentinel has two main flows.

### Write Flow

Logs are usually written by background consumers.

```text
Other module
  -> publishes event to Redis
    -> Sentinel background consumer
      -> creates Sentinel entity
        -> saves with ISentinelUnitOfWork
```

Current consumers:
- `AuditLogConsumer`
- `SystemLogConsumer`

Base consumer:
- `RedisStreamConsumer<TEvent>`

Persistence:
- `SentinelDbContext`
- `AuditLogRepository`
- `SystemLogRepository`
- `SentinelUnitOfWork`

### Read Flow

Logs are read through the Sentinel API.

```text
HTTP request
  -> LogController
    -> SentinelLogService
      -> ISentinelLogQueryRepository
        -> DTO response
```

Controllers should return DTOs, not entities.

---

## 4. Main Entities

Sentinel owns:
- `AuditLog`
- `SystemLog`

`AuditLog` stores business activity.

`SystemLog` stores technical activity.

See:
- `docs/02.sentinel.entities.md`

---

## 5. Redis Integration

Sentinel consumes Redis Streams.

Streams:
- `audit-log-events`
- `system-log-events`

The stream producer is usually another module.

The stream consumer is Sentinel.

Redis messaging details are documented in:
- `docs/redis.messaging-system.md`

Important rule:

Hosted services are singletons.

Repositories and unit of work are scoped.

So consumers create a scope before resolving `ISentinelUnitOfWork`.

---

## 6. Sentinel API

Current API controller:
- `LogController`

It exposes log read endpoints for:
- Audit logs.
- System logs.
- Log lookup by ID.
- Search/filter scenarios.

The API should:
- Validate input.
- Call application services.
- Return DTOs.
- Use Sentinel permissions.

The API should not:
- Query DbContext directly.
- Contain mapping logic.
- Persist events from other modules directly.

Exception note:

Sentinel API can persist its own exception logs directly through repository/unit of work.

This avoids losing Sentinel errors if Redis is unavailable.

Docker note:

Sentinel API has a Dockerfile:

```text
src/02.Sentinel/Sentinel.API/Dockerfile
```

Docker URL:

```text
http://localhost:5056
```

Visual Studio local URL:

```text
https://localhost:4056
```

---

## 7. Permissions

Sentinel permissions live in:

```text
src/02.Sentinel/Sentinel.Domain/SentinelPermission.cs
```

Use permission constants in controllers.

Do not hardcode permission strings.

---

## 8. Design Rules

- Other modules must not reference Sentinel entities.
- Other modules publish events instead.
- Sentinel persists logs.
- Logs are append-only.
- Query APIs return DTOs.
- Background consumers use scoped services through `IServiceProvider.CreateScope()`.
- Store extra context as JSON.
- Do not store secrets in logs.
