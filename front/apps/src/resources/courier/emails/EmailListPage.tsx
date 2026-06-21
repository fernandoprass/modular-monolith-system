import type { SortingState } from '@tanstack/react-table'
import { Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'

import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { DataTable } from '../../../components/ui/data-table'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { COURIER_PERMISSIONS } from '../../../shared/courierConstants'
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import { OrganizationSelect } from '../../iam/organizations/OrganizationSelect'
import { UserSelect } from '../../iam/users/UserSelect'
import { getEmails } from './emailApi'
import { createEmailTableColumns } from './EmailListPageColumns'
import {
  EMAIL_FEATURE_OPTIONS,
  EMAIL_FILTER_VALUES,
  EMAIL_MODULE_OPTIONS,
  type EmailLiteDto,
  type EmailSearchForm,
} from './emailTypes'
import { toTranslatedEmailOptions } from './emailUi'

function toDateTimeLocalValue(date: Date): string {
  const offsetMs = date.getTimezoneOffset() * 60_000

  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}

function getDefaultEmailSearch(): EmailSearchForm {
  const now = new Date()
  const yesterday = new Date(now)

  yesterday.setDate(yesterday.getDate() - 1)
  yesterday.setHours(0, 0, 0, 0)

  return {
    dateFrom: toDateTimeLocalValue(yesterday),
    dateTo: toDateTimeLocalValue(now),
    feature: EMAIL_FILTER_VALUES.all,
    module: EMAIL_FILTER_VALUES.all,
    organizationId: '',
    recipient: '',
    subject: '',
    userId: '',
  }
}

export function EmailListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { permissions } = useAuth()
  const [appliedFilters, setAppliedFilters] = useState<EmailSearchForm>(() => getDefaultEmailSearch())
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<EmailLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [sorting, setSorting] = useState<SortingState>([])
  const canCreate = hasPermissionCode(permissions, COURIER_PERMISSIONS.emails.write)
  const { control, handleSubmit, register, reset } = useForm<EmailSearchForm>({
    defaultValues: appliedFilters,
  })
  const columns = useMemo(() => createEmailTableColumns({
    onView: (email) => navigate(APP_ROUTES.emailView(email.id)),
    t,
  }), [navigate, t])

  const loadEmails = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getEmails({
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
    void loadEmails(pageNumber)
  }, [loadEmails, pageNumber])

  function handleReset() {
    const emptySearch = getDefaultEmailSearch()

    reset(emptySearch)
    setAppliedFilters(emptySearch)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handleSearch(value: EmailSearchForm) {
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedFilters({ ...value })
  }

  function handlePageSizeChange(nextPageSize: number) {
    setPageSize(nextPageSize)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  const totalPages = result?.totalPages ?? DEFAULT_PAGINATION.pageNumber

  return (
    <main className="page courier-email-page">
      <div className="page-header">
        <h1 className="page-title">{t('features.courier.emails.pages.list')}</h1>
        {canCreate && (
          <Button onClick={() => navigate(APP_ROUTES.emailCreate)} type="button">
            <Plus data-icon="inline-start" />
            {t('features.courier.emails.actions.create')}
          </Button>
        )}
      </div>
      <FilterToolbar onReset={handleReset} onSubmit={handleSubmit(handleSearch)}>
        <Controller
          control={control}
          name="organizationId"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.organization')}</FieldLabel>
              <OrganizationSelect clearable includeInactive onValueChange={field.onChange} value={field.value} />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="userId"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.user')}</FieldLabel>
              <UserSelect clearable includeInactive onValueChange={field.onChange} value={field.value} />
            </Field>
          )}
        />
        <Controller
          control={control}
          name="module"
          render={({ field }) => (
            <Field>
              <FieldLabel>{t('shared.fields.module')}</FieldLabel>
              <Select
                onValueChange={field.onChange}
                options={[
                  { label: t('shared.filters.all'), value: EMAIL_FILTER_VALUES.all },
                  ...toTranslatedEmailOptions(EMAIL_MODULE_OPTIONS, t),
                ]}
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
                options={[
                  { label: t('shared.filters.all'), value: EMAIL_FILTER_VALUES.all },
                  ...toTranslatedEmailOptions(EMAIL_FEATURE_OPTIONS, t),
                ]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Field>
          <FieldLabel htmlFor="email-subject">{t('shared.fields.subject')}</FieldLabel>
          <Input id="email-subject" {...register('subject')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="email-recipient">{t('shared.fields.recipient')}</FieldLabel>
          <Input id="email-recipient" {...register('recipient')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="email-date-from">{t('shared.fields.dateFrom')}</FieldLabel>
          <Input id="email-date-from" required type="datetime-local" {...register('dateFrom')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="email-date-to">{t('shared.fields.dateTo')}</FieldLabel>
          <Input id="email-date-to" required type="datetime-local" {...register('dateTo')} />
        </Field>
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('features.courier.emails.messages.empty')}
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
