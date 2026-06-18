import { API_PATHS } from '../../../data/apiPaths'
import { getIamJson, getIamJsonWithQuery, putIamJson } from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import {
  PARAMETER_FILTER_VALUES,
  PARAMETER_QUERY_PARAMS,
  PARAMETER_REQUEST_FIELDS,
  PARAMETER_TYPE_VALUES,
  type ParameterDto,
  type ParameterForm,
  type ParameterLiteDto,
  type ParameterListQuery,
  type ParameterUpdateRequest,
} from './parameterTypes'

const PARAMETER_TYPE_BY_NAME: Record<string, number> = {
  boolean: Number(PARAMETER_TYPE_VALUES.boolean),
  character: Number(PARAMETER_TYPE_VALUES.character),
  date: Number(PARAMETER_TYPE_VALUES.date),
  datetime: Number(PARAMETER_TYPE_VALUES.dateTime),
  decimal: Number(PARAMETER_TYPE_VALUES.decimal),
  integer: Number(PARAMETER_TYPE_VALUES.integer),
  list: Number(PARAMETER_TYPE_VALUES.list),
  referenceid: Number(PARAMETER_TYPE_VALUES.referenceId),
  richtext: Number(PARAMETER_TYPE_VALUES.richText),
  string: Number(PARAMETER_TYPE_VALUES.string),
  text: Number(PARAMETER_TYPE_VALUES.text),
  time: Number(PARAMETER_TYPE_VALUES.time),
  uuid: Number(PARAMETER_TYPE_VALUES.uuid),
}

