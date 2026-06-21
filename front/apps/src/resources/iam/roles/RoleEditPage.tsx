import { zodResolver } from '@hookform/resolvers/zod'
import type { ColumnDef, RowSelectionState, SortingState } from '@tanstack/react-table'
import { ArrowLeft, Minus, Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Badge } from '../../../components/ui/badge'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { DataTable } from '../../../components/ui/data-table'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import type { PermissionDto } from '../../../shared/permissions'
import { hasPermissionCode } from '../../../shared/permissions'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import {
  PERMISSION_FILTER_VALUES,
  PERMISSION_MODULE_OPTIONS,
} from '../permissions/permissionTypes'
import { toTranslatedOptions } from '../permissions/permissionUi'
import {
  assignRolePermissions,
  createRole,
  getAvailableRolePermissions,
  getRole,
  getRolePermissions,
  unassignRolePermissions,
  updateRole,
} from './roleApi'
import type { RoleDto, RoleForm } from './roleTypes'

const EMPTY_ROLE_FORM: RoleForm = {
  description: '',
  isActive: true,
  isDefault: false,
  name: '',
  organizationId: '',
}

const DEFAULT_ROLE_PERMISSION_MODULE = PERMISSION_MODULE_OPTIONS[0]?.value ?? PERMISSION_FILTER_VALUES.all

const roleEditSchema = z.object({
  description: z.string().trim().min(1),
  isActive: z.boolean(),
  isDefault: z.boolean(),
  name: z.string().trim().min(1),
  organizationId: z.string(),
})

