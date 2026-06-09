import { IAM_ACTIONS } from './iamConstants'

export type PermissionDto = {
  id: string
  module: string
  resource: string
  action: string
  code: string
  title: string
  description: string
  isActive: boolean
}

const RESOURCE_ACCESS_ACTIONS = new Set<string>([
  IAM_ACTIONS.list,
  IAM_ACTIONS.view,
])

export function hasPermissionCode(permissions: PermissionDto[], code: string): boolean {
  return permissions.some((permission) => permission.isActive && permission.code === code)
}

export function hasResourceAccess(permissions: PermissionDto[], resource: string): boolean {
  return permissions.some((permission) =>
    permission.isActive
    && permission.resource === resource
    && RESOURCE_ACCESS_ACTIONS.has(permission.action)
  )
}
