import { useForm } from '@tanstack/react-form'
import type { RowSelectionState, SortingState, ColumnDef } from '@tanstack/react-table'
import { ArrowLeft, Minus, Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Badge } from '../../../components/ui/badge'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { DataTable } from '../../../components/ui/data-table'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Textarea } from '../../../components/ui/textarea'
import type { PermissionDto } from '../../../shared/permissions'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
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
  const { id } = useParams()
  const [role, setRole] = useState<RoleDto | null>(null)
  const [availablePermissions, setAvailablePermissions] = useState<PermissionDto[]>([])
  const [rolePermissions, setRolePermissions] = useState<PermissionDto[]>([])
  const [availableSelection, setAvailableSelection] = useState<RowSelectionState>({})
  const [rolePermissionSelection, setRolePermissionSelection] = useState<RowSelectionState>({})
  const [availableSorting, setAvailableSorting] = useState<SortingState>([])
  const [rolePermissionSorting, setRolePermissionSorting] = useState<SortingState>([])
  const [availablePermissionTitleFilter, setAvailablePermissionTitleFilter] = useState('')
  const [rolePermissionTitleFilter, setRolePermissionTitleFilter] = useState('')
  const [isPermissionLoading, setIsPermissionLoading] = useState(false)
  const [isPermissionSaving, setIsPermissionSaving] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const isCreate = id === undefined
  const filteredAvailablePermissions = useMemo(() => {
    const filter = availablePermissionTitleFilter.trim().toLowerCase()

    if (filter.length === 0) {
      return availablePermissions
    }

    return availablePermissions.filter((permission) => permission.title.toLowerCase().includes(filter))
  }, [availablePermissionTitleFilter, availablePermissions])
  const filteredRolePermissions = useMemo(() => {
    const filter = rolePermissionTitleFilter.trim().toLowerCase()

    if (filter.length === 0) {
      return rolePermissions
    }

    return rolePermissions.filter((permission) => permission.title.toLowerCase().includes(filter))
  }, [rolePermissionTitleFilter, rolePermissions])
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
  const form = useForm({
    defaultValues: EMPTY_ROLE_FORM,
    onSubmit: async ({ value }) => {
      setIsSaving(true)

      try {
        if (isCreate) {
          const created = await createRole(value)
          setRole(created)
          showSuccess(t('features.iam.roles.notifications.created'))
          navigate(APP_ROUTES.roleEdit(created.id), { replace: true })
        } else {
          await updateRole(id, value)
          showSuccess(t('features.iam.roles.notifications.updated'))
          await loadRole()
        }
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSaving(false)
      }
    },
  })

  const loadRole = useCallback(async () => {
    if (isCreate) {
      return
    }

    try {
      const loaded = await getRole(id)
      setRole(loaded)
      form.setFieldValue('description', loaded.description)
      form.setFieldValue('isActive', loaded.isActive)
      form.setFieldValue('isDefault', loaded.isDefault)
      form.setFieldValue('name', loaded.name)
      form.setFieldValue('organizationId', loaded.organizationId ?? '')
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [form, id, isCreate, notifyError, t])

  const loadPermissions = useCallback(async () => {
    if (isCreate || id === undefined) {
      return
    }

    setIsPermissionLoading(true)

    try {
      const [available, assigned] = await Promise.all([
        getAvailableRolePermissions(id),
        getRolePermissions(id),
      ])

      setAvailablePermissions(available)
      setRolePermissions(assigned)
      setAvailableSelection({})
      setRolePermissionSelection({})
      setAvailablePermissionTitleFilter('')
      setRolePermissionTitleFilter('')
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsPermissionLoading(false)
    }
  }, [id, isCreate, notifyError, t])

  useEffect(() => {
    void loadRole()
  }, [loadRole])

  useEffect(() => {
    void loadPermissions()
  }, [loadPermissions])

  useEffect(() => {
    if (isCreate) {
      form.reset(EMPTY_ROLE_FORM)
      return
    }

  }, [form, isCreate])

  async function handleAssignPermissions() {
    if (id === undefined) {
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
    if (id === undefined) {
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
        <h1 className="page-title">{isCreate ? t('shared.actions.create') : t('shared.actions.edit')}</h1>
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
            <form className="edit-form" onSubmit={(event) => {
              event.preventDefault()
              void form.handleSubmit()
            }}>
              <FieldGroup>
                <form.Field name="organizationId">
                  {(field) => (
                    <Field data-disabled={!isCreate}>
                      <FieldLabel>{t('shared.fields.organization')}</FieldLabel>
                      <OrganizationSelect
                        clearable
                        disabled={!isCreate}
                        includeInactive
                        onValueChange={field.handleChange}
                        value={field.state.value}
                      />
                    </Field>
                  )}
                </form.Field>
                <form.Field name="name">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('shared.fields.name')}</FieldLabel>
                      <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                    </Field>
                  )}
                </form.Field>
                <form.Field name="description">
                  {(field) => (
                    <Field>
                      <FieldLabel htmlFor={field.name}>{t('shared.fields.description')}</FieldLabel>
                      <Textarea id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                    </Field>
                  )}
                </form.Field>
                <form.Field name="isActive">
                  {(field) => (
                    <Checkbox
                      checked={field.state.value}
                      label={t('shared.fields.isActive')}
                      onCheckedChange={(checked) => field.handleChange(checked === true)}
                    />
                  )}
                </form.Field>
                <form.Field name="isDefault">
                  {(field) => (
                    <Checkbox
                      checked={field.state.value}
                      label={t('shared.fields.isDefault')}
                      onCheckedChange={(checked) => field.handleChange(checked === true)}
                    />
                  )}
                </form.Field>
              </FieldGroup>
              <div className="form-actions">
                <Button disabled={isSaving} type="submit">
                  {isCreate ? t('shared.actions.create') : t('shared.actions.save')}
                </Button>
              </div>
            </form>
          )}
        </CardContent>
      </Card>
      {!isCreate && (
        <Card>
          <CardContent>
            <div className="permission-assignment-grid">
              <div className="permission-table-column">
                <h2 className="card-title">{t('features.iam.roles.labels.availablePermissions')}</h2>
                <Field>
                  <FieldLabel htmlFor="available-permission-title-filter">{t('shared.fields.title')}</FieldLabel>
                  <Input
                    id="available-permission-title-filter"
                    onChange={(event) => setAvailablePermissionTitleFilter(event.currentTarget.value)}
                    value={availablePermissionTitleFilter}
                  />
                </Field>
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
                  Add
                </Button>
                <Button
                  disabled={isPermissionSaving || getSelectedIds(rolePermissionSelection).length === 0}
                  onClick={() => void handleUnassignPermissions()}
                  type="button"
                  variant="outline"
                >
                  <Minus data-icon="inline-start" />
                  Remove
                </Button>
              </div>
              <div className="permission-table-column">
                <h2 className="card-title">{t('features.iam.roles.labels.assignedPermissions')}</h2>
                <Field>
                  <FieldLabel htmlFor="assigned-permission-title-filter">{t('shared.fields.title')}</FieldLabel>
                  <Input
                    id="assigned-permission-title-filter"
                    onChange={(event) => setRolePermissionTitleFilter(event.currentTarget.value)}
                    value={rolePermissionTitleFilter}
                  />
                </Field>
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
