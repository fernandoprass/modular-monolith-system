# Redis Messaging System

This document explains the Redis messaging used for module communication.

It is focused only on messaging.

Redis cache is documented in `redis.cache.md`.

---

## Why Redis Messaging Exists

Modules should not call each other databases.

Example:
- IAM should not insert directly into Sentinel tables.
- IAM publishes an event.
- Sentinel consumes the event.
- Sentinel persists the log.

This keeps modules decoupled.

It also allows more modules later.

---

## Architecture Overview

This is the high-level flow.

IAM is the current publisher.

Billing is only an example of a future module.

Sentinel is the current log consumer.

```text
┌─────────────────────────────────────────────────────────────────┐
│                      Redis Server (Port 6379)                   │
│  ┌──────────────────────┐  ┌──────────────────────────────────┐ │
│  │ Redis Streams        │  │ Redis Pub/Sub                    │ │
│  │ - audit-log-events   │  │ - notification-events            │ │
│  │ - system-log-events  │  │                                  │ │
│  └──────────────────────┘  └──────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
           ↑                              ↑
           │ Publish                      │ Publish
           │                              │
┌──────────┴────────┐          ┌─────────┴──────────┐
│ IAM Module        │          │ Future Module      │
│ - Users           │          │ - Business Actions │
│ - Authentication  │          │ - Domain Events    │
│ - Roles           │          │                    │
└───────────────────┘          └────────────────────┘
           │
           │ Consume
           ↓
┌──────────────────────────────────────────────────────┐
│ Sentinel Module (Logging & Monitoring)               │
│ - AuditLogConsumer (Background Service)              │
│ - SystemLogConsumer (Background Service)             │
│ - PostgreSQL Storage (AuditLog, SystemLog tables)    │
└──────────────────────────────────────────────────────┘
```

The important idea:

```text
Source module -> Redis -> Sentinel -> Database
```

The source module does not reference Sentinel entities or repositories.

---

## Current Messaging Patterns

| Pattern | Use Case | Persistence | Delivery Guarantee |
|---------|----------|-------------|-------------------|
| **Redis Streams** | Audit events, system logs | Persistent when Redis persistence is enabled | At-least-once with consumer groups |
| **Redis Pub/Sub** | Real-time notifications | Ephemeral memory only | Fire-and-forget |

Streams are the main pattern today.

Pub/Sub exists in the publisher, but notifications are not the main focus yet.

---

## Streams

Redis Streams work like an append-only message log.

A producer appends events to a stream.

A consumer group reads entries from the stream.

The consumer acknowledges the entry after processing.

If processing fails, the entry is not acknowledged.

That makes it possible to inspect or retry pending messages.

Current streams:

| Stream | Event | Consumer |
| --- | --- | --- |
| `audit-log-events` | `AuditLogEvent` | `AuditLogConsumer` |
| `system-log-events` | `SystemLogEvent` | `SystemLogConsumer` |

The stream names are centralized in `SharedConst.Redis`.

Sentinel-specific consumer group names are centralized in `SentinelConst.Redis`.

---

## File Structure

The messaging contract starts in Shared.

The consumer implementation lives in Sentinel.

### Shared

```text
src/01.Shared/
├── Shared.Domain/
│   ├── Events/
│   │   ├── AuditLogEvent.cs
│   │   ├── SystemLogEvent.cs
│   │   └── NotificationEvent.cs
│   ├── Enums/
│   │   ├── AuditPrivacyLevel.cs
│   │   ├── SystemLogLevel.cs
│   │   └── SystemLogStatus.cs
│   ├── Interfaces/
│   │   └── IEventPublisher.cs
│   └── SharedConst.cs
└── Shared.Infrastructure/
    └── Messaging/
        └── RedisEventPublisher.cs
```

Notes:
- Event contracts are in Domain because modules can reference them.
- Redis implementation is in Infrastructure because Redis is an external dependency.
- Constants like stream names are in `SharedConst.Redis`.

### Sentinel

