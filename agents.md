# Development Conventions & Standards

This document defines the architectural conventions, naming standards, and implementation patterns for this modular .NET system built with .NET 10, PostgreSQL (code-first approach without database-first scaffolding), and Clean Architecture principles.

Communicate with me using only short sentences; caveman language is enough. Do not provide lengthy explanations; I will ask questions if necessary.

## Collaboration Rules

- Never make assumptions. Ask questions to clarify ambiguity.
- Show me your plan before making changes.
- After showing the plan, wait for explicit approval like `go` before editing files.
- If file location, project layer, test project, naming, or design ownership is unclear, ask before changing files.
- Do not choose a test project, endpoint shape, permission name, or folder location without confirmation.
- If I ask for discussion or review, do not edit files until I approve the plan.
- When you see a better name, suggest it with a short reason before changing it.

---

## Table of Contents
1. [Collaboration Rules](#collaboration-rules)
2. [Architectural Principles](#architectural-principles)
3. [Naming Conventions](#naming-conventions)
4. [Async & Cancellation](#async--cancellation)
5. [Security & Authentication](#security--authentication)
6. [Entity & Domain Design](#entity--domain-design)
7. [Data Access Patterns](#data-access-patterns)
8. [Error Handling & Validation](#error-handling--validation)
9. [Date & Time Handling](#date--time-handling)
10. [Authorization & Permissions](#authorization--permissions)
11. [Audit Logging](#audit-logging)
12. [Caching Rules](#caching-rules)
13. [Docker & Local Environment](#docker--local-environment)
14. [Documentation Rules](#documentation-rules)
15. [Testing Rules](#testing-rules)
16. [Seeder Rules](#seeder-rules)

---

## Architectural Principles

Each business module is self-contained with Domain, Application, Infrastructure, and API layers. 

### Clean Architecture Layers

**Dependency Flow**: API -> Application -> Infrastructure -> Domain

**Layer Responsibilities**:

| Layer | Responsibility | Dependencies |
|-------|---------------|--------------|
| **Domain** | Pure business logic, dtos, entities, value objects | None |
| **Application** | Business orchestration, validation, services | Domain, Shared |
| **Infrastructure** | Data access, external services, EF Core | Domain, Application, Shared |
| **API** | HTTP endpoints, middleware, JWT config | All layers |

### Domain-Driven Design (DDD)

- **Entities are pure**: No service dependencies, no I/O operations
- **Business rules live in entities**: Use methods like `User.RecordFailedLogin(maxAttempts, lockoutDuration)`
- **Services orchestrate**: Fetch external data (parameters, config), pass to domain entities
- **Private setters**: Enforce encapsulation, expose behavior through methods

**Example**:
```csharp
// OK: Domain logic in entity
public void RecordFailedLogin(int maxAttempts, int lockoutDurationMinutes)
{
    NumFailedLoginAttempts++;
    if (NumFailedLoginAttempts >= maxAttempts)
    {
        LockedOutUntil = DateTime.UtcNow.AddMinutes(lockoutDurationMinutes);
    }
}

// Wrong: Service calculates lockout
public void UpdateFailedLogin(DateTime? lockedOutUntil)
{
    NumFailedLoginAttempts++;
    LockedOutUntil = lockedOutUntil; // Service should not decide this
}
```

### Multi-Tenancy

- **Enforcement**: `BaseService.ExecuteIfUserOwnsAsync()` validates ownership via `IUserContext`
- **Isolation**: Every mutation checks `UserOwnerId` matches entity's `OrganizationId`
- **JWT Claims**: Include `UserOwnerId` for tenant identification

---

## Naming Conventions

### DTOs (Data Transfer Objects)
Ensure immutability and value-based equality.

**Pattern**: `{Entity}{Purpose}` suffix with `Dto` or `Request`/`Response`

```csharp
// OK
public class UserDto { }
public class UserCreateRequest { }
public class LoginResponse { } // Wrapper object

// Avoid
public class UserResponse { } // Use 'Dto' instead
public class CreateUserRequest { } // Verb should be suffix
```

**Rule**: Use `Dto` for data transfer, `Request`/`Response` only for wrapper objects with metadata.

### Entity Properties

**Identifiers**:
- Use `Code` for business identifiers: `Permission.Code = "iam.users.create"`
- Use `Key` for configuration lookups: `Parameter.Key = "IAM.Security.MaxLoginAttempts"`

**Normalization**:
- Always lowercase codes: `.ToLowerInvariant()`
- Trim and normalize emails: `email.ToLower().Trim()`

### Search Requests

Do not pass several filter parameters individually through controllers, services, or repositories.

Use `{Entity}SearchRequest` records for query filters.

```csharp
public record PermissionSearchRequest(string? Module, string? Resource, string? Action);
Task<Result<IEnumerable<PermissionDto>>> GetAllAsync(PermissionSearchRequest request, CancellationToken cancellationToken = default);
```

### Method Naming

**Repository Methods**:
```csharp
Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
Task AddAsync(User entity, CancellationToken cancellationToken = default);
void Update(User entity); // Synchronous by design
Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
```

**Service Methods**:
```csharp
Task<Result<UserDto>> CreateUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default);
Task<Result> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken = default);
Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
```

---

## Async & Cancellation

### CancellationToken Usage

**Rule**: Add `CancellationToken` to **ALL** async methods across **ALL** layers.

```csharp
// Repository
Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

// Service
Task<Result> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken = default);

// Controller
public async Task<IActionResult> Update(Guid id, [FromBody] UserUpdateRequest request, CancellationToken cancellationToken)
{
    var result = await _userService.UpdateAsync(id, request, cancellationToken);
    return result.HasError ? BadRequest(result.Messages) : NoContent();
}
```

**Default Parameter**: Always use `= default` for backward compatibility.

---

## Security & Authentication

### Password Hashing

**Algorithm**: Argon2 (via `Isopoh.Cryptography.Argon2`)

```csharp
// Hashing
var passwordHash = Argon2.Hash(request.Password);

// Verification
var isValid = Argon2.Verify(user.PasswordHash, request.Password);
```

**Never** store passwords in plain text. Always hash before persistence.


### JWT Token Structure

**Claims**:
```csharp
new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
new Claim(JwtRegisteredClaimNames.Email, user.Email),
new Claim(IamConst.Security.Claim.IsSystemAdmin, user.IsSystemAdmin.ToString()),
new Claim(IamConst.Security.Claim.UserOwnerId, user.OrganizationId.ToString()),
new Claim(IamConst.Security.Claim.Role, roleId.ToString()) // Multiple role claims
```

**Important**: Include **role IDs**, not permissions (permission lists can be too long for JWT).

## Entity & Domain Design

### Entity Hierarchy (Shared Module)

```csharp
Entity<TId>              // Generic identity base
    ↓
EntityAudited<TId>       // Adds CreatedAt, CreatedBy, UpdatedAt, UpdatedBy
    ↓
Entity                   // Specialization with Guid
EntityAudited            // Specialization with Guid + audit
```

**Usage**:
- **Use `Entity`**: For simple entities needing only `Guid` ID
- **Use `EntityAudited`**: For business-critical entities requiring audit trail

### Factory Methods

**Pattern**: Private constructor + static `Create()` method

```csharp
public class User : EntityAudited
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    // ... properties

    private User() { } // Prevents unauthorized instantiation

    public static User Create(string name, string email, string passwordHash, Guid organizationId)
    {
        return new User
        {
            Id = Guid.CreateVersion7(), // Sequential GUID for performance
            Name = name,
            Email = email.ToLower().Trim(),
            PasswordHash = passwordHash,
            IsActive = true,
            OrganizationId = organizationId
        };
    }
}
```

### Encapsulated Collections

```csharp
private readonly List<UserRole> _userRoles = new();
public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
```

**Rule**: Never expose mutable collections. Use `IReadOnlyCollection<T>` with private backing field.

---

## Data Access Patterns

### Repository Pattern

**CQRS Separation**:
- **Write Repository** (`IUserRepository`): Inherits `BaseRepository<User>`, used for mutations
- **Query Repository** (`IUserQueryRepository`): Read-only, returns DTOs, uses `AsNoTracking()`

```csharp
// Write Repository
public class UserRepository(IamDbContext dbContext) : BaseRepository<User>(dbContext), IUserRepository
{
    public async Task<IEnumerable<User>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .Where(u => u.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }
}

// Query Repository
public class UserQueryRepository(IamDbContext context) : IUserQueryRepository
{
    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Users
            .AsNoTracking()
            .Include(u => u.Organization)
            .Where(u => u.Id == id)
            .Select(u => u.ToUserDto()) // Project to DTO
            .SingleOrDefaultAsync(cancellationToken);
    }
}
```

### Unit of Work Pattern

**Automatic Auditing**: `UnitOfWork<TContext>` intercepts `SaveChangesAsync()` to populate audit fields.

```csharp
public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
{
    ApplyAuditInformation();
    return await _context.SaveChangesAsync(cancellationToken);
}

private void ApplyAuditInformation()
{
    var entries = _context.ChangeTracker.Entries<EntityAudited>();
    foreach (var entry in entries)
    {
        if (entry.State == EntityState.Added)
        {
            entry.Entity.CreatedAt = DateTime.UtcNow;
            entry.Entity.CreatedBy = _userContext.UserId;
        }
        else if (entry.State == EntityState.Modified)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
            entry.Entity.UpdatedBy = _userContext.UserId;
        }
    }
}
```

### Null-Safe Queries

**Problem**: Nullable parameters in LINQ queries

```csharp
// Wrong: Throws exception if name is null
.Where(r => r.Name.Contains(name))

// OK: Conditional query building
var query = _context.Roles
    .AsNoTracking()
    .Where(r => r.OrganizationId == null || r.OrganizationId == organizationId);

if (!string.IsNullOrWhiteSpace(name))
{
    query = query.Where(r => EF.Functions.ILike(r.Name, $"%{name}%")); // PostgreSQL case-insensitive
}

return await query.ToListAsync(cancellationToken);
```

### Defensive Null Checks

**Rule**: Always check for null after `GetByIdAsync()`, even in internal methods.

```csharp
public async Task<Result> UpdateAsync(Guid id, UserUpdateRequest request, CancellationToken cancellationToken = default)
{
    var user = await _userRepository.GetByIdAsync(id, cancellationToken);
    
    if (user == null)
    {
        return Result.Failure(new NotFoundError(IamConst.Entity.User));
    }
    
    // ... proceed with update
}
```

**Reason**: Database state can change (deletions, race conditions, external tools).

---

## Error Handling & Validation

### Result Pattern

**Library**: `Myce.Response`

**Never throw exceptions for business logic failures**. Use `Result<T>` or `Result`.

```csharp
// Service returns Result
public async Task<Result<UserDto>> CreateUserAsync(UserCreateRequest request, CancellationToken cancellationToken = default)
{
    var validation = _userValidator.ValidateCreate(request, organizationExists, emailExists);
    if (validation.HasError)
    {
        return Result<UserDto>.Failure(validation.Messages);
    }
    
    var user = User.Create(...);
    await _iamUnitOfWork.Users.AddAsync(user, cancellationToken);
    await _iamUnitOfWork.SaveChangesAsync(cancellationToken);
    
    return Result<UserDto>.Success(user.ToUserDto());
}

// Controller handles Result
var result = await _userService.CreateUserAsync(request, cancellationToken);
return result.HasError ? BadRequest(result.Messages) : Created("", result.Data);
```

### Validation Pattern

**Library**: `Myce.FluentValidator`

**Validators are stateless** and receive "facts" from services:

```csharp
public class UserValidator : IUserValidator
{
    public Result ValidateCreate(UserCreateRequest request, bool organizationExists, bool emailAlreadyExists)
    {
        var validator = new FluentValidator<UserCreateRequest>()
            .RuleFor(x => x.Name).ApplyTemplate(ValidatorTemplate.NameRules)
            .RuleFor(x => x.Email).ApplyTemplate(ValidatorTemplate.EmailRules)
            .RuleFor(x => x.Password).ApplyTemplate(ValidatorTemplate.PasswordRules)
            .RuleForValue(emailAlreadyExists).IsFalse(new EmailAlreadyExistError(request.Email))
            .RuleForValue(organizationExists).IsTrue(new NotFoundError(IamConst.Entity.Organization));

        var isValid = validator.Validate(request);
        return isValid ? Result.Success() : Result.Failure(validator.Messages);
    }
}
```

**Service provides facts**:
```csharp
bool emailExists = await EmailExistsAsync(request.Email, cancellationToken);
var validation = _userValidator.ValidateCreate(request, organizationExists, emailExists);
```

### Exception Logging

- Shared exception handling logic should live in Shared.
- Module APIs should keep only module-specific wiring.
- IAM and other modules should publish system log events.
- Sentinel should persist its own exception logs directly through repository/unit of work.

---

## Date & Time Handling

### Use DateTime with UTC

**Rule**: Always use `DateTime.UtcNow` for server-side timestamps. **Do not use `DateTimeOffset`** for business logic.

```csharp
// OK
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime? LockedOutUntil { get; set; }

// Wrong
public DateTime CreatedAt { get; set; } = DateTime.Now; // Local time - ambiguous
```

**PostgreSQL**: Stores `timestamp without time zone` as UTC when using `DateTime.UtcNow`.

**When to use `DateTimeOffset`**: User-facing events with explicit timezones (e.g., "Meeting at 3 PM Paris time"). Not needed for IAM, audit trails, or server timestamps.

---

## Authorization & Permissions

### Permission Model

**Hierarchy**: `Module.Resource.Action` (lowercase)

```csharp
public class Permission : EntityAudited
{
    public string Module { get; private set; } = string.Empty; // "iam"
    public string Resource { get; private set; } = string.Empty;  // "users"
    public string Action { get; private set; } = string.Empty;   // "create"
    public string Code { get; private set; } = string.Empty;   // "iam.users.create"
    
    public static Permission Create(string module, string resource, string action, string title, string description)
    {
        return new Permission
        {
            Id = Guid.CreateVersion7(),
            Module = module,
            Resource = resource,
            Action = action,
            Code = $"{module}.{resource}.{action}".ToLowerInvariant(), // Normalized
            Title = title,
            Description = description,
            IsActive = true
        };
    }
}
```

**Usage**:
```csharp
Permission.Create("iam", "users", "create", "Create Users", "Allows creating new users");
// Code = "iam.users.create"
```

### Authorization Middleware

**Pattern**: Custom `RequirePermissionAttribute` + `AuthorizationHandler`

```csharp
[RequirePermission(IamPermission.Users.Create)]
public async Task<IActionResult> CreateUser([FromBody] UserCreateRequest request, CancellationToken cancellationToken)
{
    // Only executes if user has "iam.users.create" permission
}
```

**Attribute Rule**: `RequirePermissionAttribute` must be executable authorization metadata.

```csharp
public class RequirePermissionAttribute(string permission)
    : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public string Permission { get; } = permission;

    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}
```

If `PermissionAuthorizationHandler` is not called, check the attribute/policy wiring first.

**Permission Constants**:
- Never hardcode permission strings in controllers.
- Use `IamPermission`.
- When adding controller endpoints, update:
  - `IamPermission`
  - `SeederPermissions`
  - `SeederRolePermissions`
  - Bruno files (Bruno files live in tests/bruno-collectio, folders match controller areasn)

**Handler Logic**:
1. Check if user is `IsSystemAdmin` -> bypass all checks
2. Extract role IDs from JWT claims
3. Fetch permissions for each role (with caching)
4. Check if required permission exists in user's permission set

**Caching**: 15-minute cache for role permissions to reduce DB load.


**Dependency Injection**:
```csharp
// Program.cs
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
```

**Important**: Handler is **Singleton**, use `IServiceProvider.CreateScope()` to resolve scoped dependencies like `IRoleQueryRepository`.

---

## Audit Logging

- Use audit logging for user/business actions.
- Do not duplicate audit event creation in services.
- Use module-specific audit logger helpers, like `IamAuditLogger`.
- Use constants for log feature/action names. No magic strings.
- Feature should name the domain area, like `users`, `roles`, `permissions`.
- Action should be generic, like `create`, `update`, `delete`, `assign`, `unassign`.
- Do not log database seeder actions unless explicitly required.

---

## Caching Rules

- Cache interfaces belong in Shared.Domain when used across modules.
- Redis implementations belong in Infrastructure.
- Prefer targeted cache invalidation over deleting whole cache entries when possible.

---

## Docker & Local Environment

- Docker ports and local Visual Studio ports should be different.
- Do not change Docker ports just to fix local host conflicts.

---

## Documentation Rules

- Documentation must be written for junior developers.
- Keep text simple and concise, but complete enough to understand the context.
- Do not write documentation only for agents or senior developers.
- Before planning a code or docs task, read `docs/readme.md` and the relevant module `readme.md`.
- For cross-module tasks, read all affected module readmes.
- For docs tasks, also read the target doc before proposing changes.
- Preserve useful teaching material like concept tables, diagrams, flow examples, and file trees.
- Remove stale code examples, not useful explanations.
- Prefer small code snippets only when they teach the pattern.
- Use current project names, file paths, class names, and stream/cache names.
- If renaming or moving docs, keep module readmes and docs index aligned.
- Each module should have a `readme.md` in its module folder.
- The `docs/readme.md` file should act as an index for the docs folder.
- Module readmes should explain purpose, structure, flows, important services/entities, integration points, and design rules.
- Topic docs should go deeper than module readmes.
- When updating docs, touch only the requested docs unless the docs index or moved links also need updates.

---

## Testing Rules

- Before adding tests, ask which test project should own them if no matching test project exists.
- Do not add project references across layers without confirmation.
- Authorization handlers must have unit tests for success, failure, role claims, admin bypass, and cache behavior.
- When adding audit logging, update service tests to verify log dispatch on success.
- Do not add controller tests when behavior is only in application services.

---

## Seeder Rules

- Seeders must be idempotent.
- Do not create duplicates.
- Seed order matters:
  - parameters
  - permissions
  - roles
  - role permissions
- When adding permissions, update both permission seed data and role-permission seed data.

---

## Summary Checklist

**Before Committing Code**:

- [ ] All async methods have `CancellationToken` parameter
- [ ] DTOs use `Dto` suffix (not `Response`)
- [ ] Business logic in domain entities, not services
- [ ] All timestamps use `DateTime.UtcNow`
- [ ] Null checks after repository queries
- [ ] Permission codes are lowercase
- [ ] Controllers use `IamPermission`, not hardcoded permission strings
- [ ] New endpoints update permission constants, permission seeders, role-permission seeders, and Bruno files
- [ ] Validation uses `Result` pattern (no exceptions)
- [ ] Multi-tenancy enforced via `ExecuteIfUserOwnsAsync()`
- [ ] Authorization checks `IsSystemAdmin` first
- [ ] `RequirePermissionAttribute` implements `IAuthorizationRequirementData`
- [ ] Query repositories use `AsNoTracking()` and return DTOs
- [ ] Search filters use `{Entity}SearchRequest`
- [ ] Entities use private constructors + static `Create()` methods
- [ ] Collections are encapsulated with `IReadOnlyCollection<T>`

---

**Document Version**: 1.0  
**Last Updated**: 2025-04-28  
**Target Framework**: .NET 10.0  
**Database**: PostgreSQL with EF Core


