# Redis Cache

This document explains Redis cache usage.

Redis messaging is documented in `redis.messaging-system.md`.

Current cache areas:
- Role permissions.
- Parameters.

---

## Cache Overview

Redis is shared by all running API instances.

That means one instance can write a cache value and another instance can read it.

```text
┌─────────────────┐        ┌─────────────────┐
│ IAM API #1      │        │ IAM API #2      │
│ Authorization   │        │ Parameter reads │
└────────┬────────┘        └────────┬────────┘
         │                          │
         │ read/write cache          │ read/write cache
         ↓                          ↓
┌────────────────────────────────────────────┐
│ Redis                                      │
│ - role:{roleId}                            │
│ - param:System.MaxRetry                    │
│ - param:UI.Theme                           │
└────────────────────────────────────────────┘
         │
         │ cache miss
         ↓
┌────────────────────────────────────────────┐
│ Database                                   │
│ - role permissions                         │
│ - parameters                               │
│ - parameter overrides                      │
└────────────────────────────────────────────┘
```

Cache should make reads faster.

Database is still the source of truth.

---

## Why Cache Exists

Some data is read many times and changes rarely.

Examples:
- A role permission list.
- A system parameter value.

Without cache, every request may hit the database.

With Redis, all API instances share the same cached data.

This matters when the system runs more than one container or server.

---

## Redis Registration

Shared Infrastructure configures Redis cache.

File:
- `back/src/01.Shared/Shared.Infrastructure/SharedDependencyInjection.cs`

It uses:
- `AddStackExchangeRedisCache`
- `SharedConst.Redis.ConnectionString`

The role permission cache uses `IDistributedCache`.

The parameter cache uses `IConnectionMultiplexer` directly because it needs Redis hashes.

---

## Role Permission Cache

Class:
- `DistributedRolePermissionCache`

Location:
- `back/src/01.Shared/Shared.Infrastructure/Authorization`

Contracts:
- `IRolePermissionCache`
- `IRolePermissionCacheInvalidator`

Purpose:
- Store permission codes for a role.
- Avoid loading role permissions from database on every authorization check.

Flow:

```text
Authorization handler
        │
        │ asks permissions for role
        ↓
DistributedRolePermissionCache
        │
        ├── cache hit  -> return permissions
        │
        └── cache miss -> load permissions from DB
                         store in Redis
                         return permissions
```

Key format:

```text
role:{roleId}
```

Value:

```json
["iam.users.read","iam.users.update"]
```

Expiration:
- Uses `DistributedCacheEntryOptions.AbsoluteExpiration`.
- The caller decides the expiration date.

Read behavior:
- Empty role value returns an empty list.
- Missing cache returns an empty list.
- Invalid or empty payload returns an empty list.

Write behavior:
- Removes blank permissions.
- Removes duplicates case-insensitively.
- Serializes the result as JSON.

Invalidation:
- Remove the role key when permissions are assigned or unassigned.

Invalidation flow:

```text
Permission assigned/unassigned
        │
        ↓
PermissionService / RoleService
        │
        ↓
IRolePermissionCacheInvalidator.RemoveAsync(roleId)
        │
        ↓
Redis deletes role:{roleId}
```

Example:

```csharp
await _rolePermissionCacheInvalidator.RemoveAsync(role.Id, cancellationToken);
```

---

## Parameter Cache

Class:
- `ParameterRedisCacheRepository`

Contract:
- `IParameterCacheRespository`

Location:
- `back/src/01.Shared/Shared.Infrastructure/Repositories`

Purpose:
- Cache resolved parameter values.
- Support static parameters.
- Support overridable parameters.
- Avoid creating one Redis key per user when no override exists.

Two Redis data structures are used:

| Parameter Type | Redis Type | Why |
| :--- | :--- | :--- |
| Static | String | One global value for everyone. |
| Overridable | Hash | One default value plus only existing overrides. |

Key prefix:

```text
param:
```

Example key:

```text
param:UI.Theme
```

---

## Static Parameters

Static parameters cannot be overridden.

