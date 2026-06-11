import { API_PATHS } from '../../../data/apiPaths'
import {
  deleteIamJson,
  getIamJson,
  getIamJsonWithQuery,
  patchIamJson,
  postIamJson,
  putIamJson,
} from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import {
  ORGANIZATION_QUERY_PARAMS,
  ORGANIZATION_REQUEST_FIELDS,
  type OrganizationCodeUpdateRequest,
  type OrganizationCreateForm,
  type OrganizationCreateRequest,
  type OrganizationDto,
  type OrganizationLookupDto,
  type OrganizationUpdateRequest,
  type PagedResultDto,
} from './organizationTypes'

export type OrganizationListQuery = {
  code: string
  isActive: string | null
  name: string
  pageNumber: number
  pageSize: number
  type: string | null
}

export type OrganizationLookupQuery = {
  id?: string
  includeInactive?: boolean
  search: string
  take?: number
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
  appendOptional(query, ORGANIZATION_QUERY_PARAMS.isActive, request.isActive)
  appendOptional(query, ORGANIZATION_QUERY_PARAMS.name, request.name)
  appendOptional(query, ORGANIZATION_QUERY_PARAMS.type, request.type)

  return query
}

function buildOrganizationLookupQuery(request: OrganizationLookupQuery): URLSearchParams {
  const query = new URLSearchParams()

  query.set(ORGANIZATION_QUERY_PARAMS.includeInactive, String(request.includeInactive ?? false))
  query.set(ORGANIZATION_QUERY_PARAMS.take, String(request.take ?? 25))
  appendOptional(query, ORGANIZATION_QUERY_PARAMS.search, request.search)

  if (request.id !== undefined && request.id.trim().length > 0) {
    query.set(ORGANIZATION_QUERY_PARAMS.id, request.id.trim())
  }

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
  const response = await postIamJson(
    API_PATHS.iam.organizations.list,
    toOrganizationCreateRequest(data),
  )

  return unwrapResult<OrganizationDto>(response)
}

export async function getOrganizations(
  request: OrganizationListQuery,
): Promise<PagedResultDto<OrganizationDto>> {
  const response = await getIamJsonWithQuery(
    API_PATHS.iam.organizations.list,
    buildOrganizationQuery(request),
  )

  return unwrapResult<PagedResultDto<OrganizationDto>>(response)
}

export async function getOrganizationLookup(
  request: OrganizationLookupQuery,
): Promise<OrganizationLookupDto[]> {
  const response = await getIamJsonWithQuery(
    API_PATHS.iam.organizations.lookup,
    buildOrganizationLookupQuery(request),
  )

  return unwrapResult<OrganizationLookupDto[]>(response)
}

export async function getOrganization(id: string): Promise<OrganizationDto> {
  const response = await getIamJson(API_PATHS.iam.organizations.byId(id))

  return unwrapResult<OrganizationDto>(response)
}

export async function getOwnOrganization(): Promise<OrganizationDto> {
  const response = await getIamJson(API_PATHS.iam.organizations.own)

  return unwrapResult<OrganizationDto>(response)
}

export async function updateOrganization(id: string, request: OrganizationUpdateRequest): Promise<void> {
  const response = await putIamJson(API_PATHS.iam.organizations.byId(id), request)

  ensureResultSuccess(response)
}

export async function updateOwnOrganization(request: OrganizationUpdateRequest): Promise<void> {
  const response = await putIamJson(API_PATHS.iam.organizations.own, request)

  ensureResultSuccess(response)
}

export async function updateOrganizationCode(
  id: string,
  request: OrganizationCodeUpdateRequest,
): Promise<void> {
  const response = await patchIamJson(API_PATHS.iam.organizations.code(id), request)

  ensureResultSuccess(response)
}

export async function deleteOrganization(id: string): Promise<void> {
  const response = await deleteIamJson(API_PATHS.iam.organizations.byId(id))

  ensureResultSuccess(response)
}