```text
src/02.Sentinel/
├── Sentinel.Domain/
│   ├── Entities/
│   │   ├── AuditLog.cs
│   │   └── SystemLog.cs
│   ├── Interfaces/
│   │   ├── ISentinelUnitOfWork.cs
│   │   └── Repositories/
│   │       ├── IAuditLogRepository.cs
│   │       └── ISystemLogRepository.cs
│   ├── QueryRepositories/
│   │   └── ISentinelLogQueryRepository.cs
│   └── SentinelConst.cs
├── Sentinel.Infrastructure/
│   ├── BackgroundServices/
│   │   ├── RedisStreamConsumer.cs
│   │   ├── AuditLogConsumer.cs
│   │   └── SystemLogConsumer.cs
│   ├── Repositories/
│   │   ├── AuditLogRepository.cs
│   │   └── SystemLogRepository.cs
│   ├── UoW/
│   │   └── SentinelUnitOfWork.cs
│   └── SentinelDbContext.cs
└── Sentinel.API/
    ├── Controllers/
    │   └── LogController.cs
    └── Program.cs
```

Notes:
- Consumers are background services.
- Consumers persist data through `ISentinelUnitOfWork`.
- API controllers read logs through application/query services.

---

## Shared Publisher

The publisher contract lives in Shared Domain.

File:
- `src/01.Shared/Shared.Domain/Interfaces/IEventPublisher.cs`

Methods:

```csharp
Task PublishAuditLogEventAsync(AuditLogEvent auditEvent, CancellationToken cancellationToken = default);
Task PublishSystemLogEventAsync(SystemLogEvent systemLog, CancellationToken cancellationToken = default);
Task PublishNotificationEventAsync(NotificationEvent notification, CancellationToken cancellationToken = default);
```

Implementation:
- `src/01.Shared/Shared.Infrastructure/Messaging/RedisEventPublisher.cs`

The implementation uses:
- `IConnectionMultiplexer`
- `IDatabase.StreamAddAsync` for streams
- `ISubscriber.PublishAsync` for Pub/Sub

Important behavior:
- Events are serialized as JSON.
- The Redis stream field name is `event`.
- The publisher checks `CancellationToken`.
- The publisher does not know Sentinel.

---

## Publishing an Audit Event

Use audit events for business actions.

Examples:
- Login success or failure.
- User updated.
- Role assigned.
- Parameter override deleted.

Most services should not manually build the whole event every time.

Prefer module helpers.

Example:
- IAM uses `IamAuditLogger`.

Small direct example:

```csharp
await _eventPublisher.PublishAuditLogEventAsync(new AuditLogEvent
{
   Module = "iam",
   Feature = "users",
   Action = "update",
   PrivacyLevel = AuditPrivacyLevel.Medium,
   Description = "Updated user",
   UserId = _userContext.UserId,
   OrganizationId = _userContext.UserOwnerId,
   TargetId = user.Id,
   IpAddress = _userContext.IpAddress,
   UserAgent = _userContext.UserAgent,
   Metadata = JsonSerializer.Serialize(request)
}, cancellationToken);
```

Rules:
- `Module` is the module name in lowercase.
- `Feature` is the domain area, like `users`, `roles`, `permissions`.
- `Action` is generic, like `create`, `update`, `assign`, `unassign`.
- `Metadata` must be valid JSON.
- Do not put passwords or secrets in metadata.

---

## Publishing a System Log Event

Use system logs for technical errors.

Examples:
- Unhandled exception.
- Background worker failure.
- Infrastructure failure.

IAM and other modules publish system logs through Shared exception handling.

Sentinel persists its own API exceptions directly.

That avoids losing Sentinel errors if Redis is unavailable.

Small example:

```csharp
await _eventPublisher.PublishSystemLogEventAsync(new SystemLogEvent
{
   Level = SystemLogLevel.Error,
   Status = SystemLogStatus.Error,
   Source = "IAM.API",
   Message = exception.Message,
   Exception = exception.GetType().Name,
   StackTrace = exception.StackTrace,
   RequestId = httpContext.TraceIdentifier,
   UserId = _userContext.UserId,
   OrganizationId = _userContext.UserOwnerId
}, cancellationToken);
```

---

## Consumers

Consumers live in Sentinel Infrastructure.

Files:
- `src/02.Sentinel/Sentinel.Infrastructure/BackgroundServices/RedisStreamConsumer.cs`
- `src/02.Sentinel/Sentinel.Infrastructure/BackgroundServices/AuditLogConsumer.cs`
- `src/02.Sentinel/Sentinel.Infrastructure/BackgroundServices/SystemLogConsumer.cs`

`RedisStreamConsumer<TEvent>` contains the generic stream loop.

It handles:
- Creating the consumer group.
- Reading entries with `StreamReadGroupAsync`.
- Deserializing JSON.
- Calling `ProcessEventAsync`.
- Acknowledging successful entries.
- Logging failures.
- Delaying when the stream is empty.
- Backoff after errors.

