import type { SortingState } from '@tanstack/react-table'
import { Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { DataTable } from '../../../components/ui/data-table'
import { ConfirmDialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { hasPermissionCode } from '../../../shared/permissions'
import { UserSelect } from '../users/UserSelect'
import { deleteRole, getRoles } from './roleApi'
import { createRoleTableColumns } from './RoleListPageColumns'
import type { RoleDto, RoleSearchForm } from './roleTypes'

const EMPTY_ROLE_SEARCH: RoleSearchForm = {
  name: '',
  organizationId: '',
  userId: '',
}

export function RoleListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [roles, setRoles] = useState<RoleDto[]>([])
  const [deleteTarget, setDeleteTarget] = useState<RoleDto | null>(null)
  const [appliedFilters, setAppliedFilters] = useState<RoleSearchForm>(EMPTY_ROLE_SEARCH)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const canCreate = hasPermissionCode(permissions, IAM_PERMISSIONS.roles.write)
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.roles.write)
  const canDelete = hasPermissionCode(permissions, IAM_PERMISSIONS.roles.write)
  const { control, handleSubmit, register, reset } = useForm<RoleSearchForm>({
    defaultValues: EMPTY_ROLE_SEARCH,
  })
  const columns = useMemo(() => createRoleTableColumns({
    canDelete,
    canUpdate,
    onDelete: setDeleteTarget,
    onEdit: (role) => navigate(APP_ROUTES.roleEdit(role.id)),
    t,
  }), [canDelete, canUpdate, navigate, t])

  const loadRoles = useCallback(async () => {
    setIsLoading(true)

    try {
      setRoles(await getRoles(appliedFilters))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedFilters, notifyError, t])

  useEffect(() => {
    void loadRoles()
  }, [loadRoles])

  function handleCreate() {
    navigate(APP_ROUTES.roleCreate)
  }

  function handleReset() {
    reset(EMPTY_ROLE_SEARCH)
    setAppliedFilters(EMPTY_ROLE_SEARCH)
  }

  function handleSearch(value: RoleSearchForm) {
    setAppliedFilters({ ...value })
  }

  async function handleConfirmDelete() {
    if (deleteTarget === null) {
      return
    }

    try {
      await deleteRole(deleteTarget.id)
      showSuccess(t('features.iam.roles.notifications.deleted'))
      setDeleteTarget(null)
      await loadRoles()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.roles.pages.list')}</h1>
        {canCreate && (
          <Button onClick={handleCreate} type="button">
            <Plus data-icon="inline-start" />
            {t('shared.actions.create')}
          </Button>
        )}
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
        <Controller
          control={control}
          name="userId"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.user')}</FieldLabel>
              <UserSelect
                clearable
                onValueChange={field.onChange}
                value={field.value}
              />
            </Field>
          )}
        />
        <Field>
          <FieldLabel htmlFor="name">{t('shared.fields.name')}</FieldLabel>
          <Input id="name" {...register('name')} />
        </Field>
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={roles}
        emptyText={t('features.iam.roles.messages.empty')}
        isLoading={isLoading}
        loadingText={t('shared.common.loading')}
        onSortingChange={setSorting}
        sorting={sorting}
      />
      <ConfirmDialog
        cancelText={t('shared.actions.cancel')}
        backLabel={t('shared.actions.back')}
        confirmText={t('shared.actions.delete')}
        onConfirm={() => void handleConfirmDelete()}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        open={deleteTarget !== null}
        title={t('shared.actions.delete')}
      >
        <p>{t('features.iam.roles.messages.deleteConfirm')}</p>
      </ConfirmDialog>
    </main>
  )
}
