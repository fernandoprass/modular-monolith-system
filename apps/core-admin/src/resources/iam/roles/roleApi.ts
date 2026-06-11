import { API_PATHS } from '../../../data/apiPaths'
import { deleteIamJson, getIamJsonWithQuery, postIamJson, putIamJson } from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import {
  ROLE_QUERY_PARAMS,
  ROLE_REQUEST_FIELDS,
  type RoleCreateRequest,
  type RoleDto,
  type RoleForm,
  type RoleSearchForm,
  type RoleUpdateRequest,
} from './roleTypes'

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toRoleQuery(request: RoleSearchForm): URLSearchParams {
  const query = new URLSearchParams()

  appendOptional(query, ROLE_QUERY_PARAMS.organizationId, request.organizationId)
  appendOptional(query, ROLE_QUERY_PARAMS.userId, request.userId)
  appendOptional(query, ROLE_QUERY_PARAMS.name, request.name)

  return query
}

function toRoleCreateRequest(data: RoleForm): RoleCreateRequest {
  return {
    [ROLE_REQUEST_FIELDS.name]: data.name,
    [ROLE_REQUEST_FIELDS.description]: data.description,
    [ROLE_REQUEST_FIELDS.isDefault]: data.isDefault,
    [ROLE_REQUEST_FIELDS.isActive]: data.isActive,
    [ROLE_REQUEST_FIELDS.organizationId]: data.organizationId.trim().length > 0 ? data.organizationId : null,
  }
}

function toRoleUpdateRequest(data: RoleForm): RoleUpdateRequest {
  return {
    [ROLE_REQUEST_FIELDS.name]: data.name,
    [ROLE_REQUEST_FIELDS.description]: data.description,
    [ROLE_REQUEST_FIELDS.isDefault]: data.isDefault,
    [ROLE_REQUEST_FIELDS.isActive]: data.isActive,
  }
}

export async function getRoles(request: RoleSearchForm): Promise<RoleDto[]> {
  const response = await getIamJsonWithQuery(API_PATHS.iam.roles.list, toRoleQuery(request))

  return unwrapResult<RoleDto[]>(response)
}

export async function createRole(request: RoleForm): Promise<RoleDto> {
  const response = await postIamJson(API_PATHS.iam.roles.list, toRoleCreateRequest(request))

  return unwrapResult<RoleDto>(response)
}

export async function updateRole(id: string, request: RoleForm): Promise<void> {
  const response = await putIamJson(API_PATHS.iam.roles.byId(id), toRoleUpdateRequest(request))

  ensureResultSuccess(response)
}

export async function deleteRole(id: string): Promise<void> {
  const response = await deleteIamJson(API_PATHS.iam.roles.byId(id))

  ensureResultSuccess(response)
}
