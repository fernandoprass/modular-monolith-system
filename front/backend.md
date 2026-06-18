# Backend Context For Frontend Agents

This file explains the backend at a practical level for frontend work.

Read it before building or changing the admin UI.

For more detail, read the docs folder.

Start with:

```text
docs/readme.md
docs/00.core.md
docs/00.core.e2e-tests.md
src/03.IAM/readme.md
```

---

## Big Picture

The backend is a modular .NET system.

It uses Clean Architecture.

Business modules live under `src/`.

Important modules:
- `00.Core`
- `01.Shared`
- `02.Sentinel`
- `03.IAM`
- `04.Courier`

The admin frontend should call the Core API.

Core API hosts module endpoints in one process.

Core API is the main target for frontend development.

---

## Core API

Core API lives in:

```text
src/00.Core/Core.API
```

It loads module controllers through ASP.NET Core Application Parts.

Current local URL:

```text
https://localhost:4050
```

Use this when Core API is running from Visual Studio or the local .NET profile.

Current Docker URL:

```text
http://localhost:5050
```

Use this when Core API is running from Docker.

Swagger:

```text
https://localhost:4050/swagger/index.html
http://localhost:5050/swagger/index.html
```

Core health endpoint:

```text
GET /api/v1/core/health
```

---

## API Versioning

Most module endpoints use this shape:

```text
/api/v1/{module}/{resource}
```

Examples:

```text
/api/v1/iam/users
/api/v1/iam/organizations
/api/v1/iam/roles
/api/v1/iam/permissions
/api/v1/iam/parameters
```

Keep API path strings in frontend constants.

Do not duplicate paths across pages.

---

## Result Response Shape

The backend wraps many responses in a Result object.

Example:

```json
{
  "data": {
    "id": "00000000-0000-0000-0000-000000000000"
  },
  "messages": [],
  "isSuccess": true,
  "title": null
}
```

Frontend data provider must unwrap this shape.

React-admin wants:

```ts
{ data: record }
```

For lists, React-admin wants:

```ts
{ data: records, total: number }
```

If the backend returns an array inside `data`, use `data.length` as `total` until pagination metadata exists.

Keep this conversion in one shared data provider.

Do not unwrap Result objects inside React pages.

---

## Authentication

IAM owns authentication.

Login endpoint:

```text
POST /api/v1/iam/users/login
```

Login body:

```json
{
  "email": "admin@example.com",
  "password": "Password123!"
}
```

Login returns a JWT in the response data.

Authenticated requests must use:

```text
Authorization: Bearer {token}
```

Use a React-admin `authProvider`.

Store token access behind a helper.

Do not log tokens.

Do not store passwords.

---

## Authorization

The backend uses permission-based authorization.

Permission codes follow:

```text
module.resource.action
```

Examples:

```text
iam.users.view
iam.users.create
iam.roles.assign
iam.permissions.assign
```

Backend permission constants live in:

```text
src/03.IAM/IAM.Domain/IamPermission.cs
```

The frontend may mirror these constants for UI behavior.

Keep frontend permission constants in one file.

Frontend permission checks are for UX only.

The backend remains the source of truth.

---

## IAM Module

IAM means Identity and Access Management.

IAM owns:
- organizations
- users
- roles
- permissions
- login
- authorization data
- IAM audit events

IAM module path:

```text
src/03.IAM
```

Main frontend resources should likely start with:
- organizations
- users
- roles
- permissions
- parameters

---

## Important IAM Endpoints

Organizations:

```text
POST   /api/v1/iam/organizations
GET    /api/v1/iam/organizations/{id}
GET    /api/v1/iam/organizations
PUT    /api/v1/iam/organizations/{id}
PATCH  /api/v1/iam/organizations/{id}/code
DELETE /api/v1/iam/organizations/{id}
```

Users:

```text
POST   /api/v1/iam/users/login
GET    /api/v1/iam/users/{id}
GET    /api/v1/iam/users/me
GET    /api/v1/iam/users/by-organization/{organizationId}
POST   /api/v1/iam/users
PUT    /api/v1/iam/users/{id}
PUT    /api/v1/iam/users/me
PATCH  /api/v1/iam/users/me/password
PATCH  /api/v1/iam/users/{id}/organization-admin
DELETE /api/v1/iam/users/{id}
DELETE /api/v1/iam/users/me
```

