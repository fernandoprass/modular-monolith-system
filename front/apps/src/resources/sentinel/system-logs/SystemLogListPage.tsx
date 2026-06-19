import { useForm } from '@tanstack/react-form'
import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { useTranslate, type Translate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { DataTable } from '../../../components/ui/data-table'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import { OrganizationSelect } from '../../iam/organizations/OrganizationSelect'
import { UserSelect } from '../../iam/users/UserSelect'
import { getSystemLogs } from './systemLogApi'
import { createSystemLogTableColumns } from './SystemLogListPageColumns'
import {
  SYSTEM_LOG_FILTER_VALUES,
  SYSTEM_LOG_LEVEL_OPTIONS,
  SYSTEM_LOG_STATUS_OPTIONS,
  type SystemLogLiteDto,
  type SystemLogSearchForm,
} from './systemLogTypes'

type SystemLogOption = {
  labelKey: string
  value: string
}

function toDateTimeLocalValue(date: Date): string {
  const offsetMs = date.getTimezoneOffset() * 60_000

  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}

function getDefaultSystemLogSearch(): SystemLogSearchForm {
  const now = new Date()
  const yesterday = new Date(now)

  yesterday.setDate(yesterday.getDate() - 1)
  yesterday.setHours(0, 0, 0, 0)

  return {
    from: toDateTimeLocalValue(yesterday),
    level: SYSTEM_LOG_FILTER_VALUES.all,
    organizationId: '',
    requestId: '',
    status: SYSTEM_LOG_FILTER_VALUES.all,
    to: toDateTimeLocalValue(now),
    userId: '',
  }
}

function toTranslatedOptions(options: readonly SystemLogOption[], t: Translate) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}

export function SystemLogListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const [appliedFilters, setAppliedFilters] = useState<SystemLogSearchForm>(() => getDefaultSystemLogSearch())
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<SystemLogLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const filterForm = useForm({
    defaultValues: appliedFilters,
    onSubmit: ({ value }) => {
      setPageNumber(DEFAULT_PAGINATION.pageNumber)
      setAppliedFilters({ ...value })
    },
  })
  const columns = useMemo(() => createSystemLogTableColumns({
    onView: (systemLog) => navigate(APP_ROUTES.systemLogView(systemLog.id)),
    t,
  }), [navigate, t])

  const loadSystemLogs = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getSystemLogs({
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
    void loadSystemLogs(pageNumber)
  }, [loadSystemLogs, pageNumber])

  function handleReset() {
    const emptySearch = getDefaultSystemLogSearch()

    filterForm.reset(emptySearch)
    setAppliedFilters(emptySearch)
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
        <h1 className="page-title">{t('features.sentinel.systemLogs.pages.list')}</h1>
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={(event) => {
        event.preventDefault()
        void filterForm.handleSubmit()
      }}>
        <filterForm.Field name="organizationId">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.organization')}</FieldLabel>
              <OrganizationSelect
                clearable
                includeInactive
                onValueChange={field.handleChange}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="userId">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.user')}</FieldLabel>
              <UserSelect
                clearable
                includeInactive
                onValueChange={field.handleChange}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="level">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.level')}</FieldLabel>
              <Select
                onValueChange={field.handleChange}
                options={[{ label: t('shared.filters.all'), value: SYSTEM_LOG_FILTER_VALUES.all }, ...toTranslatedOptions(SYSTEM_LOG_LEVEL_OPTIONS, t)]}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="status">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.status')}</FieldLabel>
              <Select
                onValueChange={field.handleChange}
                options={[{ label: t('shared.filters.all'), value: SYSTEM_LOG_FILTER_VALUES.all }, ...toTranslatedOptions(SYSTEM_LOG_STATUS_OPTIONS, t)]}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="requestId">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.requestId')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="from">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.from')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="datetime-local" value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="to">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.to')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="datetime-local" value={field.state.value} />
            </Field>
          )}
        </filterForm.Field>
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('features.sentinel.systemLogs.messages.empty')}
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