const PARAMETER_OVERRIDE_TYPE_BY_NAME: Record<string, number> = {
  none: 0,
  organization: 1,
  user: 2,
}

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value !== PARAMETER_FILTER_VALUES.all && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toParameterQuery(request: ParameterListQuery): URLSearchParams {
  const query = new URLSearchParams()

  query.set(PARAMETER_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(PARAMETER_QUERY_PARAMS.pageSize, request.pageSize.toString())
  appendOptional(query, PARAMETER_QUERY_PARAMS.module, request.module)
  appendOptional(query, PARAMETER_QUERY_PARAMS.group, request.group)
  appendOptional(query, PARAMETER_QUERY_PARAMS.name, request.name)
  appendOptional(query, PARAMETER_QUERY_PARAMS.title, request.title)

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

function readBoolean(value: Record<string, unknown>, ...keys: string[]): boolean {
  for (const key of keys) {
    const data = value[key]

    if (typeof data === 'boolean') {
      return data
    }

    if (typeof data === 'string') {
      return data.toLowerCase() === 'true'
    }
  }

  return false
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

function readMappedNumber(
  value: Record<string, unknown>,
  mappedValues: Record<string, number>,
  ...keys: string[]
): number {
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

      const mapped = mappedValues[data.toLowerCase()]

      if (mapped !== undefined) {
        return mapped
      }
    }
  }

  return 0
}

function readNullableString(value: Record<string, unknown>, ...keys: string[]): string | null {
  const data = readString(value, ...keys)

  return data.length === 0 ? null : data
}

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function unwrapParameterSource(value: unknown): Record<string, unknown> {
  if (!isRecord(value)) {
    return {}
  }

  if (!('id' in value) && !('Id' in value)) {
    const nested = value.data ?? value.Data

    if (isRecord(nested)) {
      return unwrapParameterSource(nested)
    }
  }

  return value
}

function normalizeParameterLiteDto(value: unknown): ParameterLiteDto {
  const source = unwrapParameterSource(value)

  return {
    group: readString(source, 'group', 'Group'),
    id: readString(source, 'id', 'Id'),
    isOverridden: readBoolean(source, 'isOverridden', 'IsOverridden'),
    module: readString(source, 'module', 'Module'),
    name: readString(source, 'name', 'Name'),
    overrideType: readMappedNumber(source, PARAMETER_OVERRIDE_TYPE_BY_NAME, 'overrideType', 'OverrideType'),
    parameterOverrideId: readNullableString(source, 'parameterOverrideId', 'ParameterOverrideId'),
    title: readString(source, 'title', 'Title'),
    type: readMappedNumber(source, PARAMETER_TYPE_BY_NAME, 'type', 'Type'),
    value: readString(source, 'value', 'Value'),
  }
}

function normalizeParameterDto(value: unknown): ParameterDto {
  const source = unwrapParameterSource(value)

  return {
    ...normalizeParameterLiteDto(source),
    description: readString(source, 'description', 'Description'),
    externalListEndpoint: readNullableString(source, 'externalListEndpoint', 'ExternalListEndpoint'),
    isVisible: readBoolean(source, 'isVisible', 'IsVisible'),
    key: readString(source, 'key', 'Key'),
    listItems: readNullableString(source, 'listItems', 'ListItems'),
    overrideType: readMappedNumber(source, PARAMETER_OVERRIDE_TYPE_BY_NAME, 'overrideType', 'OverrideType'),
    type: readMappedNumber(source, PARAMETER_TYPE_BY_NAME, 'type', 'Type'),
    value: readString(source, 'value', 'Value'),
  }
}

function normalizePagedParameters(value: unknown): PagedResultDto<ParameterLiteDto> {
  const result = isRecord(value) ? value : {}
  const items = result.items ?? result.Items

  return {
    items: Array.isArray(items) ? items.map(normalizeParameterLiteDto) : [],
    pageNumber: readNumber(result, 'pageNumber', 'PageNumber'),
    pageSize: readNumber(result, 'pageSize', 'PageSize'),
    totalCount: readNumber(result, 'totalCount', 'TotalCount'),
    totalPages: readNumber(result, 'totalPages', 'TotalPages'),
  }
}

export async function getParameters(request: ParameterListQuery): Promise<PagedResultDto<ParameterLiteDto>> {
  const response = await getIamJsonWithQuery(
    API_PATHS.iam.parameters.list,
    toParameterQuery(request),
  )

  return normalizePagedParameters(unwrapResult<unknown>(response))
}

export async function getOrganizationSettingsParameters(): Promise<ParameterLiteDto[]> {
  const response = await getIamJson(API_PATHS.iam.parameters.myOrganization)

  return unwrapResult<unknown[]>(response).map(normalizeParameterLiteDto)
}

export async function getUserSettingsParameters(): Promise<ParameterLiteDto[]> {
  const response = await getIamJson(API_PATHS.iam.parameters.me)

  return unwrapResult<unknown[]>(response).map(normalizeParameterLiteDto)
}

function toOptionalValue(value: string): string | null {
  const trimmed = value.trim()

  return trimmed.length === 0 ? null : trimmed
}

function toParameterUpdateRequest(data: ParameterForm): ParameterUpdateRequest {
  return {
    [PARAMETER_REQUEST_FIELDS.description]: data.description,
    [PARAMETER_REQUEST_FIELDS.externalListEndpoint]: toOptionalValue(data.externalListEndpoint),
    [PARAMETER_REQUEST_FIELDS.group]: data.group,
    [PARAMETER_REQUEST_FIELDS.isVisible]: data.isVisible,
    [PARAMETER_REQUEST_FIELDS.listItems]: toOptionalValue(data.listItems),
    [PARAMETER_REQUEST_FIELDS.module]: data.module,
    [PARAMETER_REQUEST_FIELDS.name]: data.name,
    [PARAMETER_REQUEST_FIELDS.overrideType]: Number(data.overrideType),
    [PARAMETER_REQUEST_FIELDS.title]: data.title,
    [PARAMETER_REQUEST_FIELDS.type]: Number(data.type),
    [PARAMETER_REQUEST_FIELDS.validationErrorCustomMessage]: null,
    [PARAMETER_REQUEST_FIELDS.validationRegex]: null,
    [PARAMETER_REQUEST_FIELDS.value]: data.value,
  }
}

export async function getParameter(id: string): Promise<ParameterDto> {
  const response = await getIamJson(API_PATHS.iam.parameters.byId(id))

  return normalizeParameterDto(unwrapResult<unknown>(response))
}

export async function updateParameter(id: string, request: ParameterForm): Promise<void> {
  const response = await putIamJson(API_PATHS.iam.parameters.byId(id), toParameterUpdateRequest(request))

  ensureResultSuccess(response)
}
