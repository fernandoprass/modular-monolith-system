import { API_PATHS } from '../../../data/apiPaths'
import { getIamJson, getIamJsonWithQuery, putIamJson } from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import {
  PARAMETER_FILTER_VALUES,
  PARAMETER_QUERY_PARAMS,
  PARAMETER_REQUEST_FIELDS,
  type ParameterDto,
  type ParameterForm,
  type ParameterLiteDto,
  type ParameterListQuery,
  type ParameterUpdateRequest,
} from './parameterTypes'

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
    overrideType: readNumber(source, 'overrideType', 'OverrideType'),
    parameterOverrideId: readNullableString(source, 'parameterOverrideId', 'ParameterOverrideId'),
    title: readString(source, 'title', 'Title'),
    type: readNumber(source, 'type', 'Type'),
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
    overrideType: readNumber(source, 'overrideType', 'OverrideType'),
    type: readNumber(source, 'type', 'Type'),
    value: readString(source, 'value', 'Value'),
  }
}

function normalizePagedParameters(result: PagedResultDto<ParameterLiteDto>): PagedResultDto<ParameterLiteDto> {
  return {
    ...result,
    items: result.items.map(normalizeParameterLiteDto),
  }
}

export async function getParameters(request: ParameterListQuery): Promise<PagedResultDto<ParameterLiteDto>> {
  const response = await getIamJsonWithQuery(
    API_PATHS.iam.parameters.list,
    toParameterQuery(request),
  )

  return normalizePagedParameters(unwrapResult<PagedResultDto<ParameterLiteDto>>(response))
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
