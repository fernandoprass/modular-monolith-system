import { API_PATHS } from '../../../data/apiPaths'
import { deleteIamJson, deleteIamJsonWithBody, getIamJson, getIamJsonWithQuery, postIamJson, putIamJson } from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import type { PermissionDto } from '../../../shared/permissions'
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
  for (const key of keys) {
    const data = value[key]

    if (typeof data === 'string') {
      return data
    }

    if (data === null) {
      return null
    }
  }

  return null
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

function normalizeRoleDto(value: RoleDto): RoleDto {
  const source = value as unknown as Record<string, unknown>

  return {
    ...value,
    description: readString(source, 'description', 'Description'),
    id: readString(source, 'id', 'Id'),
    isActive: readBoolean(source, 'isActive', 'IsActive'),
    isDefault: readBoolean(source, 'isDefault', 'IsDefault'),
    name: readString(source, 'name', 'Name'),
    organizationId: readNullableString(source, 'organizationId', 'OrganizationId'),
  }
}

export async function getRoles(request: RoleSearchForm): Promise<RoleDto[]> {
  const response = await getIamJsonWithQuery(API_PATHS.iam.roles.list, toRoleQuery(request))

  return unwrapResult<RoleDto[]>(response).map(normalizeRoleDto)
}

export async function getRole(id: string): Promise<RoleDto> {
  const response = await getIamJson(API_PATHS.iam.roles.byId(id))

  return normalizeRoleDto(unwrapResult<RoleDto>(response))
}

export async function createRole(request: RoleForm): Promise<RoleDto> {
  const response = await postIamJson(API_PATHS.iam.roles.list, toRoleCreateRequest(request))

  return normalizeRoleDto(unwrapResult<RoleDto>(response))
}

export async function updateRole(id: string, request: RoleForm): Promise<void> {
  const response = await putIamJson(API_PATHS.iam.roles.byId(id), toRoleUpdateRequest(request))

  ensureResultSuccess(response)
}

export async function deleteRole(id: string): Promise<void> {
  const response = await deleteIamJson(API_PATHS.iam.roles.byId(id))

  ensureResultSuccess(response)
}

export async function getRolePermissions(roleId: string): Promise<PermissionDto[]> {
  const response = await postIamJson(API_PATHS.iam.roles.permissions(roleId), {})

  return unwrapResult<PermissionDto[]>(response)
}

export async function getAvailableRolePermissions(roleId: string): Promise<PermissionDto[]> {
  const response = await postIamJson(API_PATHS.iam.roles.availablePermissions(roleId), {})

  return unwrapResult<PermissionDto[]>(response)
}

export async function assignRolePermissions(roleId: string, permissionIds: string[]): Promise<void> {
  const response = await postIamJson(API_PATHS.iam.roles.permissionAssign, {
    PermissionIds: permissionIds,
    RoleId: roleId,
  })

  ensureResultSuccess(response)
}

export async function unassignRolePermissions(roleId: string, permissionIds: string[]): Promise<void> {
  const response = await deleteIamJsonWithBody(API_PATHS.iam.roles.permissionUnassign, {
    PermissionIds: permissionIds,
    RoleId: roleId,
  })

  ensureResultSuccess(response)
}
