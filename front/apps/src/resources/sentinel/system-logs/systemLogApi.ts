import { API_PATHS } from '../../../data/apiPaths'
import { getSentinelJson, getSentinelJsonWithQuery } from '../../../data/httpClient'
import { unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import {
  SYSTEM_LOG_FILTER_VALUES,
  SYSTEM_LOG_LEVELS,
  SYSTEM_LOG_QUERY_PARAMS,
  SYSTEM_LOG_STATUSES,
  type SystemLogDto,
  type SystemLogLevel,
  type SystemLogListQuery,
  type SystemLogLiteDto,
  type SystemLogStatus,
} from './systemLogTypes'

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value !== SYSTEM_LOG_FILTER_VALUES.all && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toSystemLogQuery(request: SystemLogListQuery): URLSearchParams {
  const query = new URLSearchParams()

  appendOptional(query, SYSTEM_LOG_QUERY_PARAMS.organizationId, request.organizationId)
  appendOptional(query, SYSTEM_LOG_QUERY_PARAMS.userId, request.userId)
  appendOptional(query, SYSTEM_LOG_QUERY_PARAMS.level, request.level)
  appendOptional(query, SYSTEM_LOG_QUERY_PARAMS.status, request.status)
  appendOptional(query, SYSTEM_LOG_QUERY_PARAMS.requestId, request.requestId)
  query.set(SYSTEM_LOG_QUERY_PARAMS.from, request.from)
  query.set(SYSTEM_LOG_QUERY_PARAMS.to, request.to)
  query.set(SYSTEM_LOG_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(SYSTEM_LOG_QUERY_PARAMS.pageSize, request.pageSize.toString())

  return query
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

function readNullableString(value: Record<string, unknown>, ...keys: string[]): string | null {
  const data = readString(value, ...keys)

  return data.length === 0 ? null : data
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

function readSystemLogLevel(value: Record<string, unknown>, ...keys: string[]): SystemLogLevel {
  const data = readNumber(value, ...keys)
  const levels = Object.values(SYSTEM_LOG_LEVELS)

  return levels.includes(data as SystemLogLevel) ? data as SystemLogLevel : SYSTEM_LOG_LEVELS.information
}

function readSystemLogStatus(value: Record<string, unknown>, ...keys: string[]): SystemLogStatus {
  const data = readNumber(value, ...keys)
  const statuses = Object.values(SYSTEM_LOG_STATUSES)

  return statuses.includes(data as SystemLogStatus) ? data as SystemLogStatus : SYSTEM_LOG_STATUSES.unknown
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function unwrapSystemLogSource(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) {
    return {}
  }

  if (!('id' in value) && !('Id' in value)) {
    const nested = value.data ?? value.Data

    if (isRecord(nested)) {
      return unwrapSystemLogSource(nested)
    }
  }

  return value
}

function normalizeSystemLogLiteDto(value: unknown): SystemLogLiteDto {
  const source = unwrapSystemLogSource(value)

  return {
    createdAt: readString(source, 'createdAt', 'CreatedAt'),
    id: readString(source, 'id', 'Id'),
    level: readSystemLogLevel(source, 'level', 'Level'),
    message: readString(source, 'message', 'Message'),
    module: readString(source, 'module', 'Module'),
    status: readSystemLogStatus(source, 'status', 'Status'),
  }
}

function normalizeSystemLogDto(value: unknown): SystemLogDto {
  const source = unwrapSystemLogSource(value)

  return {
    ...normalizeSystemLogLiteDto(source),
    exception: readNullableString(source, 'exception', 'Exception'),
    expiresAt: readString(source, 'expiresAt', 'ExpiresAt'),
    organizationId: readNullableString(source, 'organizationId', 'OrganizationId'),
    propertiesJson: readString(source, 'propertiesJson', 'PropertiesJson'),
    requestId: readNullableString(source, 'requestId', 'RequestId'),
    stackTrace: readNullableString(source, 'stackTrace', 'StackTrace'),
    userId: readNullableString(source, 'userId', 'UserId'),
  }
}

function normalizePagedSystemLogs(value: unknown): PagedResultDto<SystemLogLiteDto> {
  const result = isRecord(value) ? value : {}
  const items = result.items ?? result.Items

  return {
    items: Array.isArray(items) ? items.map(normalizeSystemLogLiteDto) : [],
    pageNumber: readNumber(result, 'pageNumber', 'PageNumber'),
    pageSize: readNumber(result, 'pageSize', 'PageSize'),
    totalCount: readNumber(result, 'totalCount', 'TotalCount'),
    totalPages: readNumber(result, 'totalPages', 'TotalPages'),
  }
}

export async function getSystemLogs(request: SystemLogListQuery): Promise<PagedResultDto<SystemLogLiteDto>> {
  const response = await getSentinelJsonWithQuery(
    API_PATHS.sentinel.systemLogs.list,
    toSystemLogQuery(request),
  )

  return normalizePagedSystemLogs(unwrapResult<unknown>(response))
}

export async function getSystemLog(id: string): Promise<SystemLogDto> {
  const response = await getSentinelJson(API_PATHS.sentinel.systemLogs.byId(id))

  return normalizeSystemLogDto(unwrapResult<unknown>(response))
}
