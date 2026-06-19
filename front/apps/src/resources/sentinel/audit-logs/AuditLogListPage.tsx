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
  const filterForm = useForm({
    defaultValues: appliedFilters,
    onSubmit: ({ value }) => {
      setPageNumber(DEFAULT_PAGINATION.pageNumber)
      setAppliedFilters({ ...value })
    },
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
        <h1 className="page-title">{t('features.sentinel.auditLogs.pages.list')}</h1>
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
                options={[{ label: t('shared.filters.all'), value: AUDIT_LOG_FILTER_VALUES.all }, ...toTranslatedOptions(AUDIT_LOG_MODULE_OPTIONS, t)]}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="feature">
          {(field) => (
            <Field>
              <FieldLabel>{t('shared.fields.feature')}</FieldLabel>
              <Select
                onValueChange={field.handleChange}
                options={[{ label: t('shared.filters.all'), value: AUDIT_LOG_FILTER_VALUES.all }, ...toTranslatedOptions(AUDIT_LOG_FEATURE_OPTIONS, t)]}
                value={field.state.value}
              />
            </Field>
          )}
        </filterForm.Field>
        <filterForm.Field name="action">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.action')}</FieldLabel>
              <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} value={field.state.value} />
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
        <filterForm.Field name="targetId">
          {(field) => (
            <Field>
              <FieldLabel htmlFor={field.name}>{t('shared.fields.targetId')}</FieldLabel>
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
