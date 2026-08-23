# Frontend Development Conventions

This file defines rules for frontend work under `front/`.

The current frontend app is:

```text
front/apps
```

It is the admin UI for the Core API backend.

It uses React, TypeScript, Vite, and shadcn/ui style components.

Communicate with the user using short sentences.

Ask before changing design ownership, folder structure, API contracts, route names, or state management patterns.

Show a plan before editing files.

Wait for `go` before making changes.

---

## MCP Tools

Use the shadcn MCP when working with shadcn/ui.

Use it to:
- search registry items
- view component examples
- get add commands

Prefer shadcn MCP examples before guessing component usage.

---

## SKILLS

Read `.agent/skill` if it exists.

If it does not exist, say so briefly and continue.

---

## Stack Direction

Use:
- React
- TypeScript
- Vite
- shadcn/ui style components
- Radix UI primitives when useful
- React Hook Form for create/edit/profile/modal forms
- Zod with `@hookform/resolvers` for form validation
- TanStack Table for reusable data tables

Do not use React-admin unless the user asks.

The app owns:
- admin shell
- routes
- resource pages
- forms
- filters
- notifications
- auth integration
- data loading patterns

shadcn/ui style components own:
- buttons
- inputs
- selects
- dialogs
- dropdowns
- checkboxes
- reusable UI primitives

React Hook Form owns:
- local create/edit/profile form state
- modal form state
- form submission handling
- field registration

Zod owns:
- client-side form schemas
- required field validation
- request-shape validation when useful

Plain CSS owns:
- app shell layout
- spacing
- density
- admin visual polish
- responsive behavior

---

## Backend Context

Before working on API integration, read:

```text
front/backend.md
docs/readme.md
docs/00.core.e2e-tests.md
back/src/00.Core/00.core.md
back/src/03.IAM/readme.md
```

Use backend docs for endpoint ownership and domain rules.

Do not guess backend behavior when an endpoint, DTO, permission, or flow is unclear.

Ask or inspect backend code.

---

## Project Structure

Keep frontend code organized by responsibility.

Recommended structure:

```text
front/apps/src
+-- app
+-- auth
+-- data
+-- resources
+-- shared
```

Use these folders:

| Folder | Purpose |
| :--- | :--- |
| `app` | Admin shell, providers, routes, layout, theme setup. |
| `auth` | Auth provider, login helpers, token storage. |
| `data` | HTTP client, API paths, API result unwrapping. |
| `resources` | Resource pages grouped by backend entity. |
| `shared` | Reusable constants, types, helpers, UI utilities. |

Keep resource-specific code inside its resource folder.

Example:

```text
resources/iam/users
+-- UserList.tsx
+-- UserCreate.tsx
+-- UserEdit.tsx
+-- userTypes.ts
+-- userResource.ts
```

Group resources by backend module.

Example:

```text
resources/iam/organizations
resources/iam/users
resources/iam/roles
resources/iam/permissions
resources/iam/parameters
```

Even though parameters are a Shared concept, the current admin API exposes them through IAM.

So keep parameters under `resources/iam/parameters` for now.

Reusable component helper files that belong to one component family must share the same file prefix.

Good:

```text
components/ui/data-table.tsx
components/ui/data-table-pagination.tsx
components/ui/data-table-row-actions.tsx
components/ui/data-table-sortable-button.tsx
```

Avoid mixed prefixes for the same component family.

---

## Listing Page Rules

Keep resource listing pages as similar as possible.

Use this standard shape:
- constants and local types first
- hooks
- permission booleans
- form setup
- column setup
- loader callback
- effects
- handlers
- derived render values
- JSX

For search forms, create an empty search constant.

Good:

```ts
const EMPTY_USER_SEARCH = {
  email: '',
  isActive: 'all',
  name: '',
}
```

Use applied filter state for loading data.

Do not load from current form values inside loader callbacks.

Good:

