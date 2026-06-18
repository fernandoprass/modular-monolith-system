import { useForm } from '@tanstack/react-form'
import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { useTranslate, type Translate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { DataTable } from '../../../components/ui/data-table'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { DEFAULT_PAGINATION } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import { getParameters } from './parameterApi'
import { createParameterTableColumns } from './ParameterListPageColumns'
import {
  PARAMETER_FILTER_VALUES,
  PARAMETER_MODULE_OPTIONS,
  type PagedResultDto,
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
  const [appliedGroupFilter, setAppliedGroupFilter] = useState('')
  const [appliedModuleFilter, setAppliedModuleFilter] = useState<string>(PARAMETER_FILTER_VALUES.all)
  const [appliedNameFilter, setAppliedNameFilter] = useState('')
  const [appliedTitleFilter, setAppliedTitleFilter] = useState('')
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
      setAppliedGroupFilter(value.group)
      setAppliedModuleFilter(value.module)
      setAppliedNameFilter(value.name)
      setAppliedTitleFilter(value.title)
    },
  })
  const columns = useMemo(() => createParameterTableColumns({
    canUpdate,
    onEdit: (parameter) => navigate(APP_ROUTES.parameterEdit(parameter.id)),
    t,
  }), [canUpdate, navigate, t])

  const loadParameters = useCallback(async (targetPage = pageNumber) => {
    setIsLoading(true)

    try {
      setResult(await getParameters({
        group: appliedGroupFilter,
        module: appliedModuleFilter,
        name: appliedNameFilter,
        pageNumber: targetPage,
        pageSize,
        title: appliedTitleFilter,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedGroupFilter, appliedModuleFilter, appliedNameFilter, appliedTitleFilter, notifyError, pageNumber, pageSize, t])

  useEffect(() => {
    void loadParameters(pageNumber)
  }, [loadParameters, pageNumber])

  function handleReset() {
    filterForm.reset()
    setAppliedGroupFilter('')
    setAppliedModuleFilter(PARAMETER_FILTER_VALUES.all)
    setAppliedNameFilter('')
    setAppliedTitleFilter('')
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handlePageSizeChange(nextPageSize: number) {
    setPageSize(nextPageSize)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  const totalPages = result?.totalPages ?? 1

  return (
    <main className="page">
      <h1 className="page-title">{t('features.iam.parameters.pages.list')}</h1>
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
