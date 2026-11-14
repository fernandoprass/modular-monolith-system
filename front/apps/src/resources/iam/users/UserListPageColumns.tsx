import { getLanguageLabel } from '../../../shared/translatedOptions'
import type { ColumnDef } from '@tanstack/react-table'
import type { NavigateFunction } from 'react-router-dom'

import type { Translate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { Badge } from '../../../components/ui/badge'
import { DATA_TABLE_ROW_ACTION_KINDS, DataTableRowActions } from '../../../components/ui/data-table-row-actions'
import type { UserLiteDto } from './userTypes'

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
      header: t('common.fields.name'),
    },
    {
      accessorKey: 'email',
      header: t('common.fields.email'),
    },
    {
      accessorKey: 'language',
      cell: ({ row }) => getLanguageLabel(row.original.language, t),
      header: t('common.fields.language'),
      sortingFn: (left, right) =>
        getLanguageLabel(left.original.language, t).localeCompare(getLanguageLabel(right.original.language, t)),
    },
    {
      accessorKey: 'isActive',
      cell: ({ row }) => (
        <Badge variant={row.original.isActive ? 'active' : 'inactive'}>
          {row.original.isActive ? t('common.status.active') : t('common.status.inactive')}
        </Badge>
      ),
      header: t('common.fields.isActive'),
    },
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              isVisible: canView,
              kind: DATA_TABLE_ROW_ACTION_KINDS.view,
              label: t('common.actions.view'),
              onClick: () => navigate(APP_ROUTES.userView(row.original.id)),
            },
            {
              isVisible: canUpdate,
              kind: DATA_TABLE_ROW_ACTION_KINDS.edit,
              label: t('common.actions.edit'),
              onClick: () => navigate(APP_ROUTES.userEdit(row.original.id)),
            },
            {
              isVisible: canDelete,
              kind: DATA_TABLE_ROW_ACTION_KINDS.delete,
              label: t('common.actions.delete'),
              onClick: () => setDeleteTarget(row.original),
            },
          ]}
        />
      ),
      enableHiding: false,
      enableSorting: false,
      header: t('common.fields.actions'),
      id: 'actions',
    },
  ]
}
