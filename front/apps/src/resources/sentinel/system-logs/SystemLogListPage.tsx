import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
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
  const { control, handleSubmit, register, reset } = useForm<SystemLogSearchForm>({
    defaultValues: appliedFilters,
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

    reset(emptySearch)
    setAppliedFilters(emptySearch)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handleSearch(value: SystemLogSearchForm) {
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedFilters({ ...value })
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
      <FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
        <Controller
          control={control}
          name="organizationId"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.organization')}</FieldLabel>
              <OrganizationSelect
                clearable
                includeInactive
                onValueChange={field.onChange}
                value={field.value}
              />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="userId"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.user')}</FieldLabel>
              <UserSelect
                clearable
                includeInactive
                onValueChange={field.onChange}
                value={field.value}
              />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="level"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.level')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[{ label: t('shared.filters.all'), value: SYSTEM_LOG_FILTER_VALUES.all }, ...toTranslatedOptions(SYSTEM_LOG_LEVEL_OPTIONS, t)]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="status"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.status')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[{ label: t('shared.filters.all'), value: SYSTEM_LOG_FILTER_VALUES.all }, ...toTranslatedOptions(SYSTEM_LOG_STATUS_OPTIONS, t)]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Field>
          <FieldLabel htmlFor="requestId">{t('shared.fields.requestId')}</FieldLabel>
          <Input id="requestId" {...register('requestId')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="from">{t('shared.fields.from')}</FieldLabel>
          <Input id="from" required type="datetime-local" {...register('from')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="to">{t('shared.fields.to')}</FieldLabel>
          <Input id="to" required type="datetime-local" {...register('to')} />
        </Field>
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
