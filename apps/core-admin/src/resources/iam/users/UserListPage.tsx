import { Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import type { SortingState } from '@tanstack/react-table'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { DataTable } from '../../../components/ui/data-table'
import { ConfirmDialog } from '../../../components/ui/dialog'
import { Field } from '../../../components/ui/field'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
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
  const [organizationIdFilter, setOrganizationIdFilter] = useState('')
  const [nameFilter, setNameFilter] = useState('')
  const [emailFilter, setEmailFilter] = useState('')
  const [appliedOrganizationIdFilter, setAppliedOrganizationIdFilter] = useState('')
  const [appliedNameFilter, setAppliedNameFilter] = useState('')
  const [appliedEmailFilter, setAppliedEmailFilter] = useState('')
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<UserLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<UserLiteDto | null>(null)
  const [sorting, setSorting] = useState<SortingState>([])
  const canCreate = hasPermissionCode(permissions, IAM_PERMISSIONS.users.create)
  const canView = hasPermissionCode(permissions, IAM_PERMISSIONS.users.view)
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.users.update)
  const canDelete = hasPermissionCode(permissions, IAM_PERMISSIONS.users.delete)
  const columns = useMemo(() => createUserTableColumns({
    canDelete,
    canUpdate,
    canView,
    navigate,
    setDeleteTarget,
    t,
  }), [canDelete, canUpdate, canView, navigate, t])
  const loadUsers = useCallback(async (targetPage = pageNumber) => {
    setIsLoading(true)

    try {
      setResult(await getUsers({
        email: appliedEmailFilter,
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
  }, [appliedEmailFilter, appliedNameFilter, appliedOrganizationIdFilter, notifyError, pageNumber, pageSize, t])

  useEffect(() => {
    void loadUsers(pageNumber)
  }, [loadUsers, pageNumber])

  function handleFilter(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedOrganizationIdFilter(organizationIdFilter)
    setAppliedNameFilter(nameFilter)
    setAppliedEmailFilter(emailFilter)
  }

  function handleReset() {
    setOrganizationIdFilter('')
    setNameFilter('')
    setEmailFilter('')
    setAppliedOrganizationIdFilter('')
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
      showSuccess(t('resources.iam.users.notifications.deleted'))
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
        <h1 className="page-title">{t('resources.iam.users.pages.list')}</h1>
        {canCreate && (
          <Button onClick={() => navigate(APP_ROUTES.userCreate)} type="button">
            <Plus size={16} />
            {t('resources.iam.users.actions.create')}
          </Button>
        )}
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={handleFilter}>
        <Field label={t('resources.iam.users.fields.organizationId')}>
          <OrganizationSelect
            clearable
            onValueChange={setOrganizationIdFilter}
            value={organizationIdFilter}
          />
        </Field>
        <Field label={t('resources.iam.users.fields.name')}>
          <Input onChange={(event) => setNameFilter(event.currentTarget.value)} value={nameFilter} />
        </Field>
        <Field label={t('resources.iam.users.fields.email')}>
          <Input onChange={(event) => setEmailFilter(event.currentTarget.value)} type="email" value={emailFilter} />
        </Field>
      </FilterToolbar>

      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('resources.iam.users.messages.empty')}
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
        confirmText={t('resources.iam.users.actions.delete')}
        onConfirm={() => void handleConfirmDelete()}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        open={deleteTarget !== null}
        title={t('resources.iam.users.actions.delete')}
      >
        <p>{t('resources.iam.users.messages.deleteConfirm')}</p>
      </ConfirmDialog>
    </main>
  )
}
