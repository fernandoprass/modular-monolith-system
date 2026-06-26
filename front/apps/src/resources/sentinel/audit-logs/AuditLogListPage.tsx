import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
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
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import { UserSelect } from '../../iam/users/UserSelect'
import { getAuditLogs } from './auditLogApi'
import { createAuditLogTableColumns } from './AuditLogListPageColumns'
import {
  AUDIT_LOG_FEATURE_OPTIONS,
  AUDIT_LOG_FILTER_VALUES,
  AUDIT_LOG_MODULE_OPTIONS,
  type AuditLogLiteDto,
  type AuditLogSearchForm,
} from './auditLogTypes'

type AuditLogOption = {
  labelKey: string
  value: string
}

function toDateTimeLocalValue(date: Date): string {
  const offsetMs = date.getTimezoneOffset() * 60_000

  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}

function getDefaultAuditLogSearch(): AuditLogSearchForm {
  const now = new Date()
  const yesterday = new Date(now)

  yesterday.setDate(yesterday.getDate() - 1)
  yesterday.setHours(0, 0, 0, 0)

  return {
    action: '',
    feature: AUDIT_LOG_FILTER_VALUES.all,
    from: toDateTimeLocalValue(yesterday),
    module: AUDIT_LOG_FILTER_VALUES.all,
    targetId: '',
    to: toDateTimeLocalValue(now),
    userId: '',
  }
}

function toTranslatedOptions(options: readonly AuditLogOption[], t: Translate) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}

export function AuditLogListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { user } = useAuth()
  const organizationId = user?.organizationId ?? ''
  const [appliedFilters, setAppliedFilters] = useState<AuditLogSearchForm>(() => getDefaultAuditLogSearch())
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<AuditLogLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const { control, handleSubmit, register, reset } = useForm<AuditLogSearchForm>({
    defaultValues: appliedFilters,
  })
  const columns = useMemo(() => createAuditLogTableColumns({
    onView: (auditLog) => navigate(APP_ROUTES.auditLogView(auditLog.id)),
    t,
  }), [navigate, t])

  const loadAuditLogs = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getAuditLogs({
        ...appliedFilters,
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
    void loadAuditLogs(pageNumber)
  }, [loadAuditLogs, pageNumber])

  function handleReset() {
    const emptySearch = getDefaultAuditLogSearch()

    reset(emptySearch)
    setAppliedFilters(emptySearch)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handleSearch(value: AuditLogSearchForm) {
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
        <h1 className="page-title">{t('features.sentinel.auditLogs.pages.list')}</h1>
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
                options={[{ label: t('shared.filters.all'), value: AUDIT_LOG_FILTER_VALUES.all }, ...toTranslatedOptions(AUDIT_LOG_MODULE_OPTIONS, t)]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="feature"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.feature')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[{ label: t('shared.filters.all'), value: AUDIT_LOG_FILTER_VALUES.all }, ...toTranslatedOptions(AUDIT_LOG_FEATURE_OPTIONS, t)]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Field>
          <FieldLabel htmlFor="action">{t('shared.fields.action')}</FieldLabel>
          <Input id="action" {...register('action')} />
        </Field>
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
        <Field>
          <FieldLabel htmlFor="targetId">{t('shared.fields.targetId')}</FieldLabel>
          <Input id="targetId" {...register('targetId')} />
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
        emptyText={t('features.sentinel.auditLogs.messages.empty')}
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
