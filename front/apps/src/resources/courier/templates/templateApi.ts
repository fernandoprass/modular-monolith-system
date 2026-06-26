import { API_PATHS } from '../../../data/apiPaths'
import {
  deleteCourierJson,
  getCourierJson,
  getCourierJsonWithQuery,
  postCourierJson,
  putCourierJson,
} from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import {
  NOTIFICATION_SEVERITIES,
  RETENTION_POLICIES,
  TEMPLATE_FILTER_VALUES,
  TEMPLATE_QUERY_PARAMS,
  TEMPLATE_REQUEST_FIELDS,
  type NotificationSeverity,
  type RetentionPolicy,
  type TemplateDto,
  type TemplateForm,
  type TemplateListQuery,
  type TemplateLiteDto,
  type TemplateRequest,
  type TemplateTranslationDto,
  type TemplateTranslationForm,
  type TemplateTranslationRequest,
} from './templateTypes'

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value !== TEMPLATE_FILTER_VALUES.all && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toTemplateQuery(request: TemplateListQuery): URLSearchParams {
  const query = new URLSearchParams()

  appendOptional(query, TEMPLATE_QUERY_PARAMS.module, request.module)
  appendOptional(query, TEMPLATE_QUERY_PARAMS.key, request.key)
  appendOptional(query, TEMPLATE_QUERY_PARAMS.name, request.name)
  appendOptional(query, TEMPLATE_QUERY_PARAMS.severity, request.severity)
  query.set(TEMPLATE_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(TEMPLATE_QUERY_PARAMS.pageSize, request.pageSize.toString())

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

function readRecord(value: Record<string, unknown>, ...keys: string[]): Record<string, unknown> | null {
  for (const key of keys) {
    const data = value[key]

    if (isRecord(data)) {
      return data
    }
  }

  return null
}

function unwrapTemplateSource(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) {
    return {}
  }

  if (!('id' in value) && !('Id' in value)) {
    const nested = value.data ?? value.Data

    if (isRecord(nested)) {
      return unwrapTemplateSource(nested)
    }
  }

  return value
}

function readNotificationSeverity(value: Record<string, unknown>, ...keys: string[]): NotificationSeverity {
  const severity = readNumber(value, ...keys)
  const severities = Object.values(NOTIFICATION_SEVERITIES)

  return severities.includes(severity as NotificationSeverity)
    ? severity as NotificationSeverity
    : NOTIFICATION_SEVERITIES.information
}

function readRetentionPolicy(value: Record<string, unknown>, ...keys: string[]): RetentionPolicy {
  const policy = readNumber(value, ...keys)
  const policies = Object.values(RETENTION_POLICIES)

  return policies.includes(policy as RetentionPolicy)
    ? policy as RetentionPolicy
    : RETENTION_POLICIES.operational
}

function normalizeTemplateLiteDto(value: unknown): TemplateLiteDto {
  const source = unwrapTemplateSource(value)

  return {
    id: readString(source, 'id', 'Id'),
    isAllowingOptOut: readBoolean(source, 'isAllowingOptOut', 'IsAllowingOptOut'),
    key: readString(source, 'key', 'Key'),
    module: readString(source, 'module', 'Module'),
    name: readString(source, 'name', 'Name'),
    retentionPolicy: readRetentionPolicy(source, 'retentionPolicy', 'RetentionPolicy'),
    severity: readNotificationSeverity(source, 'severity', 'Severity'),
  }
}

function normalizeTranslation(value: unknown): TemplateTranslationDto {
  const source = isRecord(value) ? value : {}
  const email = readRecord(source, 'email', 'Email')
  const notification = readRecord(source, 'notification', 'Notification')

  return {
    email: email === null ? null : {
      body: readString(email, 'body', 'Body'),
      isHtml: readBoolean(email, 'isHtml', 'IsHtml'),
      subject: readString(email, 'subject', 'Subject'),
    },
    language: readString(source, 'language', 'Language'),
    name: readString(source, 'name', 'Name'),
    notification: notification === null ? null : {
      actionLink: readNullableString(notification, 'actionLink', 'ActionLink'),
      message: readString(notification, 'message', 'Message'),
      title: readString(notification, 'title', 'Title'),
    },
  }
}

function normalizeTemplateDto(value: unknown): TemplateDto {
  const source = unwrapTemplateSource(value)
  const translations = source.translations ?? source.Translations

  return {
    ...normalizeTemplateLiteDto(source),
    createdAt: readString(source, 'createdAt', 'CreatedAt'),
    createdBy: readString(source, 'createdBy', 'CreatedBy'),
    translations: Array.isArray(translations) ? translations.map(normalizeTranslation) : [],
    updatedAt: readNullableString(source, 'updatedAt', 'UpdatedAt'),
    updatedBy: readNullableString(source, 'updatedBy', 'UpdatedBy'),
  }
}

function normalizePagedTemplates(value: unknown): PagedResultDto<TemplateLiteDto> {
  const result = isRecord(value) ? value : {}
  const items = result.items ?? result.Items

  return {
    items: Array.isArray(items) ? items.map(normalizeTemplateLiteDto) : [],
    pageNumber: readNumber(result, 'pageNumber', 'PageNumber'),
    pageSize: readNumber(result, 'pageSize', 'PageSize'),
    totalCount: readNumber(result, 'totalCount', 'TotalCount'),
    totalPages: readNumber(result, 'totalPages', 'TotalPages'),
  }
}

function toTemplateRequest(data: TemplateForm): TemplateRequest {
  return {
    [TEMPLATE_REQUEST_FIELDS.isAllowingOptOut]: data.isAllowingOptOut,
    [TEMPLATE_REQUEST_FIELDS.key]: data.key,
    [TEMPLATE_REQUEST_FIELDS.module]: data.module,
    [TEMPLATE_REQUEST_FIELDS.retentionPolicy]: Number(data.retentionPolicy),
    [TEMPLATE_REQUEST_FIELDS.severity]: Number(data.severity),
  }
}

function toTranslationRequest(data: TemplateTranslationForm): TemplateTranslationRequest {
  return {
    Email: data.emailEnabled ? {
      Body: data.emailBody,
      Subject: data.emailSubject,
    } : null,
    Language: data.language,
    Name: data.name,
    Notification: data.notificationEnabled ? {
      ActionLink: data.notificationActionLink.trim() || null,
      Message: data.notificationMessage,
      Title: data.notificationTitle,
    } : null,
  }
}

export async function getTemplates(request: TemplateListQuery): Promise<PagedResultDto<TemplateLiteDto>> {
  const response = await getCourierJsonWithQuery(API_PATHS.courier.templates.list, toTemplateQuery(request))

  return normalizePagedTemplates(unwrapResult<unknown>(response))
}

export async function getTemplate(id: string): Promise<TemplateDto> {
  const response = await getCourierJson(API_PATHS.courier.templates.byId(id))

  return normalizeTemplateDto(unwrapResult<unknown>(response))
}

export async function createTemplate(request: TemplateForm): Promise<TemplateDto> {
  const response = await postCourierJson(API_PATHS.courier.templates.list, toTemplateRequest(request))

  return normalizeTemplateDto(unwrapResult<unknown>(response))
}

export async function updateTemplate(id: string, request: TemplateForm): Promise<void> {
  const response = await putCourierJson(API_PATHS.courier.templates.byId(id), toTemplateRequest(request))

  ensureResultSuccess(response)
}

export async function deleteTemplate(id: string): Promise<void> {
  const response = await deleteCourierJson(API_PATHS.courier.templates.byId(id))

  ensureResultSuccess(response)
}

export async function addTemplateTranslation(id: string, request: TemplateTranslationForm): Promise<void> {
  const response = await postCourierJson(
    API_PATHS.courier.templates.translations(id),
    toTranslationRequest(request),
  )

  ensureResultSuccess(response)
}

export async function updateTemplateTranslation(
  id: string,
  language: string,
  request: TemplateTranslationForm,
): Promise<void> {
  const response = await putCourierJson(
    API_PATHS.courier.templates.translation(id, language),
    toTranslationRequest(request),
  )

  ensureResultSuccess(response)
}

export async function deleteTemplateTranslation(id: string, language: string): Promise<void> {
  const response = await deleteCourierJson(API_PATHS.courier.templates.translation(id, language))

  ensureResultSuccess(response)
}