type RoleEditFormProps = {
  isCreate: boolean
  onCreated: (role: RoleDto) => void
  onSaved: () => Promise<void>
  role: RoleDto | null
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

function getSelectedIds(selection: RowSelectionState): string[] {
  return Object.entries(selection)
    .filter(([, selected]) => selected)
    .map(([id]) => id)
}

export function RoleEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const { id } = useParams()
  const [role, setRole] = useState<RoleDto | null>(null)
  const [availablePermissions, setAvailablePermissions] = useState<PermissionDto[]>([])
  const [rolePermissions, setRolePermissions] = useState<PermissionDto[]>([])
  const [availableSelection, setAvailableSelection] = useState<RowSelectionState>({})
  const [rolePermissionSelection, setRolePermissionSelection] = useState<RowSelectionState>({})
  const [availableSorting, setAvailableSorting] = useState<SortingState>([])
  const [rolePermissionSorting, setRolePermissionSorting] = useState<SortingState>([])
  const [permissionTitleFilter, setPermissionTitleFilter] = useState('')
  const [permissionModuleFilter, setPermissionModuleFilter] = useState<string>(DEFAULT_ROLE_PERMISSION_MODULE)
  const [isPermissionLoading, setIsPermissionLoading] = useState(false)
  const [isPermissionSaving, setIsPermissionSaving] = useState(false)
  const isCreate = id === undefined
  const canAssignPermissions = hasPermissionCode(permissions, IAM_PERMISSIONS.permissions.assign)
  const pageTitle = isCreate ? t('features.iam.roles.pages.create') : t('features.iam.roles.pages.edit')
  const permissionModuleOptions = useMemo(() => [
    { label: t('shared.filters.all'), value: PERMISSION_FILTER_VALUES.all },
    ...toTranslatedOptions(PERMISSION_MODULE_OPTIONS, t),
  ], [t])
  const filteredAvailablePermissions = useMemo(() => {
    const filter = permissionTitleFilter.trim().toLowerCase()

    if (filter.length === 0) {
      return availablePermissions
    }

    return availablePermissions.filter((permission) => permission.title.toLowerCase().includes(filter))
  }, [availablePermissions, permissionTitleFilter])
  const filteredRolePermissions = useMemo(() => {
    const filter = permissionTitleFilter.trim().toLowerCase()

    if (filter.length === 0) {
      return rolePermissions
    }

    return rolePermissions.filter((permission) => permission.title.toLowerCase().includes(filter))
  }, [permissionTitleFilter, rolePermissions])
  const availablePermissionColumns = useMemo<ColumnDef<PermissionDto>[]>(() => [
    {
      accessorKey: 'module',
      header: t('shared.fields.module'),
    },
    {
      accessorKey: 'resource',
      header: t('shared.fields.resource'),
    },
    {
      accessorKey: 'title',
      header: t('shared.fields.title'),
    },
    {
      cell: ({ row }) => (
        <span className="permission-info" title={row.original.description}>
          ?
        </span>
      ),
      enableSorting: false,
      header: t('shared.fields.info'),
      id: 'info',
    },
  ], [t])
  const assignedPermissionColumns = useMemo<ColumnDef<PermissionDto>[]>(() => [
    {
      accessorKey: 'module',
      header: t('shared.fields.module'),
    },
    {
      accessorKey: 'resource',
      header: t('shared.fields.resource'),
    },
    {
      accessorKey: 'title',
      header: t('shared.fields.title'),
    },
    {
      accessorKey: 'isActive',
      cell: ({ row }) => (
        <Badge variant={row.original.isActive ? 'active' : 'inactive'}>
          {row.original.isActive ? t('shared.status.active') : t('shared.status.inactive')}
        </Badge>
      ),
      header: t('shared.fields.isActive'),
    },
  ], [t])
  const loadRole = useCallback(async () => {
    if (isCreate) {
      return
    }

    try {
      const loaded = await getRole(id)
      setRole(loaded)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [id, isCreate, notifyError, t])

  const loadPermissions = useCallback(async () => {
    if (isCreate || id === undefined || !canAssignPermissions) {
      return
    }

    setIsPermissionLoading(true)

    try {
      const [available, assigned] = await Promise.all([
        getAvailableRolePermissions(id, permissionModuleFilter),
        getRolePermissions(id, permissionModuleFilter),
      ])

      setAvailablePermissions(available)
      setRolePermissions(assigned)
      setAvailableSelection({})
      setRolePermissionSelection({})
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsPermissionLoading(false)
    }
  }, [
    canAssignPermissions,
    id,
    isCreate,
    notifyError,
    permissionModuleFilter,
    t,
  ])

  useEffect(() => {
    void loadRole()
  }, [loadRole])

  useEffect(() => {
    setRole(null)
  }, [id])

  useEffect(() => {
    void loadPermissions()
  }, [loadPermissions])

  function handlePermissionModuleChange(value: string) {
    setAvailableSelection({})
    setRolePermissionSelection({})
    setPermissionModuleFilter(value)
  }

  async function handleAssignPermissions() {
    if (id === undefined || !canAssignPermissions) {
      return
    }

    const permissionIds = getSelectedIds(availableSelection)

    if (permissionIds.length === 0) {
      return
    }

    setIsPermissionSaving(true)

    try {
      await assignRolePermissions(id, permissionIds)
      showSuccess(t('features.iam.roles.notifications.permissionsAssigned'))
      await loadPermissions()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsPermissionSaving(false)
    }
  }

  async function handleUnassignPermissions() {
    if (id === undefined || !canAssignPermissions) {
      return
    }

    const permissionIds = getSelectedIds(rolePermissionSelection)

    if (permissionIds.length === 0) {
      return
    }

    setIsPermissionSaving(true)

    try {
      await unassignRolePermissions(id, permissionIds)
      showSuccess(t('features.iam.roles.notifications.permissionsUnassigned'))
      await loadPermissions()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsPermissionSaving(false)
    }
  }

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{pageTitle}</h1>
        <Button onClick={() => navigate(APP_ROUTES.roles)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {!isCreate && role === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <RoleEditForm
              key={role?.id ?? 'create'}
              isCreate={isCreate}
              onCreated={(created) => {
                setRole(created)
                navigate(APP_ROUTES.roleEdit(created.id), { replace: true })
              }}
              onSaved={loadRole}
              role={role}
            />
          )}
        </CardContent>
      </Card>
      {!isCreate && canAssignPermissions && (
        <Card>
          <CardContent className="permission-section">
            <h2 className="card-title">{t('features.iam.permissions.name')}</h2>
            <div className="permission-filter-form">
              <Field>
                <FieldLabel>{t('shared.fields.module')}</FieldLabel>
                <Select
                  onValueChange={handlePermissionModuleChange}
                  options={permissionModuleOptions}
                  value={permissionModuleFilter}
                />
              </Field>
              <Field>
                <FieldLabel htmlFor="permission-title-filter">{t('shared.fields.title')}</FieldLabel>
                <Input
                  id="permission-title-filter"
                  onChange={(event) => setPermissionTitleFilter(event.currentTarget.value)}
                  value={permissionTitleFilter}
                />
              </Field>
            </div>
            <div className="permission-assignment-grid">
              <div className="permission-table-column">
                <h2 className="card-title">{t('features.iam.roles.labels.availablePermissions')}</h2>
                <DataTable
                  columns={availablePermissionColumns}
                  data={filteredAvailablePermissions}
                  emptyText={t('features.iam.permissions.messages.empty')}
                  getRowId={(permission) => permission.id}
                  isLoading={isPermissionLoading}
                  loadingText={t('shared.common.loading')}
                  onRowSelectionChange={setAvailableSelection}
                  onSortingChange={setAvailableSorting}
                  rowSelection={availableSelection}
                  sorting={availableSorting}
                />
              </div>
              <div className="permission-action-column">
                <Button
                  disabled={isPermissionSaving || getSelectedIds(availableSelection).length === 0}
                  onClick={() => void handleAssignPermissions()}
                  type="button"
                >
                  <Plus data-icon="inline-start" />
                  {t('shared.actions.add')}
                </Button>
                <Button
                  disabled={isPermissionSaving || getSelectedIds(rolePermissionSelection).length === 0}
                  onClick={() => void handleUnassignPermissions()}
                  type="button"
                  variant="outline"
                >
                  <Minus data-icon="inline-start" />
                  {t('shared.actions.remove')}
                </Button>
              </div>
              <div className="permission-table-column">
                <h2 className="card-title">{t('features.iam.roles.labels.assignedPermissions')}</h2>
                <DataTable
                  columns={assignedPermissionColumns}
                  data={filteredRolePermissions}
                  emptyText={t('features.iam.permissions.messages.empty')}
                  getRowId={(permission) => permission.id}
                  isLoading={isPermissionLoading}
                  loadingText={t('shared.common.loading')}
                  onRowSelectionChange={setRolePermissionSelection}
                  onSortingChange={setRolePermissionSorting}
                  rowSelection={rolePermissionSelection}
                  sorting={rolePermissionSorting}
                />
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </main>
  )
}

function RoleEditForm({
  isCreate,
  onCreated,
  onSaved,
  role,
}: RoleEditFormProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const {
    control,
    handleSubmit,
    register,
  } = useForm<RoleForm>({
    defaultValues: role === null ? EMPTY_ROLE_FORM : toForm(role),
    resolver: zodResolver(roleEditSchema),
  })

  async function handleSave(value: RoleForm) {
    setIsSaving(true)

    try {
      if (isCreate) {
        const created = await createRole(value)
        showSuccess(t('features.iam.roles.notifications.created'))
        onCreated(created)
      } else if (role !== null) {
        await updateRole(role.id, value)
        showSuccess(t('features.iam.roles.notifications.updated'))
        await onSaved()
      }
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <form className="edit-form" onSubmit={handleSubmit(handleSave)}>
      <FieldGroup>
        <Controller
          control={control}
          name="organizationId"
          render={({ field }) => (
            <Field data-disabled={!isCreate}>
              <FieldLabel>{t('shared.fields.organization')}</FieldLabel>
              <OrganizationSelect
                clearable
                disabled={!isCreate}
                includeInactive
                onValueChange={field.onChange}
                value={field.value}
              />
            </Field>
          )}
        />
        <Field>
          <FieldLabel htmlFor="name">{t('shared.fields.name')}</FieldLabel>
          <Input id="name" required {...register('name')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="description">{t('shared.fields.description')}</FieldLabel>
          <Textarea id="description" required {...register('description')} />
        </Field>
        <Controller
          control={control}
          name="isActive"
          render={({ field }) => (
            <Checkbox
              checked={field.value}
              label={t('shared.fields.isActive')}
              onCheckedChange={field.onChange}
            />
          )}
        />
        <Controller
          control={control}
          name="isDefault"
          render={({ field }) => (
            <Checkbox
              checked={field.value}
              label={t('shared.fields.isDefault')}
              onCheckedChange={field.onChange}
            />
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">
          {isCreate ? t('shared.actions.create') : t('shared.actions.save')}
        </Button>
      </div>
    </form>
  )
}
