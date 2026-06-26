import { API_PATHS } from '../../../data/apiPaths'
import { getIamJsonWithQuery, putIamJson } from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import type { PagedResultDto } from '../../../shared/pagination'
import type { PermissionDto } from '../../../shared/permissions'
import {
  PERMISSION_FILTER_VALUES,
  PERMISSION_QUERY_PARAMS,
  PERMISSION_REQUEST_FIELDS,
  type PermissionSearchForm,
  type PermissionUpdateForm,
  type PermissionUpdateRequest,
} from './permissionTypes'

export type PermissionListQuery = PermissionSearchForm & {
  pageNumber: number
  pageSize: number
}

function appendOptional(query: URLSearchParams, key: string, value: string): void {
  if (value !== PERMISSION_FILTER_VALUES.all && value.trim().length > 0) {
    query.set(key, value.trim())
  }
}

function toPermissionQuery(request: PermissionListQuery): URLSearchParams {
  const query = new URLSearchParams()

  query.set(PERMISSION_QUERY_PARAMS.pageNumber, request.pageNumber.toString())
  query.set(PERMISSION_QUERY_PARAMS.pageSize, request.pageSize.toString())
  appendOptional(query, PERMISSION_QUERY_PARAMS.module, request.module)
  appendOptional(query, PERMISSION_QUERY_PARAMS.resource, request.resource)
  appendOptional(query, PERMISSION_QUERY_PARAMS.action, request.action)
  appendOptional(query, PERMISSION_QUERY_PARAMS.title, request.title)

  if (request.isActive !== PERMISSION_FILTER_VALUES.all) {
    query.set(PERMISSION_QUERY_PARAMS.isActive, request.isActive)
  }

  query.set(PERMISSION_QUERY_PARAMS.includeInactive, String(request.isActive !== PERMISSION_FILTER_VALUES.active))

  return query
}

function toPermissionUpdateRequest(data: PermissionUpdateForm): PermissionUpdateRequest {
  return {
    [PERMISSION_REQUEST_FIELDS.module]: data.module,
    [PERMISSION_REQUEST_FIELDS.resource]: data.resource,
    [PERMISSION_REQUEST_FIELDS.action]: data.action,
    [PERMISSION_REQUEST_FIELDS.title]: data.title,
    [PERMISSION_REQUEST_FIELDS.description]: data.description,
    [PERMISSION_REQUEST_FIELDS.isActive]: data.isActive,
  }
}

export async function getPermissions(request: PermissionListQuery): Promise<PagedResultDto<PermissionDto>> {
  const response = await getIamJsonWithQuery(
    API_PATHS.iam.permissions.list,
    toPermissionQuery(request),
  )

  return unwrapResult<PagedResultDto<PermissionDto>>(response)
}

export async function updatePermission(id: string, request: PermissionUpdateForm): Promise<void> {
  const response = await putIamJson(API_PATHS.iam.permissions.byId(id), toPermissionUpdateRequest(request))

  ensureResultSuccess(response)
}
