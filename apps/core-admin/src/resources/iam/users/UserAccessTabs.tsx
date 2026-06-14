import { useEffect, useState } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Badge } from '../../../components/ui/badge'
import { Empty, EmptyDescription } from '../../../components/ui/empty'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../../../components/ui/tabs'
import { formatUserDateTime } from '../../../shared/dateFormat'
import type { PermissionDto } from '../../../shared/permissions'
import { getUserPermissions, getUserRoles } from './userApi'
import type { UserRoleDto } from './userTypes'

type UserAccessTabsProps = {
  userId: string
}

export function UserAccessTabs({ userId }: UserAccessTabsProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const [roles, setRoles] = useState<UserRoleDto[]>([])
  const [permissions, setPermissions] = useState<PermissionDto[]>([])
  const [isLoadingRoles, setIsLoadingRoles] = useState(false)
  const [isLoadingPermissions, setIsLoadingPermissions] = useState(false)

  useEffect(() => {
    let isCurrent = true

    async function loadRoles() {
      setIsLoadingRoles(true)

      try {
        const loadedRoles = await getUserRoles(userId)

        if (isCurrent) {
          setRoles(loadedRoles)
        }
      } catch (error) {
        if (isCurrent) {
          notifyError(error, t('shared.errors.generic'))
        }
      } finally {
        if (isCurrent) {
          setIsLoadingRoles(false)
        }
      }
    }

    async function loadPermissions() {
      setIsLoadingPermissions(true)

      try {
        const loadedPermissions = await getUserPermissions(userId)

        if (isCurrent) {
          setPermissions(loadedPermissions)
        }
      } catch (error) {
        if (isCurrent) {
          notifyError(error, t('shared.errors.generic'))
        }
      } finally {
        if (isCurrent) {
          setIsLoadingPermissions(false)
        }
      }
    }

    void loadRoles()
    void loadPermissions()

    return () => {
      isCurrent = false
    }
  }, [notifyError, t, userId])

  return (
    <Tabs defaultValue="roles">
      <TabsList>
        <TabsTrigger value="roles">{t('features.iam.users.tabs.roles')}</TabsTrigger>
        <TabsTrigger value="permissions">{t('features.iam.users.tabs.permissions')}</TabsTrigger>
      </TabsList>
      <TabsContent value="roles">
        <RoleList isLoading={isLoadingRoles} roles={roles} />
      </TabsContent>
      <TabsContent value="permissions">
        <PermissionList isLoading={isLoadingPermissions} permissions={permissions} />
      </TabsContent>
    </Tabs>
  )
}

type RoleListProps = {
  isLoading: boolean
  roles: UserRoleDto[]
}

function RoleList({ isLoading, roles }: RoleListProps) {
  const t = useTranslate()

  if (isLoading) {
    return <p className="page-subtitle">{t('shared.common.loading')}</p>
  }

  if (roles.length === 0) {
    return (
      <Empty>
        <EmptyDescription>{t('features.iam.users.messages.noRoles')}</EmptyDescription>
      </Empty>
    )
  }

  return (
    <div className="access-table-wrap">
      <table className="access-table">
        <thead>
          <tr>
            <th>{t('shared.fields.name')}</th>
            <th>{t('shared.fields.startsAt')}</th>
            <th>{t('shared.fields.expiresAt')}</th>
            <th>{t('shared.fields.assignedBy')}</th>
            <th>{t('shared.fields.assignedAt')}</th>
            <th>{t('shared.fields.isActive')}</th>
            <th>{t('shared.fields.isDefault')}</th>
          </tr>
        </thead>
        <tbody>
          {roles.map((role) => (
            <tr key={role.id}>
              <td>{role.name}</td>
              <td>{formatUserDateTime(role.startsAt)}</td>
              <td>{role.expiresAt ? formatUserDateTime(role.expiresAt) : '-'}</td>
              <td>{formatUserDateTime(role.assignedBy)}</td>
              <td>{formatUserDateTime(role.assignedAt)}</td>
              <td>
                <Badge variant={role.isActive ? 'active' : 'inactive'}>
                  {role.isActive ? t('shared.status.active') : t('shared.status.inactive')}
                </Badge>
              </td>
              <td>
                <Badge variant={role.isDefault ? 'active' : 'inactive'}>
                  {role.isDefault ? t('shared.common.yes') : t('shared.common.no')}
                </Badge>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}

type PermissionListProps = {
  isLoading: boolean
  permissions: PermissionDto[]
}

function PermissionList({ isLoading, permissions }: PermissionListProps) {
  const t = useTranslate()

  if (isLoading) {
    return <p className="page-subtitle">{t('shared.common.loading')}</p>
  }

  if (permissions.length === 0) {
    return (
      <Empty>
        <EmptyDescription>{t('features.iam.users.messages.noPermissions')}</EmptyDescription>
      </Empty>
    )
  }

  return (
    <div className="access-table-wrap">
      <table className="access-table">
        <thead>
          <tr>
            <th>{t('shared.fields.title')}</th>
            <th>{t('shared.fields.isActive')}</th>
          </tr>
        </thead>
        <tbody>
          {permissions.map((permission) => (
            <tr key={permission.id}>
              <td>{permission.title}</td>
              <td>
                <Badge variant={permission.isActive ? 'active' : 'inactive'}>
                  {permission.isActive ? t('shared.status.active') : t('shared.status.inactive')}
                </Badge>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