Roles:

```text
GET    /api/v1/iam/roles
POST   /api/v1/iam/roles
PUT    /api/v1/iam/roles/{id}
DELETE /api/v1/iam/roles/{id}
POST   /api/v1/iam/roles/assign
DELETE /api/v1/iam/roles/unassign
GET    /api/v1/iam/roles/user/{userId}/permissions
```

Permissions:

```text
GET    /api/v1/iam/permissions
PUT    /api/v1/iam/permissions/{id}
POST   /api/v1/iam/permissions/assign
DELETE /api/v1/iam/permissions/unassign
POST   /api/v1/iam/authorization/check
```

Parameters:

```text
GET    /api/v1/iam/parameters
GET    /api/v1/iam/parameters/{id}
GET    /api/v1/iam/parameters/value?key={key}
POST   /api/v1/iam/parameters
PUT    /api/v1/iam/parameters/{id}
PUT    /api/v1/iam/parameters/{id}/override
DELETE /api/v1/iam/parameters/{id}
DELETE /api/v1/iam/parameters/{id}/override
```

Check the backend controllers before relying on this list.

Endpoints can change.

---

## DTO Naming

Backend DTOs use names like:
- `UserDto`
- `UserCreateRequest`
- `UserUpdateRequest`
- `OrganizationDto`
- `OrganizationCreateRequest`
- `RoleDto`
- `RoleCreateRequest`
- `PermissionDto`
- `ParameterDto`
- `ParameterValueDto`

Use matching TypeScript names when possible.

This makes frontend and backend easier to compare.

---

## Search Requests

Backend search endpoints often use query request objects.

Examples:
- `RoleSearchRequest`
- `PermissionSearchRequest`
- `ParameterSearchRequest`
- `OrganizationSearchRequest`

Frontend filters should map to these query parameters.

Keep query parameter names as constants when reused.

---

## Multi-Tenancy

Organizations own users and organization-specific roles.

The backend enforces ownership.

Frontend must still pass the correct organization id when creating users or organization roles.

Do not rely on frontend filtering for security.

Backend authorization and ownership checks are the source of truth.

---

## Parameters

Shared parameters are exposed through IAM API.

Parameters can have default values and override values.

Example parameter:

```text
IAM.Security.MaxPasswordAgeInDays
```

The value endpoint can return:
- `value`
- `defaultValue`
- `isOverride`
- `parameterOverrideId`
- `overrideType`

When deleting an override, use the parameter override id when that is what the endpoint expects.

Check current backend behavior before implementing delete UI.

---

## E2E Tests As Flow Documentation

The Core E2E tests are useful examples of real API flows.

Path:

```text
tests/00.Core/Core.API.EndToEnd.Tests
```

Read scenario files when building frontend workflows.

Important files:

```text
Scenarios/OrganizationEndToEndTests.cs
Scenarios/UserEndToEndTests.cs
Scenarios/RoleEndToEndTests.cs
Scenarios/PermissionEndToEndTests.cs
Scenarios/ParameterEndToEndTests.cs
Infrastructure/AuthenticatedCoreApiClient.cs
```

The authenticated test client shows working endpoint calls.

Use it as a backend integration reference.

---

## Local Development

Recommended local backend for frontend work:

```text
https://localhost:4050
```

Use this for Visual Studio or local .NET runs.

If using Docker:

```text
http://localhost:5050
```

Use this for `docker-compose.core.yaml`.

Keep frontend API base URL in environment configuration.

Do not hardcode it in pages.

Suggested frontend env variable:

```text
VITE_CORE_API_URL=https://localhost:4050
```

Docker example:

```text
VITE_CORE_API_URL=http://localhost:5050
```

---

## When More Detail Is Needed

Read these docs:

```text
docs/00.core.md
docs/00.core.e2e-tests.md
docs/deployment-modes.md
docs/docker.md
docs/redis.cache.md
docs/03.iam.entities.md
src/03.IAM/readme.md
src/01.Shared/readme.md
```

Read backend source when docs are not enough.

Do not guess endpoint contracts.
