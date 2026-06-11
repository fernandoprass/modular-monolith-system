import type { ColumnDef } from '@tanstack/react-table'
import type { NavigateFunction } from 'react-router-dom'

import type { Translate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { Badge } from '../../../components/ui/badge'
import { DATA_TABLE_ROW_ACTION_KINDS, DataTableRowActions } from '../../../components/ui/data-table-row-actions'
import type { UserLiteDto } from './userTypes'
import { getLanguageLabel } from './userUi'

type UserTableColumnsOptions = {
  canDelete: boolean
  canUpdate: boolean
  canView: boolean
  navigate: NavigateFunction
  setDeleteTarget: (user: UserLiteDto) => void
  t: Translate
}

export function createUserTableColumns({
  canDelete,
  canUpdate,
  canView,
  navigate,
  setDeleteTarget,
  t,
}: UserTableColumnsOptions): ColumnDef<UserLiteDto>[] {
  return [
    {
      accessorKey: 'name',
      header: t('resources.iam.users.fields.name'),
    },
    {
      accessorKey: 'email',
      header: t('resources.iam.users.fields.email'),
    },
    {
      accessorKey: 'language',
      cell: ({ row }) => getLanguageLabel(row.original.language, t),
      header: t('resources.iam.users.fields.language'),
      sortingFn: (left, right) =>
        getLanguageLabel(left.original.language, t).localeCompare(getLanguageLabel(right.original.language, t)),
    },
    {
      accessorKey: 'isActive',
      cell: ({ row }) => (
        <Badge variant={row.original.isActive ? 'active' : 'inactive'}>
          {row.original.isActive ? t('shared.status.active') : t('shared.status.inactive')}
        </Badge>
      ),
      header: t('resources.iam.users.fields.isActive'),
    },
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              isVisible: canView,
              kind: DATA_TABLE_ROW_ACTION_KINDS.view,
              label: t('resources.iam.users.actions.view'),
              onClick: () => navigate(APP_ROUTES.userView(row.original.id)),
            },
            {
              isVisible: canUpdate,
              kind: DATA_TABLE_ROW_ACTION_KINDS.edit,
              label: t('resources.iam.users.actions.edit'),
              onClick: () => navigate(APP_ROUTES.userEdit(row.original.id)),
            },
            {
              isVisible: canDelete,
              kind: DATA_TABLE_ROW_ACTION_KINDS.delete,
              label: t('resources.iam.users.actions.delete'),
              onClick: () => setDeleteTarget(row.original),
            },
          ]}
        />
      ),
      enableHiding: false,
      enableSorting: false,
      header: t('resources.iam.users.fields.actions'),
      id: 'actions',
    },
  ]
}
