import { API_PATHS } from '../../../data/apiPaths'
import { getCourierJson, getCourierJsonWithQuery, postCourierJson } from '../../../data/httpClient'
import { unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import {
  EMAIL_FILTER_VALUES,
  EMAIL_QUERY_PARAMS,
  EMAIL_REQUEST_FIELDS,
  EMAIL_STATUSES,
  type DeliveryAttemptDto,
  type EmailCreateDto,
  type EmailCreateForm,
  type EmailCreateRequest,
  type EmailDto,
  type EmailListQuery,
  type EmailLiteDto,
  type EmailStatus,
} from './emailTypes'

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value !== EMAIL_FILTER_VALUES.all && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toEmailQuery(request: EmailListQuery): URLSearchParams {
  const query = new URLSearchParams()

  appendOptional(query, EMAIL_QUERY_PARAMS.organizationId, request.organizationId)
  appendOptional(query, EMAIL_QUERY_PARAMS.userId, request.userId)
  appendOptional(query, EMAIL_QUERY_PARAMS.module, request.module)
  appendOptional(query, EMAIL_QUERY_PARAMS.feature, request.feature)
  appendOptional(query, EMAIL_QUERY_PARAMS.subject, request.subject)
  appendOptional(query, EMAIL_QUERY_PARAMS.recipient, request.recipient)
  query.set(EMAIL_QUERY_PARAMS.dateFrom, request.dateFrom)
  query.set(EMAIL_QUERY_PARAMS.dateTo, request.dateTo)
  query.set(EMAIL_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(EMAIL_QUERY_PARAMS.pageSize, request.pageSize.toString())

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

function readBoolean(value: Record<string, unknown>, ...keys: string[]): boolean {
  for (const key of keys) {
    const data = value[key]

    if (typeof data === 'boolean') {
      return data
    }
  }

  return false
}

function readNullableString(value: Record<string, unknown>, ...keys: string[]): string | null {
  const data = readString(value, ...keys)

  return data.length === 0 ? null : data
}

function unwrapEmailSource(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) {
    return {}
  }

  if (!('id' in value) && !('Id' in value)) {
    const nested = value.data ?? value.Data

    if (isRecord(nested)) {
      return unwrapEmailSource(nested)
    }
  }

  return value
}

function readEmailStatus(value: Record<string, unknown>, ...keys: string[]): EmailStatus {
  const status = readNumber(value, ...keys)
  const statuses = Object.values(EMAIL_STATUSES)

  return statuses.includes(status as EmailStatus) ? status as EmailStatus : EMAIL_STATUSES.pending
}

function normalizeEmailLiteDto(value: unknown): EmailLiteDto {
  const source = unwrapEmailSource(value)

  return {
    feature: readString(source, 'feature', 'Feature'),
    id: readString(source, 'id', 'Id'),
    module: readString(source, 'module', 'Module'),
    recipient: readString(source, 'recipient', 'Recipient'),
    status: readEmailStatus(source, 'status', 'Status'),
    subject: readString(source, 'subject', 'Subject'),
  }
}

function normalizeDeliveryAttempt(value: unknown): DeliveryAttemptDto {
  const source = isRecord(value) ? value : {}

  return {
    attemptedAt: readString(source, 'attemptedAt', 'AttemptedAt'),
    errorMessage: readString(source, 'errorMessage', 'ErrorMessage'),
    stackTrace: readNullableString(source, 'stackTrace', 'StackTrace'),
  }
}

function normalizeEmailDto(value: unknown): EmailDto {
  const source = unwrapEmailSource(value)
  const attempts = source.attempts ?? source.Attempts

  return {
    ...normalizeEmailLiteDto(source),
    attempts: Array.isArray(attempts) ? attempts.map(normalizeDeliveryAttempt) : [],
    body: readString(source, 'body', 'Body'),
    createdAt: readString(source, 'createdAt', 'CreatedAt'),
    expiresAt: readString(source, 'expiresAt', 'ExpiresAt'),
    isHtml: readBoolean(source, 'isHtml', 'IsHtml'),
    nextAttemptAt: readNullableString(source, 'nextAttemptAt', 'NextAttemptAt'),
    organizationId: readString(source, 'organizationId', 'OrganizationId'),
    retryCount: readNumber(source, 'retryCount', 'RetryCount'),
    sentAt: readNullableString(source, 'sentAt', 'SentAt'),
    templateKey: readString(source, 'templateKey', 'TemplateKey'),
    userId: readString(source, 'userId', 'UserId'),
  }
}

function normalizePagedEmails(value: unknown): PagedResultDto<EmailLiteDto> {
  const result = isRecord(value) ? value : {}
  const items = result.items ?? result.Items

  return {
    items: Array.isArray(items) ? items.map(normalizeEmailLiteDto) : [],
    pageNumber: readNumber(result, 'pageNumber', 'PageNumber'),
    pageSize: readNumber(result, 'pageSize', 'PageSize'),
    totalCount: readNumber(result, 'totalCount', 'TotalCount'),
    totalPages: readNumber(result, 'totalPages', 'TotalPages'),
  }
}

function toEmailCreateRequest(data: EmailCreateForm): EmailCreateRequest {
  return {
    [EMAIL_REQUEST_FIELDS.body]: data.body,
    [EMAIL_REQUEST_FIELDS.feature]: data.feature,
    [EMAIL_REQUEST_FIELDS.isHtml]: data.isHtml,
    [EMAIL_REQUEST_FIELDS.module]: data.module,
    [EMAIL_REQUEST_FIELDS.organizationId]: data.organizationId,
    [EMAIL_REQUEST_FIELDS.recipient]: data.recipient,
    [EMAIL_REQUEST_FIELDS.subject]: data.subject,
    [EMAIL_REQUEST_FIELDS.templateKey]: data.templateKey,
    [EMAIL_REQUEST_FIELDS.userId]: data.userId,
  }
}

export async function getEmails(request: EmailListQuery): Promise<PagedResultDto<EmailLiteDto>> {
  const response = await getCourierJsonWithQuery(API_PATHS.courier.emails.list, toEmailQuery(request))

  return normalizePagedEmails(unwrapResult<unknown>(response))
}

export async function getEmail(id: string): Promise<EmailDto> {
  const response = await getCourierJson(API_PATHS.courier.emails.byId(id))

  return normalizeEmailDto(unwrapResult<unknown>(response))
}

export async function createEmail(request: EmailCreateForm): Promise<EmailCreateDto> {
  const response = await postCourierJson(API_PATHS.courier.emails.list, toEmailCreateRequest(request))
  const result = unwrapResult<unknown>(response)
  const source = unwrapEmailSource(result)

  return { id: readString(source, 'id', 'Id') }
}
