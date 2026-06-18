import { API_PATHS } from '../../../data/apiPaths'
import { deleteIamJsonWithBody, getIamJson, postIamJson } from '../../../data/httpClient'
import { ensureResultSuccess, unwrapResult } from '../../../data/result'
import type { RoleDto } from '../roles/roleTypes'
import type { UserRoleDto } from '../users/userTypes'

export async function getAvailableUserRoles(userId: string): Promise<RoleDto[]> {
  const response = await getIamJson(API_PATHS.iam.userAccess.availableRoles(userId))

  return unwrapResult<RoleDto[]>(response)
}

export async function getAssignedUserRoles(userId: string): Promise<UserRoleDto[]> {
  const response = await getIamJson(API_PATHS.iam.userAccess.userRoles(userId))

  return unwrapResult<UserRoleDto[]>(response)
}

export async function assignUserRoles(
  userId: string,
  startsAt: string,
  expiresAt: string | null,
  roleIds: string[],
): Promise<void> {
  const response = await postIamJson(API_PATHS.iam.userAccess.roleAssign, {
    ExpiresAt: expiresAt,
    RoleIds: roleIds,
    StartsAt: startsAt,
    UserId: userId,
  })

  ensureResultSuccess(response)
}

export async function unassignUserRoles(userId: string, roleIds: string[]): Promise<void> {
  const response = await deleteIamJsonWithBody(API_PATHS.iam.userAccess.roleUnassign, {
    RoleIds: roleIds,
    UserId: userId,
  })

  ensureResultSuccess(response)
}