```tsx
const [appliedFilters, setAppliedFilters] = useState(EMPTY_USER_SEARCH)

const { handleSubmit, register, reset } = useForm<UserSearchForm>({
  defaultValues: EMPTY_USER_SEARCH,
})

function handleSearch(value: UserSearchForm) {
  setPageNumber(DEFAULT_PAGINATION.pageNumber)
  setAppliedFilters({ ...value })
}

<FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
  <Input id="name" {...register('name')} />
</FilterToolbar>
```

Reset should reset both the form and applied filters.

For paged list endpoints:
- keep `pageNumber` and `pageSize` state in the page
- reset `pageNumber` to `DEFAULT_PAGINATION.pageNumber` when filters or page size change
- use `PagedResultDto<T>` from `front/apps/src/shared/pagination.ts`
- do not duplicate `PagedResultDto<T>` in resource type files

For array-backed list endpoints, still use `appliedFilters`.

This keeps reload-after-save and reload-after-delete behavior predictable.

When using values from objects in memoized callbacks, derive primitive values first.

Good:

```ts
const organizationId = user?.organizationId ?? ''
```

Prefer depending on `organizationId` instead of `user?.organizationId` in callback dependency arrays.

---

## Form Rules

Use React Hook Form plus Zod for create, edit, profile, and modal forms.

Use `Controller` for controlled shadcn/Radix/custom inputs:
- `Select`
- `Checkbox`
- lookup selects such as `OrganizationSelect` and `UserSelect`
- date/time pickers

Use `register` for plain inputs and textareas.

Good:

```tsx
const form = useForm<UserForm>({
  defaultValues: EMPTY_USER_FORM,
  resolver: zodResolver(userSchema),
})

<Input id="name" required {...form.register('name')} />

<Controller
  control={form.control}
  name="language"
  render={({ field }) => (
    <Select
      onValueChange={field.onChange}
      options={toTranslatedOptions(LANGUAGE_OPTIONS, t)}
      value={field.value}
    />
  )}
/>
```

Normalize DTO data before it reaches the form whenever possible.

For fixed option values, the form value must exactly match the option value.

Example:

```ts
export const LANGUAGE_CODES = {
  english: 'en-US',
  portugueseBrazil: 'pt-BR',
  spanish: 'es-ES',
} as const
```

Do not paper over a bad read by falling back to a default in edit mode. Fix the API boundary or form initialization instead.

---

## Edit Page Rules

Keep edit and profile pages as similar as possible.

Use this standard shape:
- `EMPTY_*_FORM`
- `toForm(dto)`
- Zod schema
- request-mapping helper when the backend request shape differs from form shape
- load callback
- keyed child form component rendered after the DTO is loaded
- submit handler

Good:

```ts
const EMPTY_ROLE_FORM = {
  description: '',
  isActive: true,
  isDefault: false,
  name: '',
  organizationId: '',
}

function toForm(role: RoleDto): RoleForm {
  return {
    description: role.description,
    isActive: role.isActive,
    isDefault: role.isDefault,
    name: role.name,
    organizationId: role.organizationId ?? '',
  }
}
```

For edit pages, load the entity into state.

Then render a keyed child form component after the entity is loaded.

The child form should initialize React Hook Form from final props.

Good:

```ts
const [role, setRole] = useState<RoleDto | null>(null)

const loadRole = useCallback(async () => {
  const loaded = await getRole(id)
  setRole(loaded)
}, [id])

useEffect(() => {
  void loadRole()
}, [loadRole])

return role === null
  ? <p>{t('shared.common.loading')}</p>
  : <RoleEditForm key={role.id} role={role} />
```

Good:

```ts
function RoleEditForm({ role }: RoleEditFormProps) {
  const form = useForm<RoleForm>({
    defaultValues: toForm(role),
    resolver: zodResolver(roleSchema),
  })

  async function handleSave(value: RoleForm) {
    await updateRole(role.id, value)
  }

  return <form onSubmit={form.handleSubmit(handleSave)}>...</form>
}
```

