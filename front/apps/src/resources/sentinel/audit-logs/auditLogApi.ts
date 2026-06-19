import { API_PATHS } from '../../../data/apiPaths'
import { getSentinelJson, getSentinelJsonWithQuery } from '../../../data/httpClient'
import { unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import {
  AUDIT_PRIVACY_LEVELS,
  AUDIT_LOG_FILTER_VALUES,
  AUDIT_LOG_QUERY_PARAMS,
  type AuditPrivacyLevel,
  type AuditLogDto,
  type AuditLogListQuery,
  type AuditLogLiteDto,
} from './auditLogTypes'

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value !== AUDIT_LOG_FILTER_VALUES.all && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toAuditLogQuery(request: AuditLogListQuery): URLSearchParams {
  const query = new URLSearchParams()

  appendOptional(query, AUDIT_LOG_QUERY_PARAMS.action, request.action)
  appendOptional(query, AUDIT_LOG_QUERY_PARAMS.feature, request.feature)
  appendOptional(query, AUDIT_LOG_QUERY_PARAMS.module, request.module)
  appendOptional(query, AUDIT_LOG_QUERY_PARAMS.organizationId, request.organizationId)
  appendOptional(query, AUDIT_LOG_QUERY_PARAMS.targetId, request.targetId)
  appendOptional(query, AUDIT_LOG_QUERY_PARAMS.userId, request.userId)
  query.set(AUDIT_LOG_QUERY_PARAMS.from, request.from)
  query.set(AUDIT_LOG_QUERY_PARAMS.to, request.to)
  query.set(AUDIT_LOG_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(AUDIT_LOG_QUERY_PARAMS.pageSize, request.pageSize.toString())

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

function readAuditPrivacyLevel(value: Record<string, unknown>, ...keys: string[]): AuditPrivacyLevel {
  const data = readNumber(value, ...keys)
  const levels = Object.values(AUDIT_PRIVACY_LEVELS)

  return levels.includes(data as AuditPrivacyLevel) ? data as AuditPrivacyLevel : AUDIT_PRIVACY_LEVELS.low
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function unwrapAuditLogSource(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) {
    return {}
  }

  if (!('id' in value) && !('Id' in value)) {
    const nested = value.data ?? value.Data

    if (isRecord(nested)) {
      return unwrapAuditLogSource(nested)
    }
  }

  return value
}

function normalizeAuditLogLiteDto(value: unknown): AuditLogLiteDto {
  const source = unwrapAuditLogSource(value)

  return {
    action: readString(source, 'action', 'Action'),
    createdAt: readString(source, 'createdAt', 'CreatedAt'),
    description: readString(source, 'description', 'Description'),
    feature: readString(source, 'feature', 'Feature'),
    id: readString(source, 'id', 'Id'),
    module: readString(source, 'module', 'Module'),
    privacyLevel: readAuditPrivacyLevel(source, 'privacyLevel', 'PrivacyLevel'),
  }
}

function normalizeAuditLogDto(value: unknown): AuditLogDto {
  const source = unwrapAuditLogSource(value)

  return {
    ...normalizeAuditLogLiteDto(source),
    expiresAt: readString(source, 'expiresAt', 'ExpiresAt'),
    ipAddress: readNullableString(source, 'ipAddress', 'IpAddress'),
    metadata: readString(source, 'metadata', 'Metadata'),
    organizationId: readString(source, 'organizationId', 'OrganizationId'),
    targetId: readString(source, 'targetId', 'TargetId'),
    userId: readString(source, 'userId', 'UserId'),
    userAgent: readNullableString(source, 'userAgent', 'UserAgent'),
  }
}

function normalizePagedAuditLogs(value: unknown): PagedResultDto<AuditLogLiteDto> {
  const result = isRecord(value) ? value : {}
  const items = result.items ?? result.Items

  return {
    items: Array.isArray(items) ? items.map(normalizeAuditLogLiteDto) : [],
    pageNumber: readNumber(result, 'pageNumber', 'PageNumber'),
    pageSize: readNumber(result, 'pageSize', 'PageSize'),
    totalCount: readNumber(result, 'totalCount', 'TotalCount'),
    totalPages: readNumber(result, 'totalPages', 'TotalPages'),
  }
}

export async function getAuditLogs(request: AuditLogListQuery): Promise<PagedResultDto<AuditLogLiteDto>> {
  const response = await getSentinelJsonWithQuery(
    API_PATHS.sentinel.auditLogs.list,
    toAuditLogQuery(request),
  )

  return normalizePagedAuditLogs(unwrapResult<unknown>(response))
}

export async function getAuditLog(id: string): Promise<AuditLogDto> {
  const response = await getSentinelJson(API_PATHS.sentinel.auditLogs.byId(id))

  return normalizeAuditLogDto(unwrapResult<unknown>(response))
}
