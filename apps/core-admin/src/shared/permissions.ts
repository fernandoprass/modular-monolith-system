import { IAM_ACTIONS, IAM_MODULE, IAM_RESOURCES } from './iamConstants'

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

export type NavigationResource = {
  module: string
  resource: string
  labelKey: string
  permissions: PermissionDto[]
}

export type NavigationGroup = {
  key: string
  labelKey: string
  resources: NavigationResource[]
}

const IAM_AUTHORIZATION_RESOURCES = new Set<string>([
  IAM_RESOURCES.roles,
  IAM_RESOURCES.permissions,
])

const IAM_RESOURCE_ORDER: string[] = [
  IAM_RESOURCES.organizations,
  IAM_RESOURCES.users,
  IAM_RESOURCES.parameters,
  IAM_RESOURCES.roles,
  IAM_RESOURCES.permissions,
]

const RESOURCE_ACCESS_ACTIONS = new Set<string>([
  IAM_ACTIONS.list,
  IAM_ACTIONS.view,
])

export function hasResourceAccess(permissions: PermissionDto[], resource: string): boolean {
  return permissions.some((permission) =>
    permission.isActive
    && permission.resource === resource
    && RESOURCE_ACCESS_ACTIONS.has(permission.action)
  )
}

function byResourceOrder(left: NavigationResource, right: NavigationResource): number {
  return IAM_RESOURCE_ORDER.indexOf(left.resource) - IAM_RESOURCE_ORDER.indexOf(right.resource)
}

function toResource(module: string, resource: string, permissions: PermissionDto[]): NavigationResource {
  return {
    module,
    resource,
    labelKey: `resources.${module}.${resource}.name`,
    permissions,
  }
}

export function buildNavigationGroups(permissions: PermissionDto[]): NavigationGroup[] {
  const activePermissions = permissions.filter((permission) => permission.isActive)
  const iamPermissions = activePermissions.filter((permission) => permission.module === IAM_MODULE)
  const resources = new Map<string, PermissionDto[]>()
  const topResources: NavigationResource[] = []
  const authorizationResources: NavigationResource[] = []

  iamPermissions.forEach((permission) => {
    const resourcePermissions = resources.get(permission.resource) ?? []
    resourcePermissions.push(permission)
    resources.set(permission.resource, resourcePermissions)
  })

  resources.forEach((resourcePermissions, resource) => {
    const navigationResource = toResource(IAM_MODULE, resource, resourcePermissions)

    if (IAM_AUTHORIZATION_RESOURCES.has(resource)) {
      authorizationResources.push(navigationResource)
      return
    }

    topResources.push(navigationResource)
  })

  const groups: NavigationGroup[] = [
    {
      key: 'iam',
      labelKey: 'navigation.groups.iam',
      resources: topResources.sort(byResourceOrder),
    },
  ]

  if (authorizationResources.length > 0) {
    groups[0].resources.push({
      module: IAM_MODULE,
      resource: IAM_RESOURCES.authorization,
      labelKey: 'navigation.groups.authorization',
      permissions: authorizationResources.flatMap((resource) => resource.permissions),
    })

    groups.push({
      key: 'iam.authorization',
      labelKey: 'navigation.groups.authorization',
      resources: authorizationResources.sort(byResourceOrder),
    })
  }

  return groups.filter((group) => group.resources.length > 0)
}