Avoid long `form.setFieldValue` sequences after loading an entity.

Avoid `form.reset(toForm(dto))` as the main edit-page hydration strategy.

For edit mode, prefer mounting the child form after the DTO is loaded so the form is born with final `defaultValues`.

This matters for controlled Radix components such as `Select`, where async value changes can be easy to get wrong.

For create/edit pages, derive `isCreate` once.

Good:

```ts
const isCreate = id === undefined
```

For create mode, reset the form from the empty-form helper when context defaults change.

For modal forms, each modal should own its own React Hook Form instance.

When a modal opens or receives a different DTO, call `reset(...)` inside an effect.

Good:

```ts
useEffect(() => {
  if (!isOpen) {
    return
  }

  form.reset(item === null ? EMPTY_FORM : toForm(item))
}, [form, isOpen, item])
```

For DTO normalization, normalize unknown API data at the API boundary.

If a backend can return nested `data` wrappers, unwrap them in the API helper before the page sees the DTO.

Do not patch empty form fields inside pages when the real issue is response normalization.

---

## Data Table Rules

Use TanStack Table through the shared data table components.

Current shared data table files:

```text
components/ui/data-table.tsx
components/ui/data-table-pagination.tsx
components/ui/data-table-row-actions.tsx
components/ui/data-table-sortable-button.tsx
```

Action columns should be consistent.

Use:

```ts
{
  id: 'actions',
  enableHiding: false,
  enableSorting: false,
}
```

Use `DataTableRowActions` for row buttons.

Use icon buttons for common row actions such as view, edit, and delete.

For paged tables, use `DataTablePagination`.

Pagination must handle empty backend results.

The backend can return:

```json
{
  "totalCount": 0,
  "totalPages": 0
}
```

In that case the UI should show a zero range and disable navigation.

---

## No Magic Strings

Avoid magic strings.

Names that repeat must be constants.

Create constants for:
- API base URL keys
- API paths
- resource names
- route names
- local storage keys
- permission codes
- query parameter names
- field names used in filters more than once
- enum display labels
- action labels such as Add and Remove
- notification messages reused across files

Good:

```ts
export const RESOURCE_NAMES = {
  users: "users",
  organizations: "organizations",
} as const;
```

Good:

```ts
export const STORAGE_KEYS = {
  authToken: "core-admin.auth.token",
} as const;
```

Avoid:

```ts
localStorage.getItem("token");
```

Prefer:

```ts
localStorage.getItem(STORAGE_KEYS.authToken);
```

User-facing action labels must use translation keys.

Good:

```tsx
{t('shared.actions.add')}
```

Avoid:

```tsx
Add
```

---

## API Integration Rules

The backend uses a Result wrapper:

```json
{
  "data": {},
  "messages": [],
  "isSuccess": true,
  "title": null
}
```

Keep this conversion in one place.

Do not unwrap API results inside pages.

For custom actions, create reusable API functions under `data` or the resource folder.

---

## Authentication Rules

IAM login endpoint returns a JWT.

Store the token behind a small token storage helper.

Do not access `localStorage` directly across many files.

Use an app auth provider.

The auth provider should own:
- login
- logout
- auth check
- error check
- identity loading when available
- permission loading when available

Use the bearer token on authenticated requests:

```text
Authorization: Bearer {token}
```

Do not store passwords.

Do not log tokens.

---

## Permission Rules

Permission codes come from the backend.

Do not hardcode permission strings in pages.

Create frontend constants that mirror backend permission codes when needed.

Keep permission constants in one file.

Example:

```ts
export const IAM_PERMISSIONS = {
  users: {
    view: "iam.users.view",
    create: "iam.users.create",
  },
} as const;
```

Use permissions to:
- hide actions
- disable buttons
- protect routes when needed

Authorization is still enforced by the backend.

Frontend permission checks are only for user experience.

---

## TypeScript Rules

