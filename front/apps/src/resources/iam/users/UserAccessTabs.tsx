import { useEffect, useState } from 'react'

import { useLocale, useTranslate } from '../../../app/i18n/i18n'
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
          notifyError(error, t('common.messages.errors.generic'))
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
          notifyError(error, t('common.messages.errors.generic'))
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
        <TabsTrigger value="roles">{t('features.iam.labels.roles')}</TabsTrigger>
        <TabsTrigger value="permissions">{t('features.iam.labels.permissions')}</TabsTrigger>
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
  const locale = useLocale()

  if (isLoading) {
    return <p className="page-subtitle">{t('common.states.loading')}</p>
  }

  if (roles.length === 0) {
    return (
      <Empty>
        <EmptyDescription>{t('common.states.noResults', { entities: t('app.entities.roles') })}</EmptyDescription>
      </Empty>
    )
  }

  return (
    <div className="access-table-wrap">
      <table className="access-table">
        <thead>
          <tr>
            <th>{t('common.labels.name')}</th>
            <th>{t('common.labels.startsAt')}</th>
            <th>{t('common.labels.expiresAt')}</th>
            <th>{t('features.iam.labels.assignedBy')}</th>
            <th>{t('features.iam.labels.assignedAt')}</th>
            <th>{t('common.labels.isActive')}</th>
            <th>{t('common.labels.isDefault')}</th>
          </tr>
        </thead>
        <tbody>
          {roles.map((role) => (
            <tr key={role.id}>
              <td>{role.name}</td>
              <td>{formatUserDateTime(role.startsAt, locale)}</td>
              <td>{role.expiresAt ? formatUserDateTime(role.expiresAt, locale) : '-'}</td>
              <td>{role.assignedBy}</td>
              <td>{formatUserDateTime(role.assignedAt, locale)}</td>
              <td>
                <Badge variant={role.isActive ? 'active' : 'inactive'}>
                  {role.isActive ? t('common.states.active') : t('common.states.inactive')}
                </Badge>
              </td>
              <td>
                <Badge variant={role.isDefault ? 'active' : 'inactive'}>
                  {role.isDefault ? t('common.states.yes') : t('common.states.no')}
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
    return <p className="page-subtitle">{t('common.states.loading')}</p>
  }

  if (permissions.length === 0) {
    return (
      <Empty>
        <EmptyDescription>{t('common.states.noResults', { entities: t('app.entities.permissions') })}</EmptyDescription>
      </Empty>
    )
  }

  return (
    <div className="access-table-wrap">
      <table className="access-table">
        <thead>
          <tr>
            <th>{t('common.labels.title')}</th>
            <th>{t('common.labels.isActive')}</th>
          </tr>
        </thead>
        <tbody>
          {permissions.map((permission) => (
            <tr key={permission.id}>
              <td>{permission.title}</td>
              <td>
                <Badge variant={permission.isActive ? 'active' : 'inactive'}>
                  {permission.isActive ? t('common.states.active') : t('common.states.inactive')}
                </Badge>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
