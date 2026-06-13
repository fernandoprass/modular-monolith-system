import { useForm } from '@tanstack/react-form'
import type { SortingState } from '@tanstack/react-table'
import { Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { DataTable } from '../../../components/ui/data-table'
import { ConfirmDialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { hasPermissionCode } from '../../../shared/permissions'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { UserSelect } from '../users/UserSelect'
import { deleteRole, getRoles } from './roleApi'
import { RoleEditDialog } from './RoleEditDialog'
import { createRoleTableColumns } from './RoleListPageColumns'
import type { RoleDto, RoleSearchForm } from './roleTypes'

const EMPTY_ROLE_SEARCH: RoleSearchForm = {
  name: '',
  organizationId: '',
  userId: '',
}

export function RoleListPage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [roles, setRoles] = useState<RoleDto[]>([])
  const [selectedRole, setSelectedRole] = useState<RoleDto | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<RoleDto | null>(null)
  const [isDialogOpen, setIsDialogOpen] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [organizationIdFilter, setOrganizationIdFilter] = useState('')
  const [sorting, setSorting] = useState<SortingState>([])
  const canCreate = hasPermissionCode(permissions, IAM_PERMISSIONS.roles.write)
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.roles.write)
  const canDelete = hasPermissionCode(permissions, IAM_PERMISSIONS.roles.write)
  const filterForm = useForm({
    defaultValues: EMPTY_ROLE_SEARCH,
    onSubmit: async ({ value }) => {
      await loadRoles(value)
    },
  })
  const columns = useMemo(() => createRoleTableColumns({
    canDelete,
    canUpdate,
    onDelete: setDeleteTarget,
    onEdit: (role) => {
      setSelectedRole(role)
      setIsDialogOpen(true)
    },
    t,
  }), [canDelete, canUpdate, t])

  const loadRoles = useCallback(async (request: RoleSearchForm = filterForm.state.values) => {
    setIsLoading(true)

    try {
      setRoles(await getRoles(request))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [filterForm.state.values, notifyError, t])

  useEffect(() => {
    void loadRoles()
  }, [loadRoles])

  function handleCreate() {
    setSelectedRole(null)
    setIsDialogOpen(true)
  }

  function handleReset() {
    filterForm.reset()
    setOrganizationIdFilter('')
    void loadRoles(EMPTY_ROLE_SEARCH)
  }

  async function handleSaved() {
    await loadRoles()
  }

  async function handleConfirmDelete() {
    if (deleteTarget === null) {
      return
    }

    try {
      await deleteRole(deleteTarget.id)
      showSuccess(t('resources.iam.roles.notifications.deleted'))
      setDeleteTarget(null)
      await loadRoles()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('resources.iam.roles.pages.list')}</h1>
        {canCreate && (
          <Button onClick={handleCreate} type="button">
            <Plus data-icon="inline-start" />
            {t('resources.iam.roles.actions.create')}
          </Button>
        )}
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={(event) => {
        event.preventDefault()
        void filterForm.handleSubmit()
      }}>
        <filterForm.Field name="organizationId">
          {(field) => (
            <Field>
              <FieldLabel>{t('resources.iam.roles.fields.organizationId')}</FieldLabel>
              <OrganizationSelect
                clearable
                onValueChange={(value) => {
                  setOrganizationIdFilter(value)
                  field.handleChange(value)
                  filterForm.setFieldValue('userId', '')
                }}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="userId">
          {(field) => (
            <Field>
              <FieldLabel>{t('resources.iam.roles.fields.userId')}</FieldLabel>
              <UserSelect
                clearable
                key={organizationIdFilter}
                onValueChange={field.handleChange}
                organizationId={organizationIdFilter}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="name">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('resources.iam.roles.fields.name')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={roles}
        emptyText={t('resources.iam.roles.messages.empty')}
        isLoading={isLoading}
        loadingText={t('shared.common.loading')}
        onSortingChange={setSorting}
        sorting={sorting}
      />
      {isDialogOpen && (
        <RoleEditDialog
          isOpen={isDialogOpen}
          onClose={() => setIsDialogOpen(false)}
          onSaved={handleSaved}
          role={selectedRole}
        />
      )}
      <ConfirmDialog
        cancelText={t('shared.actions.cancel')}
        backLabel={t('shared.actions.back')}
        confirmText={t('resources.iam.roles.actions.delete')}
        onConfirm={() => void handleConfirmDelete()}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        open={deleteTarget !== null}
        title={t('resources.iam.roles.actions.delete')}
      >
        <p>{t('resources.iam.roles.messages.deleteConfirm')}</p>
      </ConfirmDialog>
    </main>
  )
}
