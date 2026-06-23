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
  RETENTION_POLICIES,
  TEMPLATE_FILTER_VALUES,
  TEMPLATE_QUERY_PARAMS,
  TEMPLATE_REQUEST_FIELDS,
  TEMPLATE_TRANSLATION_REQUEST_FIELDS,
  TEMPLATE_TYPES,
  type RetentionPolicy,
  type TemplateDto,
  type TemplateEmailTranslationDto,
  type TemplateForm,
  type TemplateListQuery,
  type TemplateLiteDto,
  type TemplateRequest,
  type TemplateTranslationForm,
  type TemplateTranslationRequest,
  type TemplateType,
} from './templateTypes'

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value !== TEMPLATE_FILTER_VALUES.all && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toTemplateQuery(request: TemplateListQuery): URLSearchParams {
  const query = new URLSearchParams()

  appendOptional(query, TEMPLATE_QUERY_PARAMS.key, request.key)
  appendOptional(query, TEMPLATE_QUERY_PARAMS.name, request.name)
  appendOptional(query, TEMPLATE_QUERY_PARAMS.type, request.type)
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

function readTemplateType(value: Record<string, unknown>, ...keys: string[]): TemplateType {
  const type = readNumber(value, ...keys)
  const types = Object.values(TEMPLATE_TYPES)

  return types.includes(type as TemplateType) ? type as TemplateType : TEMPLATE_TYPES.email
}

function readRetentionPolicy(value: Record<string, unknown>, ...keys: string[]): RetentionPolicy {
  const policy = readNumber(value, ...keys)
  const policies = Object.values(RETENTION_POLICIES)

  return policies.includes(policy as RetentionPolicy) ? policy as RetentionPolicy : RETENTION_POLICIES.operational
}

function normalizeTemplateLiteDto(value: unknown): TemplateLiteDto {
  const source = unwrapTemplateSource(value)

  return {
    id: readString(source, 'id', 'Id'),
    key: readString(source, 'key', 'Key'),
    name: readString(source, 'name', 'Name'),
    retentionPolicy: readRetentionPolicy(source, 'retentionPolicy', 'RetentionPolicy'),
    type: readTemplateType(source, 'type', 'Type'),
  }
}

function normalizeTranslation(value: unknown): TemplateEmailTranslationDto {
  const source = isRecord(value) ? value : {}

  return {
    body: readString(source, 'body', 'Body'),
    isHtml: readBoolean(source, 'isHtml', 'IsHtml'),
    language: readString(source, 'language', 'Language'),
    subject: readString(source, 'subject', 'Subject'),
  }
}

function normalizeTemplateDto(value: unknown): TemplateDto {
  const source = unwrapTemplateSource(value)
  const translations = source.emailTranslations ?? source.EmailTranslations

  return {
    ...normalizeTemplateLiteDto(source),
    createdAt: readString(source, 'createdAt', 'CreatedAt'),
    createdBy: readString(source, 'createdBy', 'CreatedBy'),
    emailTranslations: Array.isArray(translations) ? translations.map(normalizeTranslation) : [],
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
    [TEMPLATE_REQUEST_FIELDS.key]: data.key,
    [TEMPLATE_REQUEST_FIELDS.name]: data.name,
    [TEMPLATE_REQUEST_FIELDS.retentionPolicy]: Number(data.retentionPolicy),
    [TEMPLATE_REQUEST_FIELDS.type]: Number(data.type),
  }
}

function toTranslationRequest(data: TemplateTranslationForm): TemplateTranslationRequest {
  return {
    [TEMPLATE_TRANSLATION_REQUEST_FIELDS.body]: data.body,
    [TEMPLATE_TRANSLATION_REQUEST_FIELDS.language]: data.language,
    [TEMPLATE_TRANSLATION_REQUEST_FIELDS.subject]: data.subject,
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
    API_PATHS.courier.templates.emailTranslations(id),
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
    API_PATHS.courier.templates.emailTranslation(id, language),
    toTranslationRequest(request),
  )

  ensureResultSuccess(response)
}

export async function deleteTemplateTranslation(id: string, language: string): Promise<void> {
  const response = await deleteCourierJson(API_PATHS.courier.templates.emailTranslation(id, language))

  ensureResultSuccess(response)
}
