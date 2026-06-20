import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'

import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { DataTable } from '../../../components/ui/data-table'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import type { PermissionDto } from '../../../shared/permissions'
import { hasPermissionCode } from '../../../shared/permissions'
import { getPermissions } from './permissionApi'
import { PermissionEditDialog } from './PermissionEditDialog'
import { createPermissionTableColumns } from './PermissionListPageColumns'
import {
  PERMISSION_FILTER_VALUES,
  PERMISSION_MODULE_OPTIONS,
  PERMISSION_RESOURCE_OPTIONS,
  type PermissionSearchForm,
} from './permissionTypes'
import { toTranslatedOptions } from './permissionUi'

const EMPTY_PERMISSION_SEARCH: PermissionSearchForm = {
  action: '',
  isActive: PERMISSION_FILTER_VALUES.all,
  module: PERMISSION_FILTER_VALUES.all,
  resource: PERMISSION_FILTER_VALUES.all,
  title: '',
}

export function PermissionListPage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { permissions: userPermissions } = useAuth()
  const [result, setResult] = useState<PagedResultDto<PermissionDto> | null>(null)
  const [selectedPermission, setSelectedPermission] = useState<PermissionDto | null>(null)
  const [appliedFilters, setAppliedFilters] = useState<PermissionSearchForm>(EMPTY_PERMISSION_SEARCH)
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const canUpdate = hasPermissionCode(userPermissions, IAM_PERMISSIONS.permissions.write)
  const { control, handleSubmit, register, reset } = useForm<PermissionSearchForm>({
    defaultValues: EMPTY_PERMISSION_SEARCH,
  })
  const columns = useMemo(() => createPermissionTableColumns({
    canUpdate,
    onEdit: setSelectedPermission,
    t,
  }), [canUpdate, t])

  const loadPermissions = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getPermissions({
        ...appliedFilters,
        pageNumber: targetPage,
        pageSize,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedFilters, notifyError, pageSize, t])

  useEffect(() => {
    void loadPermissions(pageNumber)
  }, [loadPermissions, pageNumber])

  function handleReset() {
    reset(EMPTY_PERMISSION_SEARCH)
    setAppliedFilters(EMPTY_PERMISSION_SEARCH)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handleSearch(value: PermissionSearchForm) {
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedFilters({ ...value })
  }

  function handlePageSizeChange(nextPageSize: number) {
    setPageSize(nextPageSize)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  async function handleSaved() {
    await loadPermissions(pageNumber)
  }

  const totalPages = result?.totalPages ?? DEFAULT_PAGINATION.pageNumber

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.permissions.pages.list')}</h1>
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
        <Controller
          control={control}
          name="module"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.module')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[{ label: t('shared.filters.all'), value: PERMISSION_FILTER_VALUES.all }, ...toTranslatedOptions(PERMISSION_MODULE_OPTIONS, t)]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="resource"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.resource')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[{ label: t('shared.filters.all'), value: PERMISSION_FILTER_VALUES.all }, ...toTranslatedOptions(PERMISSION_RESOURCE_OPTIONS, t)]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Field>
          <FieldLabel htmlFor="action">{t('shared.fields.action')}</FieldLabel>
          <Input id="action" {...register('action')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="title">{t('shared.fields.title')}</FieldLabel>
          <Input id="title" {...register('title')} />
        </Field>
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('features.iam.permissions.messages.empty')}
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
      {selectedPermission !== null && (
        <PermissionEditDialog
          isOpen={selectedPermission !== null}
          onClose={() => setSelectedPermission(null)}
          onSaved={handleSaved}
          permission={selectedPermission}
        />
      )}
    </main>
  )
}
