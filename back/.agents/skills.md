# Skills

Keep workflows small.

## Add Endpoint

1. Ask for endpoint shape if unclear.
2. Add controller action.
3. Add service contract and implementation.
4. Add request/response DTOs when needed.
5. Add tests.
6. Add Bruno file.

## Add Permission

1. Add constant in `IamPermission`.
2. Add controller `RequirePermission`.
3. Add seed in `SeederPermissions`.
4. Add role mapping in `SeederRolePermissions`.
5. Add or update Bruno file.

## Add Seeder

1. Make it idempotent.
2. Wire it in `DatabaseSeeder`.
3. Keep seed order correct.
4. Build API project.

## Add Tests

Ask which test project owns the tests if unclear.

## Update Docs

Update docs when architecture, permissions, seeders, tests, or endpoints change.