Use TypeScript types for backend DTOs.

Keep DTO names aligned with backend names when practical.

Examples:
- `UserDto`
- `UserCreateRequest`
- `OrganizationDto`
- `RoleDto`

Use `type` for simple object shapes.

Use `interface` only when extension or declaration merging is useful.

Avoid `any`.

If unknown data comes from HTTP, parse or narrow it at the data boundary.

Keep nullable fields explicit.

---

## UI Rules

This is an admin application.

Prefer clear and dense screens.

Avoid marketing-style pages.

Avoid oversized hero sections.

Avoid decorative UI that does not help the workflow.

Use standard admin patterns:
- lists
- filters
- detail pages
- edit forms
- create forms
- confirmation dialogs
- inline status chips
- tabs for related data

Use icons only when they clarify common actions.

Use short labels.

Keep forms predictable.

Do not hide important backend state.

---

## Internationalization Rules

The frontend must support internationalization.

Default language is:

```text
en-US
```

The app should be ready to add:

```text
pt-BR
es-ES
```

Do not hardcode user-facing text in components.

Use translation keys for:
- menu labels
- page titles
- field labels
- buttons
- filters
- empty states
- validation messages
- notification messages
- confirmation dialogs
- enum labels
- status labels

Keep translation keys stable.

Group keys by module and resource.

Example:

```text
resources.iam.users.fields.email
resources.iam.users.actions.create
resources.iam.roles.notifications.created
resources.iam.parameters.fields.overrideType
```

Shared text should live under shared keys.

Example:

```text
shared.actions.save
shared.actions.cancel
shared.status.active
shared.status.inactive
```

Do not use backend enum numeric values directly as labels.

Map enum values to translation keys.

Keep translation files close to app-level i18n setup.

Recommended folder:

```text
front/apps/src/app/i18n
```

If a component needs text, it should use the app translation function.

Do not concatenate translated strings unless there is no better option.

Prefer full sentence translation keys for messages.

---

## React Rules

Keep components small.

Do not put data fetching logic inside visual components if an API helper or resource hook can own it.

Avoid global state unless needed.

Use React and TanStack Table behavior before adding another state library.

Do not add Redux unless explicitly approved.

Avoid callback dependencies that confuse React Compiler memoization.

Derive primitive values outside callbacks when reading optional object properties.

Good:

```ts
const userLanguage = user?.language
```

Then use `userLanguage` inside `useMemo` or `useCallback`.

---

## Error Handling

Backend business errors arrive in the Result wrapper.

Error shape:

```json
{
  "data": null,
  "messages": [
    {
      "type": 2,
      "code": "InvalidEmailPasswordError",
      "text": "Invalid email or password.",
      "variables": []
    }
  ],
  "isSuccess": false,
  "title": "Invalid email or password."
}
```

Use backend `title` first for user notifications, `title` is the text of the first message.

If `title` is empty, use `messages[].text`.

Do not hardcode backend business error text in translation files.

Do not translate backend message codes in the frontend unless the backend explicitly changes to code-only messages.

Keep error translation in shared data/auth helpers.

Do not duplicate error parsing in pages.

Show simple messages.

Do not expose stack traces or raw JSON to users.

---

## Testing Rules

Ask before adding a test framework if none exists.

Prefer focused tests for:
- data provider unwrapping
- auth provider behavior
- permission helpers
- important custom components

Do not over-test shadcn/ui, Radix UI, or browser built-in behavior.

---

## Documentation Rules

When adding frontend architecture, update docs only if requested.

Keep docs simple enough for a backend developer learning the frontend.

Use current paths and names.

Do not write docs only for agents.

---

## Before Finishing

For frontend code changes:
- run tests if available
- run lint if available: `npm.cmd run lint`
- run TypeScript build if available: `npm.cmd run build`
- start dev server if the user asked to try the app

If a command cannot run, say why.

If Vite reports only a large chunk warning and the build succeeds, report it as a warning, not a failure.
