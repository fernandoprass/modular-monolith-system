import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
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
import { deleteOrganization, getOrganizations } from './organizationApi'
import { createOrganizationTableColumns } from './OrganizationListPageColumns'
import { ORGANIZATION_TYPE_OPTIONS, type OrganizationDto } from './organizationTypes'
import { toTranslatedOptions } from './organizationUi'

const ORGANIZATION_FILTER_VALUES = {
  all: 'all',
} as const

type OrganizationSearchForm = {
  code: string
  isActive: string
  name: string
  type: string
}

const EMPTY_ORGANIZATION_SEARCH: OrganizationSearchForm = {
  code: '',
  isActive: ORGANIZATION_FILTER_VALUES.all,
  name: '',
  type: ORGANIZATION_FILTER_VALUES.all,
}

export function OrganizationListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [appliedFilters, setAppliedFilters] = useState<OrganizationSearchForm>(EMPTY_ORGANIZATION_SEARCH)
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<OrganizationDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<OrganizationDto | null>(null)
  const [sorting, setSorting] = useState<SortingState>([])
  const canView = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.read)
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.write)
  const canDelete = hasPermissionCode(permissions, IAM_PERMISSIONS.organizations.write)
  const { control, handleSubmit, register, reset } = useForm<OrganizationSearchForm>({
    defaultValues: EMPTY_ORGANIZATION_SEARCH,
  })
  const columns = useMemo(() => createOrganizationTableColumns({
    canDelete,
    canUpdate,
    canView,
    navigate,
    setDeleteTarget,
    t,
  }), [canDelete, canUpdate, canView, navigate, t])
  const loadOrganizations = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getOrganizations({
        code: appliedFilters.code,
        isActive: appliedFilters.isActive === ORGANIZATION_FILTER_VALUES.all ? null : appliedFilters.isActive,
        name: appliedFilters.name,
        pageNumber: targetPage,
        pageSize,
        type: appliedFilters.type === ORGANIZATION_FILTER_VALUES.all ? null : appliedFilters.type,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedFilters, notifyError, pageSize, t])

  useEffect(() => {
    void loadOrganizations(pageNumber)
  }, [loadOrganizations, pageNumber])

  function handleReset() {
    reset(EMPTY_ORGANIZATION_SEARCH)
    setAppliedFilters(EMPTY_ORGANIZATION_SEARCH)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handleSearch(value: OrganizationSearchForm) {
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
      await deleteOrganization(deleteTarget.id)
      showSuccess(t('features.iam.organizations.notifications.deleted'))
      setDeleteTarget(null)
      await loadOrganizations(pageNumber)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  const totalPages = result?.totalPages ?? DEFAULT_PAGINATION.pageNumber

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.organizations.pages.list')}</h1>
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
        <Controller
          control={control}
          name="type"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.type')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[{ label: t('shared.filters.all'), value: ORGANIZATION_FILTER_VALUES.all }, ...toTranslatedOptions(ORGANIZATION_TYPE_OPTIONS, t)]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Field>
          <FieldLabel htmlFor="code">{t('shared.fields.code')}</FieldLabel>
          <Input id="code" {...register('code')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="name">{t('shared.fields.name')}</FieldLabel>
          <Input id="name" {...register('name')} />
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
                  { label: t('shared.filters.all'), value: ORGANIZATION_FILTER_VALUES.all },
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
        emptyText={t('features.iam.organizations.messages.empty')}
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
        <p>{t('features.iam.organizations.messages.deleteConfirm')}</p>
      </ConfirmDialog>
    </main>
  )
}
