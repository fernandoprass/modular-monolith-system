import { API_PATHS } from '../../../data/apiPaths'
import {
  deleteCourierJson,
  getCourierJson,
  getCourierJsonWithQuery,
  patchCourierJson,
} from '../../../data/httpClient'
import { unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import {
  NOTIFICATION_FILTER_VALUES,
  NOTIFICATION_QUERY_PARAMS,
  NOTIFICATION_STATUSES,
  type NotificationListQuery,
  type NotificationLiteDto,
  type NotificationStatus,
} from './notificationTypes'

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value !== NOTIFICATION_FILTER_VALUES.all && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toNotificationQuery(request: NotificationListQuery): URLSearchParams {
  const query = new URLSearchParams()

  appendOptional(query, NOTIFICATION_QUERY_PARAMS.organizationId, request.organizationId)
  appendOptional(query, NOTIFICATION_QUERY_PARAMS.userId, request.userId)
  appendOptional(query, NOTIFICATION_QUERY_PARAMS.module, request.module)
  appendOptional(query, NOTIFICATION_QUERY_PARAMS.title, request.title)
  appendOptional(query, NOTIFICATION_QUERY_PARAMS.status, request.status)
  query.set(NOTIFICATION_QUERY_PARAMS.dateFrom, request.dateFrom)
  query.set(NOTIFICATION_QUERY_PARAMS.dateTo, request.dateTo)
  query.set(NOTIFICATION_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(NOTIFICATION_QUERY_PARAMS.pageSize, request.pageSize.toString())

  return query
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function readString(value: Record<string, unknown>, ...keys: string[]): string {
  for (const key of keys) {
    const data = value[key]

    if (typeof data === 'string') {
      return data
    }
  }

  return ''
}

function readNumber(value: Record<string, unknown>, ...keys: string[]): number {
  for (const key of keys) {
    const data = value[key]

    if (typeof data === 'number') {
      return data
    }

    if (typeof data === 'string') {
      const parsed = Number(data)

      if (Number.isFinite(parsed)) {
        return parsed
      }
    }
  }

  return 0
}

function readNullableString(value: Record<string, unknown>, ...keys: string[]): string | null {
  const data = readString(value, ...keys)

  return data.length === 0 ? null : data
}

function unwrapNotificationSource(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) {
    return {}
  }

  if (!('id' in value) && !('Id' in value)) {
    const nested = value.data ?? value.Data

    if (isRecord(nested)) {
      return unwrapNotificationSource(nested)
    }
  }

  return value
}

function readNotificationStatus(value: Record<string, unknown>, ...keys: string[]): NotificationStatus {
  const status = readNumber(value, ...keys)
  const statuses = Object.values(NOTIFICATION_STATUSES)

  return statuses.includes(status as NotificationStatus)
    ? status as NotificationStatus
    : NOTIFICATION_STATUSES.unread
}

function normalizeNotificationLiteDto(value: unknown): NotificationLiteDto {
  const source = unwrapNotificationSource(value)

  return {
    actionLink: readString(source, 'actionLink', 'ActionLink'),
    createdAt: readString(source, 'createdAt', 'CreatedAt'),
    feature: readString(source, 'feature', 'Feature'),
    id: readString(source, 'id', 'Id'),
    message: readString(source, 'message', 'Message'),
    module: readString(source, 'module', 'Module'),
    readAt: readNullableString(source, 'readAt', 'ReadAt'),
    status: readNotificationStatus(source, 'status', 'Status'),
    title: readString(source, 'title', 'Title'),
  }
}

function normalizePagedNotifications(value: unknown): PagedResultDto<NotificationLiteDto> {
  const result = isRecord(value) ? value : {}
  const items = result.items ?? result.Items

  return {
    items: Array.isArray(items) ? items.map(normalizeNotificationLiteDto) : [],
    pageNumber: readNumber(result, 'pageNumber', 'PageNumber'),
    pageSize: readNumber(result, 'pageSize', 'PageSize'),
    totalCount: readNumber(result, 'totalCount', 'TotalCount'),
    totalPages: readNumber(result, 'totalPages', 'TotalPages'),
  }
}

export async function getNotifications(request: NotificationListQuery): Promise<PagedResultDto<NotificationLiteDto>> {
  const response = await getCourierJsonWithQuery(API_PATHS.courier.notifications.list, toNotificationQuery(request))

  return normalizePagedNotifications(unwrapResult<unknown>(response))
}

export async function getUnreadNotificationCount(): Promise<number> {
  const response = await getCourierJson(API_PATHS.courier.notifications.unreadCount)

  return readNumber({ count: unwrapResult<unknown>(response) }, 'count')
}

export async function markNotificationAsRead(id: string): Promise<void> {
  await patchCourierJson(API_PATHS.courier.notifications.read(id), {})
}

export async function deleteNotification(id: string): Promise<void> {
  await deleteCourierJson(API_PATHS.courier.notifications.byId(id))
}
