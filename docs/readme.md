# Documentation Index

This folder contains project documentation.

Use this file as the starting point.

The files are grouped by prefix:
- `00` for general system documentation.
- `01` for Shared module documentation.
- `02` for Sentinel module documentation.
- `03` for IAM module documentation.

---

## General Docs

| File | Description |
| :--- | :--- |
| `00.folder-structure.md` | Explains the repository layout, Clean Architecture layers, module shape, dependency direction, and where new code should go. |
| `00.docker.md` | Explains Docker concepts, compose files, networks, volumes, ports, health checks, and how to run infrastructure and app containers. |
| `00.migrations.md` | Explains database migrations and how schema changes should be managed. |
| `00.database.seeder.md` | Explains the dedicated seeder project, what default data it creates, seed order, and how to run it. |
| `00.redis.messaging-system.md` | Explains Redis messaging, streams, Pub/Sub, event publishing, Sentinel consumers, and troubleshooting. |
| `00.redis.cache.md` | Explains Redis cache usage for role permissions and parameters, including cache structures and invalidation rules. |

---

## Shared Docs

| File | Description |
| :--- | :--- |
| `01.shared.entities.md` | Explains Shared base entities, audited entities, parameters, parameter overrides, and parameter relationships. |
| `01.shared.enums.md` | Explains Shared enums such as parameter type, override type, audit privacy level, and system log status/level. |
| `01.shared.repositories.md` | Explains the shared repository pattern, base repository, query repositories, ownership checks, and testing guidance. |
| `01.shared.uow.md` | Explains Unit of Work, automatic audit field population, transaction boundaries, and module-specific UoW implementations. |

Module overview:

- `src/01.Shared/readme.md`

Read it when you need the big picture for Shared services, contracts, repositories, parameters, Redis cache, Redis messaging, authorization, and exception handling.

---

## Sentinel Docs

| File | Description |
| :--- | :--- |
| `02.sentinel.entities.md` | Explains Sentinel entities, audit logs, system logs, event mapping, append-only behavior, and query DTOs. |

Module overview:

- `src/02.Sentinel/readme.md`

---

## IAM Docs

| File | Description |
| :--- | :--- |
| `03.iam.entities.md` | Explains IAM entities such as organizations, users, roles, permissions, user roles, and role permissions. |
| `03.iam.libraries.md` | Explains important libraries used by IAM and why they are used. |

Module overview:
- `src/03.IAM/readme.md`
