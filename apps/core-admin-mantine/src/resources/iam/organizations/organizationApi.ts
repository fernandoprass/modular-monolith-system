import { API_PATHS } from '../../../data/apiPaths'
import { deleteJson, getJson, getJsonWithQuery, patchJson, postJson, putJson } from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import {
  ORGANIZATION_QUERY_PARAMS,
  ORGANIZATION_REQUEST_FIELDS,
  type OrganizationCodeUpdateRequest,
  type OrganizationCreateForm,
  type OrganizationCreateRequest,
  type OrganizationDto,
  type OrganizationUpdateRequest,
  type PagedResultDto,
} from './organizationTypes'

export type OrganizationListQuery = {
  code: string
  name: string
  pageNumber: number
  pageSize: number
  type: string | null
}

function appendOptional(query: URLSearchParams, key: string, value: string | null): void {
  if (value !== null && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function buildOrganizationQuery(request: OrganizationListQuery): URLSearchParams {
  const query = new URLSearchParams()

  query.set(ORGANIZATION_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(ORGANIZATION_QUERY_PARAMS.pageSize, request.pageSize.toString())
  appendOptional(query, ORGANIZATION_QUERY_PARAMS.code, request.code)
  appendOptional(query, ORGANIZATION_QUERY_PARAMS.name, request.name)
  appendOptional(query, ORGANIZATION_QUERY_PARAMS.type, request.type)

  return query
}

export function toOrganizationCreateRequest(data: OrganizationCreateForm): OrganizationCreateRequest {
  return {
    [ORGANIZATION_REQUEST_FIELDS.type]: data.type,
    [ORGANIZATION_REQUEST_FIELDS.name]: data.name,
    [ORGANIZATION_REQUEST_FIELDS.code]: data.code,
    [ORGANIZATION_REQUEST_FIELDS.description]: data.description,
    [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: data.defaultLanguage,
    [ORGANIZATION_REQUEST_FIELDS.user]: {
      [ORGANIZATION_REQUEST_FIELDS.userName]: data.userName,
      [ORGANIZATION_REQUEST_FIELDS.userEmail]: data.userEmail,
      [ORGANIZATION_REQUEST_FIELDS.userPassword]: data.userPassword,
    },
  }
}

export async function createOrganization(data: OrganizationCreateForm): Promise<OrganizationDto> {
  const response = await postJson(
    API_PATHS.iam.organizations.list,
    toOrganizationCreateRequest(data),
  )

  return unwrapResult<OrganizationDto>(response)
}

export async function getOrganizations(
  request: OrganizationListQuery,
): Promise<PagedResultDto<OrganizationDto>> {
  const response = await getJsonWithQuery(
    API_PATHS.iam.organizations.list,
    buildOrganizationQuery(request),
  )

  return unwrapResult<PagedResultDto<OrganizationDto>>(response)
}

export async function getOrganization(id: string): Promise<OrganizationDto> {
  const response = await getJson(API_PATHS.iam.organizations.byId(id))

  return unwrapResult<OrganizationDto>(response)
}

export async function updateOrganization(id: string, request: OrganizationUpdateRequest): Promise<void> {
  const response = await putJson(API_PATHS.iam.organizations.byId(id), request)

  ensureResultSuccess(response)
}

export async function updateOrganizationCode(
  id: string,
  request: OrganizationCodeUpdateRequest,
): Promise<void> {
  const response = await patchJson(API_PATHS.iam.organizations.code(id), request)

  ensureResultSuccess(response)
}

export async function deleteOrganization(id: string): Promise<void> {
  const response = await deleteJson(API_PATHS.iam.organizations.byId(id))

  ensureResultSuccess(response)
}
