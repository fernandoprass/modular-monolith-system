import type { SortingState } from '@tanstack/react-table'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate } from 'react-router-dom'

import { useTranslate } from '../../../app/i18n/i18n'
import { useToast } from '../../../app/ToastProvider'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { DataTable } from '../../../components/ui/data-table'
import { DataTablePagination } from '../../../components/ui/data-table-pagination'
import { ConfirmDialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldLabel } from '../../../components/ui/form'
import { FilterToolbar } from '../../../components/ui/filter-toolbar'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { COURIER_PERMISSIONS } from '../../../shared/courierConstants'
import { DEFAULT_PAGINATION, type PagedResultDto } from '../../../shared/pagination'
import { hasPermissionCode } from '../../../shared/permissions'
import {
  deleteNotification,
  getNotifications,
  markNotificationAsRead,
} from './notificationApi'
import { createNotificationTableColumns } from './NotificationListPageColumns'
import {
  NOTIFICATION_FILTER_VALUES,
  NOTIFICATION_MODULE_OPTIONS,
  NOTIFICATION_STATUSES,
  type NotificationLiteDto,
  type NotificationSearchForm,
} from './notificationTypes'
import { toTranslatedNotificationOptions } from './notificationUi'

function toDateTimeLocalValue(date: Date): string {
  const offsetMs = date.getTimezoneOffset() * 60_000

  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16)
}

function getDefaultNotificationSearch(): NotificationSearchForm {
  const now = new Date()
  const monthAgo = new Date(now)

  monthAgo.setDate(monthAgo.getDate() - 30)
  monthAgo.setHours(0, 0, 0, 0)

  return {
    dateFrom: toDateTimeLocalValue(monthAgo),
    dateTo: toDateTimeLocalValue(now),
    module: NOTIFICATION_FILTER_VALUES.all,
    status: NOTIFICATION_FILTER_VALUES.all,
    title: '',
  }
}

export function NotificationListPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions, user } = useAuth()
  const [appliedFilters, setAppliedFilters] = useState<NotificationSearchForm>(() => getDefaultNotificationSearch())
  const [pageNumber, setPageNumber] = useState<number>(DEFAULT_PAGINATION.pageNumber)
  const [pageSize, setPageSize] = useState<number>(DEFAULT_PAGINATION.pageSize)
  const [result, setResult] = useState<PagedResultDto<NotificationLiteDto> | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [notificationToDelete, setNotificationToDelete] = useState<NotificationLiteDto | null>(null)
  const [sorting, setSorting] = useState<SortingState>([])
  const canWrite = hasPermissionCode(permissions, COURIER_PERMISSIONS.notifications.write)
  const organizationId = user?.organizationId ?? ''
  const userId = user?.id ?? ''
  const { control, handleSubmit, register, reset } = useForm<NotificationSearchForm>({
    defaultValues: appliedFilters,
  })

  const loadNotifications = useCallback(async (targetPage: number) => {
    setIsLoading(true)

    try {
      setResult(await getNotifications({
        ...appliedFilters,
        organizationId,
        pageNumber: targetPage,
        pageSize,
        userId,
      }))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [appliedFilters, notifyError, organizationId, pageSize, t, userId])

  const handleMarkRead = useCallback(async (notification: NotificationLiteDto) => {
    try {
      await markNotificationAsRead(notification.id)
      showSuccess(t('features.courier.notifications.notifications.markedRead'))
      await loadNotifications(pageNumber)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [loadNotifications, notifyError, pageNumber, showSuccess, t])

  const handleOpenLink = useCallback((notification: NotificationLiteDto) => {
    const actionLink = notification.actionLink

    if (actionLink.startsWith('http://') || actionLink.startsWith('https://')) {
      window.location.assign(actionLink)
      return
    }

    navigate(actionLink)
  }, [navigate])

  const columns = useMemo(() => createNotificationTableColumns({
    canDelete: canWrite,
    canMarkRead: canWrite,
    language: user?.language,
    onDelete: setNotificationToDelete,
    onMarkRead: (notification) => void handleMarkRead(notification),
    onOpenLink: handleOpenLink,
    t,
  }), [canWrite, handleMarkRead, handleOpenLink, t, user?.language])

  useEffect(() => {
    void loadNotifications(pageNumber)
  }, [loadNotifications, pageNumber])

  function handleReset() {
    const emptySearch = getDefaultNotificationSearch()

    reset(emptySearch)
    setAppliedFilters(emptySearch)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  function handleSearch(value: NotificationSearchForm) {
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
    setAppliedFilters({ ...value })
  }

  function handlePageSizeChange(nextPageSize: number) {
    setPageSize(nextPageSize)
    setPageNumber(DEFAULT_PAGINATION.pageNumber)
  }

  async function handleDelete() {
    if (notificationToDelete === null) {
      return
    }

    try {
      await deleteNotification(notificationToDelete.id)
      showSuccess(t('features.courier.notifications.notifications.deleted'))
      setNotificationToDelete(null)
      await loadNotifications(pageNumber)
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  const totalPages = result?.totalPages ?? DEFAULT_PAGINATION.pageNumber

  return (
    <main className="page courier-notification-page">
      <div className="page-header">
        <h1 className="page-title">{t('features.courier.notifications.pages.list')}</h1>
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
                options={[
                  { label: t('shared.filters.all'), value: NOTIFICATION_FILTER_VALUES.all },
                  ...toTranslatedNotificationOptions(NOTIFICATION_MODULE_OPTIONS, t),
                ]}
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
                options={[
                  { label: t('shared.filters.all'), value: NOTIFICATION_FILTER_VALUES.all },
                  {
                    label: t('features.courier.notifications.statuses.unread'),
                    value: NOTIFICATION_STATUSES.unread.toString(),
                  },
                  {
                    label: t('features.courier.notifications.statuses.read'),
                    value: NOTIFICATION_STATUSES.read.toString(),
                  },
                ]}
                value={field.value}
              />
            </Field>
          )}
        />
        <Field>
          <FieldLabel htmlFor="notification-title">{t('shared.fields.title')}</FieldLabel>
          <Input id="notification-title" {...register('title')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="notification-date-from">{t('shared.fields.dateFrom')}</FieldLabel>
          <Input id="notification-date-from" required type="datetime-local" {...register('dateFrom')} />
        </Field>
        <Field>
          <FieldLabel htmlFor="notification-date-to">{t('shared.fields.dateTo')}</FieldLabel>
          <Input id="notification-date-to" required type="datetime-local" {...register('dateTo')} />
        </Field>
      </FilterToolbar>
      <DataTable
        columns={columns}
        data={result?.items ?? []}
        emptyText={t('features.courier.notifications.messages.empty')}
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
        backLabel={t('shared.actions.back')}
        cancelText={t('shared.actions.cancel')}
        confirmText={t('shared.actions.delete')}
        onConfirm={() => void handleDelete()}
        onOpenChange={(open) => !open && setNotificationToDelete(null)}
        open={notificationToDelete !== null}
        title={t('features.courier.notifications.messages.deleteTitle')}
      >
        {t('features.courier.notifications.messages.deleteConfirm')}
      </ConfirmDialog>
    </main>
  )
}
