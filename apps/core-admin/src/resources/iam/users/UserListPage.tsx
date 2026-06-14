import { useForm } from '@tanstack/react-form'
import { Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import type { SortingState } from '@tanstack/react-table'

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
import { Select } from '../../../components/ui/select'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { DEFAULT_PAGINATION } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { createUserTableColumns } from './UserListPageColumns'
import { deleteUser, getUsers } from './userApi'
import type { PagedResultDto, UserLiteDto } from './userTypes'

export function UserListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [appliedOrganizationIdFilter, setAppliedOrganizationIdFilter] = useState('')
  const [appliedIsActiveFilter, setAppliedIsActiveFilter] = useState<string | null>(null)
  const [appliedNameFilter, setAppliedNameFilter] = useState('')
  const [appliedEmailFilter, setAppliedEmailFilter] = useState('')
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<UserLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<UserLiteDto | null>(null)
  const [sorting, setSorting] = useState<SortingState>([])
  const canCreate = hasPermissionCode(permissions, IAM_PERMISSIONS.users.write)
  const canView = hasPermissionCode(permissions, IAM_PERMISSIONS.users.read)
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.users.write)
  const canDelete = hasPermissionCode(permissions, IAM_PERMISSIONS.users.write)
  const columns = useMemo(() => createUserTableColumns({
    canDelete,
    canUpdate,
    canView,
    navigate,
    setDeleteTarget,
    t,
  }), [canDelete, canUpdate, canView, navigate, t])
  const filterForm = useForm({
    defaultValues: {
      email: '',
      isActive: 'all',
      name: '',
      organizationId: '',
    },
    onSubmit: ({ value }) => {
      setPageNumber(DEFAULT_PAGINATION.pageNumber)
      setAppliedOrganizationIdFilter(value.organizationId)
      setAppliedIsActiveFilter(value.isActive === 'all' ? null : value.isActive)
      setAppliedNameFilter(value.name)
      setAppliedEmailFilter(value.email)
    },
  })
  const loadUsers = useCallback(async (targetPage = pageNumber) => {
    setIsLoading(true)

    try {
      setResult(await getUsers({
        email: appliedEmailFilter,
        isActive: appliedIsActiveFilter,
        name: appliedNameFilter,
        organizationId: appliedOrganizationIdFilter,
        pageNumber: targetPage,
        pageSize,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedEmailFilter, appliedIsActiveFilter, appliedNameFilter, appliedOrganizationIdFilter, notifyError, pageNumber, pageSize, t])

  useEffect(() => {
    void loadUsers(pageNumber)
  }, [loadUsers, pageNumber])

  function handleReset() {
    filterForm.reset()
    setAppliedOrganizationIdFilter('')
    setAppliedIsActiveFilter(null)
    setAppliedNameFilter('')
    setAppliedEmailFilter('')
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handlePageSizeChange(nextPageSize: number) {
    setPageSize(nextPageSize)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  async function handleConfirmDelete() {
    if (deleteTarget === null) {
      return
    }

    try {
      await deleteUser(deleteTarget.id)
      showSuccess(t('features.iam.users.notifications.deleted'))
      setDeleteTarget(null)
      await loadUsers(pageNumber)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  const totalPages = result?.totalPages ?? 1

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.users.pages.list')}</h1>
        {canCreate && (
          <Button onClick={() => navigate(APP_ROUTES.userCreate)} type="button">
            <Plus data-icon="inline-start" />
            {t('shared.actions.create')}
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
              <FieldLabel>{t('resources.iam.organization')}</FieldLabel>
              <OrganizationSelect
                clearable
                onValueChange={field.handleChange}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>

        <filterForm.Field name="name">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.name')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="email">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.email')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
                <filterForm.Field name="isActive">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.isActive')}</FieldLabel>
              <Select
                onValueChange={field.handleChange}
                options={[
                  { label: t('shared.filters.all'), value: 'all' },
                  { label: t('shared.status.active'), value: 'true' },
                  { label: t('shared.status.inactive'), value: 'false' },
                ]}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
      </FilterToolbar>

      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('features.iam.users.messages.empty')}
        isLoading={isLoading}
        loadingText={t('shared.common.loading')}
        onSortingChange={setSorting}
        sorting={sorting}
      />

      <DataTablePagination
        onPageChange={setPageNumber}
        onPageSizeChange={handlePageSizeChange}
        pageNumber={result?.pageNumber ?? pageNumber}
        pageSize={pageSize}
        totalCount={result?.totalCount ?? 0}
        totalPages={totalPages}
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
        <p>{t('features.iam.users.messages.deleteConfirm')}</p>
      </ConfirmDialog>
    </main>
  )
}
