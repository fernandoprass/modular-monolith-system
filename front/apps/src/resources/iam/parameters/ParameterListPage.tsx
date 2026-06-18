import { useForm } from '@tanstack/react-form'
import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { useTranslate, type Translate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { DataTable } from '../../../components/ui/data-table'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import { getParameters } from './parameterApi'
import { createParameterTableColumns } from './ParameterListPageColumns'
import {
  PARAMETER_FILTER_VALUES,
  PARAMETER_MODULE_OPTIONS,
  type ParameterLiteDto,
  type ParameterSearchForm,
} from './parameterTypes'

const EMPTY_PARAMETER_SEARCH: ParameterSearchForm = {
  group: '',
  module: PARAMETER_FILTER_VALUES.all,
  name: '',
  title: '',
}

type ParameterOption = {
  labelKey: string
  value: string
}

function toTranslatedOptions(options: readonly ParameterOption[], t: Translate) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}

export function ParameterListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { permissions } = useAuth()
  const [appliedFilters, setAppliedFilters] = useState<ParameterSearchForm>(EMPTY_PARAMETER_SEARCH)
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<ParameterLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const canUpdate = hasPermissionCode(permissions, IAM_PERMISSIONS.parameters.write)
  const filterForm = useForm({
    defaultValues: EMPTY_PARAMETER_SEARCH,
    onSubmit: ({ value }) => {
      setPageNumber(DEFAULT_PAGINATION.pageNumber)
      setAppliedFilters({ ...value })
    },
  })
  const columns = useMemo(() => createParameterTableColumns({
    canUpdate,
    onEdit: (parameter) => navigate(APP_ROUTES.parameterEdit(parameter.id)),
    t,
  }), [canUpdate, navigate, t])

  const loadParameters = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getParameters({
        group: appliedFilters.group,
        module: appliedFilters.module,
        name: appliedFilters.name,
        pageNumber: targetPage,
        pageSize,
        title: appliedFilters.title,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedFilters, notifyError, pageSize, t])

  useEffect(() => {
    void loadParameters(pageNumber)
  }, [loadParameters, pageNumber])

  function handleReset() {
    filterForm.reset()
    setAppliedFilters(EMPTY_PARAMETER_SEARCH)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handlePageSizeChange(nextPageSize: number) {
    setPageSize(nextPageSize)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  const totalPages = result?.totalPages ?? DEFAULT_PAGINATION.pageNumber

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.parameters.pages.list')}</h1>
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={(event) => {
        event.preventDefault()
        void filterForm.handleSubmit()
      }}>
        <filterForm.Field name="module">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.module')}</FieldLabel>
              <Select
                onValueChange={field.handleChange}
                options={[{ label: t('shared.filters.all'), value: PARAMETER_FILTER_VALUES.all }, ...toTranslatedOptions(PARAMETER_MODULE_OPTIONS, t)]}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="group">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.group')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
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
        <filterForm.Field name="title">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.title')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('features.iam.parameters.messages.empty')}
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
    </main>
  )
}
