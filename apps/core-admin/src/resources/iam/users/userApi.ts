import { API_PATHS } from '../../../data/apiPaths'
import {
  deleteIamJson,
  getIamJson,
  getIamJsonWithQuery,
  postIamJson,
  putIamJson,
} from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import type { PermissionDto } from '../../../shared/permissions'
import {
  USER_QUERY_PARAMS,
  USER_REQUEST_FIELDS,
  type PagedResultDto,
  type UserCreateForm,
  type UserCreateRequest,
  type UserDto,
  type UserLiteDto,
  type UserLookupDto,
  type UserRoleDto,
  type UserUpdateRequest,
} from './userTypes'

export type UserListQuery = {
  email: string
  isActive: string | null
  name: string
  organizationId: string
  pageNumber: number
  pageSize: number
}

export type UserLookupQuery = {
  id: string
  includeInactive: boolean
  organizationId: string
  search: string
  take: number
}

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function buildUserQuery(request: UserListQuery): URLSearchParams {
  const query = new URLSearchParams()

  query.set(USER_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(USER_QUERY_PARAMS.pageSize, request.pageSize.toString())
  appendOptional(query, USER_QUERY_PARAMS.isActive, request.isActive ?? '')
  appendOptional(query, USER_QUERY_PARAMS.name, request.name)
  appendOptional(query, USER_QUERY_PARAMS.email, request.email)
  appendOptional(query, USER_QUERY_PARAMS.organizationId, request.organizationId)

  return query
}

function buildUserLookupQuery(request: UserLookupQuery): URLSearchParams {
  const query = new URLSearchParams()

  appendOptional(query, USER_QUERY_PARAMS.id, request.id)
  appendOptional(query, USER_QUERY_PARAMS.search, request.search)
  appendOptional(query, USER_QUERY_PARAMS.organizationId, request.organizationId)
  query.set(USER_QUERY_PARAMS.includeInactive, request.includeInactive.toString())
  query.set(USER_QUERY_PARAMS.take, request.take.toString())

  return query
}

export function toUserCreateRequest(data: UserCreateForm): UserCreateRequest {
  return {
    [USER_REQUEST_FIELDS.name]: data.name,
    [USER_REQUEST_FIELDS.email]: data.email,
    [USER_REQUEST_FIELDS.password]: data.password,
    [USER_REQUEST_FIELDS.language]: data.language,
    [USER_REQUEST_FIELDS.organizationId]: data.organizationId,
  }
}

export async function getUsers(request: UserListQuery): Promise<PagedResultDto<UserLiteDto>> {
  const response = await getIamJsonWithQuery(
    API_PATHS.iam.users.list,
    buildUserQuery(request),
  )

  return unwrapResult<PagedResultDto<UserLiteDto>>(response)
}

export async function getUserLookup(request: UserLookupQuery): Promise<UserLookupDto[]> {
  const response = await getIamJsonWithQuery(
    API_PATHS.iam.users.lookup,
    buildUserLookupQuery(request),
  )

  return unwrapResult<UserLookupDto[]>(response)
}

export async function getUser(id: string): Promise<UserDto> {
  const response = await getIamJson(API_PATHS.iam.users.byId(id))

  return unwrapResult<UserDto>(response)
}

export async function getCurrentUser(): Promise<UserDto> {
  const response = await getIamJson(API_PATHS.iam.users.profile)

  return unwrapResult<UserDto>(response)
}

export async function getUserRoles(userId: string): Promise<UserRoleDto[]> {
  const response = await getIamJson(API_PATHS.iam.userAccess.userRoles(userId))

  return unwrapResult<UserRoleDto[]>(response)
}

export async function getUserPermissions(userId: string): Promise<PermissionDto[]> {
  const response = await getIamJson(API_PATHS.iam.userAccess.userPermissions(userId))

  return unwrapResult<PermissionDto[]>(response)
}

export async function createUser(data: UserCreateForm): Promise<UserDto> {
  const response = await postIamJson(API_PATHS.iam.users.list, toUserCreateRequest(data))

  return unwrapResult<UserDto>(response)
}

export async function updateUser(id: string, request: UserUpdateRequest): Promise<void> {
  const response = await putIamJson(API_PATHS.iam.users.byId(id), request)

  ensureResultSuccess(response)
}

export async function updateCurrentUser(request: UserUpdateRequest): Promise<void> {
  const response = await putIamJson(API_PATHS.iam.users.profile, request)

  ensureResultSuccess(response)
}

export async function deleteUser(id: string): Promise<void> {
  const response = await deleteIamJson(API_PATHS.iam.users.byId(id))

  ensureResultSuccess(response)
}