Concrete consumers only define:
- Stream name.
- Consumer group.
- Consumer name prefix.
- Display name.
- Error message.
- How to map the event to Sentinel entities.

---

## Consumer Scope Rule

Hosted services are singletons.

Repositories and unit of work are scoped.

So consumers must create a scope before resolving scoped services.

Example:

```csharp
using var scope = _serviceProvider.CreateScope();
var unitOfWork = scope.ServiceProvider.GetRequiredService<ISentinelUnitOfWork>();
```

Do not inject scoped repositories directly into a hosted service constructor.

That will fail service validation.

---

## Audit Consumer Flow

`AuditLogConsumer`:

1. Reads `AuditLogEvent` from Redis.
2. Creates a DI scope.
3. Resolves `ISentinelUnitOfWork`.
4. Maps the event to `AuditLog`.
5. Adds the log.
6. Saves changes.
7. Acknowledges the Redis stream entry.

Small mapping example:

```csharp
var auditLog = AuditLog.Create(
   auditEvent.Id,
   auditEvent.Timestamp,
   auditEvent.Module,
   auditEvent.Feature,
   auditEvent.Action,
   auditEvent.PrivacyLevel,
   auditEvent.Description,
   auditEvent.UserId,
   auditEvent.OrganizationId,
   auditEvent.TargetId,
   auditEvent.IpAddress,
   auditEvent.UserAgent,
   auditEvent.Metadata);
```

---

## System Log Consumer Flow

`SystemLogConsumer`:

1. Reads `SystemLogEvent` from Redis.
2. Creates a DI scope.
3. Resolves `ISentinelUnitOfWork`.
4. Serializes `Properties`.
5. Maps the event to `SystemLog`.
6. Adds the log.
7. Saves changes.
8. Acknowledges the Redis stream entry.

Small mapping example:

```csharp
var propertiesJson = JsonSerializer.Serialize(systemLogEvent.Properties, JsonOptions);

var systemLog = SystemLog.Create(
   systemLogEvent.Id,
   systemLogEvent.Timestamp,
   systemLogEvent.Level,
   systemLogEvent.Status,
   systemLogEvent.Source,
   systemLogEvent.Message,
   systemLogEvent.Exception,
   systemLogEvent.StackTrace,
   systemLogEvent.RequestId,
   systemLogEvent.UserId,
   systemLogEvent.OrganizationId,
   propertiesJson);
```

---

## How To Add a New Stream Consumer

Use this when a future module needs durable message processing.

Steps:

1. Create an event contract in Shared Domain if multiple modules use it.
2. Add stream constants.
3. Add a publisher method if needed.
4. Create a concrete consumer that inherits `RedisStreamConsumer<TEvent>`.
5. Implement `ProcessEventAsync`.
6. Register it as hosted service.
7. Add tests for processing and bad payload behavior.

Skeleton:

```csharp
public class MyEventConsumer(
   IConnectionMultiplexer redis,
   IServiceProvider serviceProvider,
   ILogger<MyEventConsumer> logger)
   : RedisStreamConsumer<MyEvent>(redis, logger)
{
   protected override string StreamName => "my-events";
   protected override string ConsumerGroup => "my-consumer-group";
   protected override string ConsumerNamePrefix => "my-consumer";
   protected override string ConsumerDisplayName => "My event consumer";
   protected override string ProcessingErrorMessage => "Error processing my event";

   protected override async Task ProcessEventAsync(MyEvent eventData, CancellationToken cancellationToken)
   {
      using var scope = serviceProvider.CreateScope();

      // Resolve scoped dependencies here.
      // Map event to module behavior here.
   }
}
```

---

## Troubleshooting

Use Redis CLI:

```bash
docker exec -it redis redis-cli
```

Useful commands:

```bash
XLEN audit-log-events
XINFO STREAM audit-log-events
XINFO GROUPS audit-log-events
XPENDING audit-log-events sentinel-audit-consumer
XREAD STREAMS audit-log-events 0
```

Common problems:

| Problem | Check |
| --- | --- |
| Consumer does not process events | Is Sentinel running? Is the consumer group created? |
| Events are stuck pending | Consumer failed after reading. Check Sentinel logs. |
| Event cannot deserialize | Check event contract and JSON shape. |
| App cannot connect to Redis | Check connection string and Docker network. |

---

## Design Rules

- Publishers must not reference Sentinel entities.
- Consumers must not contain business logic from source modules.
- Events should contain enough context to persist the log.
- Events should not contain secrets.
- Use streams for durable processing.
- Use Pub/Sub only for messages that can be missed.
