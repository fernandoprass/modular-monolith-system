import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import type { SortingState } from '@tanstack/react-table'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { DataTable } from '../../../components/ui/data-table'
import { ConfirmDialog } from '../../../components/ui/dialog'
import { Field } from '../../../components/ui/field'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { DEFAULT_PAGINATION } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import { deleteOrganization, getOrganizations } from './organizationApi'
import { createOrganizationTableColumns } from './OrganizationListPageColumns'
import { ORGANIZATION_TYPE_OPTIONS, type OrganizationDto, type PagedResultDto } from './organizationTypes'
import { toTranslatedOptions } from './organizationUi'

export function OrganizationListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [codeFilter, setCodeFilter] = useState('')
  const [nameFilter, setNameFilter] = useState('')
  const [typeFilter, setTypeFilter] = useState<string | null>(null)
  const [appliedCodeFilter, setAppliedCodeFilter] = useState('')
  const [appliedNameFilter, setAppliedNameFilter] = useState('')
  const [appliedTypeFilter, setAppliedTypeFilter] = useState<string | null>(null)
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<OrganizationDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<OrganizationDto | null>(null)
  const [sorting, setSorting] = useState<SortingState>([])
  const canView = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.view)
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.update)
  const canDelete = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.delete)
  const columns = useMemo(() => createOrganizationTableColumns({
    canDelete,
    canUpdate,
    canView,
    navigate,
    setDeleteTarget,
    t,
  }), [canDelete, canUpdate, canView, navigate, t])
  const loadOrganizations = useCallback(async (targetPage = pageNumber) => {
    setIsLoading(true)

    try {
      setResult(await getOrganizations({
        code: appliedCodeFilter,
        name: appliedNameFilter,
        pageNumber: targetPage,
        pageSize,
        type: appliedTypeFilter,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedCodeFilter, appliedNameFilter, appliedTypeFilter, notifyError, pageNumber, pageSize, t])

  useEffect(() => {
    void loadOrganizations(pageNumber)
  }, [loadOrganizations, pageNumber])

  function handleFilter(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedCodeFilter(codeFilter)
    setAppliedNameFilter(nameFilter)
    setAppliedTypeFilter(typeFilter)
  }

  function handleReset() {
    setCodeFilter('')
    setNameFilter('')
    setTypeFilter(null)
    setAppliedCodeFilter('')
    setAppliedNameFilter('')
    setAppliedTypeFilter(null)
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
      await deleteOrganization(deleteTarget.id)
      showSuccess(t('resources.iam.organizations.notifications.deleted'))
      setDeleteTarget(null)
      await loadOrganizations(pageNumber)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  const totalPages = result?.totalPages ?? 1

  return (
    <main className="page">
      <h1 className="page-title">{t('resources.iam.organizations.pages.list')}</h1>
      <FilterToolbar onReset={handleReset} onSubmit={handleFilter}>
        <Field label={t('resources.iam.organizations.fields.type')}>
          <Select
            onValueChange={(value) => setTypeFilter(value === 'all' ? null : value)}
            options={[{ label: t('shared.filters.all'), value: 'all' }, ...toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)]}
            value={typeFilter ?? 'all'}
          />
        </Field>
        <Field label={t('resources.iam.organizations.fields.code')}>
          <Input onChange={(event) => setCodeFilter(event.currentTarget.value)} value={codeFilter} />
        </Field>
        <Field label={t('resources.iam.organizations.fields.name')}>
          <Input onChange={(event) => setNameFilter(event.currentTarget.value)} value={nameFilter} />
        </Field>
      </FilterToolbar>

      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('resources.iam.organizations.messages.empty')}
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
        confirmText={t('resources.iam.organizations.actions.delete')}
        onConfirm={() => void handleConfirmDelete()}
        onOpenChange={(open) => !open && setDeleteTarget(null)}
        open={deleteTarget !== null}
        title={t('resources.iam.organizations.actions.delete')}
      >
        <p>{t('resources.iam.organizations.messages.deleteConfirm')}</p>
      </ConfirmDialog>
    </main>
  )
}