They are stored as Redis strings.

Example:

```text
key   = param:System.MaxRetry
value = 3
```

Operations:
- Read with `StringGetAsync`.
- Write with `StringSetAsync`.
- Delete with `KeyDeleteAsync`.

Flow:

```text
ParameterService.GetValueAsync("System.MaxRetry")
        │
        ↓
ParameterRedisCacheRepository.GetAsync
        │
        ├── Redis string exists
        │       ↓
        │    return cached value
        │
        └── Redis key missing
                ↓
             query database
                ↓
             SetAsync writes Redis string
                ↓
             return value
```

---

## Overridable Parameters

Overridable parameters are stored as Redis hashes.

One hash is used per parameter.

Example:

```text
key = param:UI.Theme

fields:
default = Blue
{ownerId} = Black
{userId} = Green
```

Diagram:

```text
Redis Hash: param:UI.Theme
┌──────────────────────────────────────┐
│ field              │ value           │
├────────────────────┼─────────────────┤
│ default            │ Blue            │
│ owner-organization │ Black           │
│ user-123           │ Green           │
└──────────────────────────────────────┘
```

This is a value-pointer strategy.

The default value is stored once.

Only real overrides are stored as extra fields.

No user field is created when the user uses the default value.

This saves memory.

---

## Override Lookup

`ParameterRedisCacheRepository.GetAsync` reads fields in one Redis call.

Current fields:

```csharp
var values = await _database.HashGetAsync(
   redisKey,
   [DefaultField, organizationId.ToString(), userId.ToString()]);
```

Current priority:

1. User override field.
2. Owner override field.
3. Cache miss.

The method intentionally returns `null` when no override field exists.

Then `ParameterService` queries the database.

That lets the service decide if the correct result is:
- A specific override.
- The default parameter value.
- Not found.

Lookup flow:

```text
Request UI.Theme for user-123
        │
        ↓
HMGET param:UI.Theme default ownerId userId
        │
        ├── userId field exists
        │       ↓
        │    return user override
        │
        ├── ownerId field exists
        │       ↓
        │    return owner override
        │
        └── no override field
                ↓
             return null
                ↓
             ParameterService queries DB
```

Why return null instead of default here?

Because the cache does not know if the user has an override missing from Redis.

The service checks the database and then returns the correct value.

---

## Writing Parameters

`SetAsync` receives `ParameterValueDto`.

Important flags:
- `CanBeOverride`
- `IsOverride`

If `CanBeOverride` is false:
- Store as string.

If `CanBeOverride` is true:
- Store `default`.
- Store owner field only when `IsOverride` is true.

Example:

```csharp
await _database.HashSetAsync(redisKey, "default", parameter.DefaultValue);

if (parameter.IsOverride)
{
   await _database.HashSetAsync(redisKey, ownerId.ToString(), parameter.Value);
}
```

---

## Invalidation

Default parameter changed:
- Remove the whole parameter key.

```csharp
await _parameterCache.RemoveAsync(parameter.Key, cancellationToken);
```

Override changed:
- Remove only the override field.

```csharp
await _parameterCache.RemoveOverrideAsync(parameter.Key, ownerId, cancellationToken);
```

This keeps other cached values warm.

Invalidation diagram:

```text
Default value changed
        ↓
Delete whole key: param:UI.Theme

Override changed
        ↓
Delete one hash field: param:UI.Theme[{ownerId or userId}]
```

---

## Null Cache Repository

Class:
- `ParameterNullCacheRepository`

Purpose:
- Allows the application to run when Redis cache is disabled.
- Keeps `ParameterService` independent of Redis availability.
- Implements the same interface but does not store values.

Use it when Redis is not configured.

---

## Testing Rules

Role permission cache tests should verify:
- Serialization.
- Empty role behavior.
- Absolute expiration.
- Removal.

Parameter cache tests should verify:
- Static string cache hit.
- Static cache miss.
- Hash lookup with user override.
- Hash lookup with owner override.
- Missing override returns null.
- Override removal deletes only one hash field.
