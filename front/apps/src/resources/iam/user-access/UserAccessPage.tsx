import type { ColumnDef, RowSelectionState, SortingState } from '@tanstack/react-table'
import { Minus, Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Badge } from '../../../components/ui/badge'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { DataTable } from '../../../components/ui/data-table'
import { Empty, EmptyDescription } from '../../../components/ui/empty'
import { Field, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { formatUserDateTime } from '../../../shared/dateFormat'
import type { RoleDto } from '../roles/roleTypes'
import { UserSelect } from '../users/UserSelect'
import type { UserRoleDto } from '../users/userTypes'
import {
  assignUserRoles,
  getAssignedUserRoles,
  getAvailableUserRoles,
  unassignUserRoles,
} from './userAccessApi'

function todayDateTimeLocal(): string {
  const now = new Date()

  const year = now.getFullYear()
  const month = String(now.getMonth() + 1).padStart(2, '0')
  const day = String(now.getDate()).padStart(2, '0')
  const hours = String(now.getHours()).padStart(2, '0')
  const minutes = String(now.getMinutes()).padStart(2, '0')

  return `${year}-${month}-${day}T${hours}:${minutes}`
}

function dateTimeLocalToUtcIso(value: string): string {
  return new Date(value).toISOString()
}

function getSelectedIds(selection: RowSelectionState): string[] {
  return Object.entries(selection)
    .filter(([, selected]) => selected)
    .map(([id]) => id)
}

export function UserAccessPage() {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { user } = useAuth()
  const userLanguage = user?.language
  const [userId, setUserId] = useState('')
  const [availableRoles, setAvailableRoles] = useState<RoleDto[]>([])
  const [assignedRoles, setAssignedRoles] = useState<UserRoleDto[]>([])
  const [availableSelection, setAvailableSelection] = useState<RowSelectionState>({})
  const [assignedSelection, setAssignedSelection] = useState<RowSelectionState>({})
  const [availableSorting, setAvailableSorting] = useState<SortingState>([])
  const [assignedSorting, setAssignedSorting] = useState<SortingState>([])
  const [startsAt, setStartsAt] = useState(todayDateTimeLocal)
  const [expiresAt, setExpiresAt] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const availableRoleColumns = useMemo<ColumnDef<RoleDto>[]>(() => [
    {
      accessorKey: 'name',
      header: t('shared.fields.name'),
    },
    {
      accessorKey: 'isActive',
      cell: ({ row }) => (
        <Badge variant={row.original.isActive ? 'active' : 'inactive'}>
          {row.original.isActive ? t('shared.status.active') : t('shared.status.inactive')}
        </Badge>
      ),
      header: t('shared.fields.isActive'),
    },
    {
      accessorKey: 'isDefault',
      cell: ({ row }) => (
        <Badge variant={row.original.isDefault ? 'active' : 'inactive'}>
          {row.original.isDefault ? t('shared.common.yes') : t('shared.common.no')}
        </Badge>
      ),
      header: t('shared.fields.isDefault'),
    },
  ], [t])
  const assignedRoleColumns = useMemo<ColumnDef<UserRoleDto>[]>(() => [
    {
      accessorKey: 'name',
      header: t('shared.fields.name'),
    },
    {
      accessorKey: 'startsAt',
      cell: ({ row }) => formatUserDateTime(row.original.startsAt, userLanguage),
      header: t('shared.fields.startsAt'),
    },
    {
      accessorKey: 'expiresAt',
      cell: ({ row }) => row.original.expiresAt ? formatUserDateTime(row.original.expiresAt, userLanguage) : '-',
      header: t('shared.fields.expiresAt'),
    },
    {
      accessorKey: 'isActive',
      cell: ({ row }) => (
        <Badge variant={row.original.isActive ? 'active' : 'inactive'}>
          {row.original.isActive ? t('shared.status.active') : t('shared.status.inactive')}
        </Badge>
      ),
      header: t('shared.fields.isActive'),
    },
  ], [t, userLanguage])

  const loadUserAccess = useCallback(async () => {
    if (userId.length === 0) {
      setAvailableRoles([])
      setAssignedRoles([])
      return
    }

    setIsLoading(true)

    try {
      const [available, assigned] = await Promise.all([
        getAvailableUserRoles(userId),
        getAssignedUserRoles(userId),
      ])

      setAvailableRoles(available)
      setAssignedRoles(assigned)
      setAvailableSelection({})
      setAssignedSelection({})
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsLoading(false)
    }
  }, [notifyError, t, userId])

  useEffect(() => {
    void loadUserAccess()
  }, [loadUserAccess])

  async function handleAssignRoles() {
    const roleIds = getSelectedIds(availableSelection)

    if (userId.length === 0 || roleIds.length === 0) {
      return
    }

    setIsSaving(true)

    try {
      await assignUserRoles(
        userId,
        dateTimeLocalToUtcIso(startsAt),
        expiresAt.length === 0 ? null : dateTimeLocalToUtcIso(expiresAt),
        roleIds,
      )
      showSuccess(t('features.iam.userAccess.notifications.rolesAssigned'))
      await loadUserAccess()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  async function handleUnassignRoles() {
    const roleIds = getSelectedIds(assignedSelection)

    if (userId.length === 0 || roleIds.length === 0) {
      return
    }

    setIsSaving(true)

    try {
      await unassignUserRoles(userId, roleIds)
      showSuccess(t('features.iam.userAccess.notifications.rolesUnassigned'))
      await loadUserAccess()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.userAccess.pages.list')}</h1>
      </div>
      <Card>
        <CardContent>
          <Field>
            <FieldLabel>{t('shared.fields.user')}</FieldLabel>
            <UserSelect clearable includeInactive onValueChange={setUserId} value={userId} />
          </Field>
        </CardContent>
      </Card>
      {userId.length === 0 ? (
        <Card>
          <CardContent>
            <Empty>
              <EmptyDescription>{t('features.iam.userAccess.messages.noUserSelected')}</EmptyDescription>
            </Empty>
          </CardContent>
        </Card>
      ) : (
        <Card>
          <CardContent>
            <div className="permission-assignment-grid">
              <div className="permission-table-column">
                <h2 className="card-title">{t('features.iam.userAccess.labels.availableRoles')}</h2>
                <DataTable
                  columns={availableRoleColumns}
                  data={availableRoles}
                  emptyText={t('features.iam.userAccess.messages.noRoles')}
                  getRowId={(role) => role.id}
                  isLoading={isLoading}
                  loadingText={t('shared.common.loading')}
                  onRowSelectionChange={setAvailableSelection}
                  onSortingChange={setAvailableSorting}
                  rowSelection={availableSelection}
                  sorting={availableSorting}
                />
              </div>
              <div className="permission-action-column user-access-action-column">
                <Field>
                  <FieldLabel>{t('shared.fields.startsAt')}</FieldLabel>
                  <Input onChange={(event) => setStartsAt(event.currentTarget.value)} type="datetime-local" value={startsAt} />
                </Field>
                <Field>
                  <FieldLabel>{t('shared.fields.expiresAt')}</FieldLabel>
                  <Input onChange={(event) => setExpiresAt(event.currentTarget.value)} type="datetime-local" value={expiresAt} />
                </Field>
                <Button
                  disabled={isSaving || getSelectedIds(availableSelection).length === 0}
                  onClick={() => void handleAssignRoles()}
                  type="button"
                >
                  <Plus data-icon="inline-start" />
                  {t('shared.actions.add')}
                </Button>
                <Button
                  disabled={isSaving || getSelectedIds(assignedSelection).length === 0}
                  onClick={() => void handleUnassignRoles()}
                  type="button"
                  variant="outline"
                >
                  <Minus data-icon="inline-start" />
                  {t('shared.actions.remove')}
                </Button>
              </div>
              <div className="permission-table-column">
                <h2 className="card-title">{t('features.iam.userAccess.labels.assignedRoles')}</h2>
                <DataTable
                  columns={assignedRoleColumns}
                  data={assignedRoles}
                  emptyText={t('features.iam.userAccess.messages.noRoles')}
                  getRowId={(role) => role.roleId}
                  isLoading={isLoading}
                  loadingText={t('shared.common.loading')}
                  onRowSelectionChange={setAssignedSelection}
                  onSortingChange={setAssignedSorting}
                  rowSelection={assignedSelection}
                  sorting={assignedSorting}
                />
              </div>
            </div>
          </CardContent>
        </Card>
      )}
    </main>
  )
}
