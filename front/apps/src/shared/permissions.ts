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

export type PermissionCode = string

const RESOURCE_ACCESS_ACTIONS = new Set<string>([
  IAM_ACTIONS.read,
])

export function hasPermissionCode(permissions: PermissionCode[], code: string): boolean {
  return permissions.includes(code)
}

export function hasResourceAccess(permissions: PermissionCode[], resource: string): boolean {
  return permissions.some((permission) => {
    const [, permissionResource, action] = permission.split('.')

    return permissionResource === resource && action !== undefined && RESOURCE_ACCESS_ACTIONS.has(action)
  })
}
