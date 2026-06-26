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
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { ConfirmDialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import { createUserTableColumns } from './UserListPageColumns'
import { deleteUser, getUsers } from './userApi'
import type { UserLiteDto } from './userTypes'

const USER_FILTER_VALUES = {
  all: 'all',
} as const

type UserSearchForm = {
  email: string
  isActive: string
  name: string
}

const EMPTY_USER_SEARCH: UserSearchForm = {
  email: '',
  isActive: USER_FILTER_VALUES.all,
  name: '',
}

export function UserListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions, user } = useAuth()
  const organizationId = user?.organizationId ?? ''
  const [appliedFilters, setAppliedFilters] = useState<UserSearchForm>(EMPTY_USER_SEARCH)
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
  const { control, handleSubmit, register, reset } = useForm<UserSearchForm>({
    defaultValues: EMPTY_USER_SEARCH,
  })
  const columns = useMemo(() => createUserTableColumns({
    canDelete,
    canUpdate,
    canView,
    navigate,
    setDeleteTarget,
    t,
  }), [canDelete, canUpdate, canView, navigate, t])
  const loadUsers = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getUsers({
        email: appliedFilters.email,
        isActive: appliedFilters.isActive === USER_FILTER_VALUES.all ? null : appliedFilters.isActive,
        name: appliedFilters.name,
        organizationId,
        pageNumber: targetPage,
        pageSize,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedFilters, notifyError, organizationId, pageSize, t])

  useEffect(() => {
    void loadUsers(pageNumber)
  }, [loadUsers, pageNumber])

  function handleReset() {
    reset(EMPTY_USER_SEARCH)
    setAppliedFilters(EMPTY_USER_SEARCH)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handleSearch(value: UserSearchForm) {
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedFilters({ ...value })
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

  const totalPages = result?.totalPages ?? DEFAULT_PAGINATION.pageNumber

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
      <FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
        <Field>
          <FieldLabel htmlFor="name">{t('shared.fields.name')}</FieldLabel>
          <Input id="name" {...register('name')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="email">{t('shared.fields.email')}</FieldLabel>
          <Input id="email" {...register('email')} />
        </Field>
        <Controller
          control={control}
          name="isActive"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.isActive')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[
                  { label: t('shared.filters.all'), value: USER_FILTER_VALUES.all },
                  { label: t('shared.status.active'), value: 'true' },
                  { label: t('shared.status.inactive'), value: 'false' },
                ]}
                value={field.value}
              />
            </Field>
          )}
        />
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
