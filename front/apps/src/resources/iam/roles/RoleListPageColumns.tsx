import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import { Badge } from '../../../components/ui/badge'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { RoleDto } from './roleTypes'

type CreateRoleTableColumnsRequest = {
  canDelete: boolean
  canUpdate: boolean
  onDelete: (role: RoleDto) => void
  onEdit: (role: RoleDto) => void
  t: Translate
}

export function createRoleTableColumns({
  canDelete,
  canUpdate,
  onDelete,
  onEdit,
  t,
}: CreateRoleTableColumnsRequest): ColumnDef<RoleDto>[] {
  return [
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
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              isVisible: canUpdate,
              kind: DATA_TABLE_ROW_ACTION_KINDS.edit,
              label: t('shared.actions.edit'),
              onClick: () => onEdit(row.original),
            },
            {
              isVisible: canDelete,
              kind: DATA_TABLE_ROW_ACTION_KINDS.delete,
              label: t('shared.actions.delete'),
              onClick: () => onDelete(row.original),
            },
          ]}
        />
      ),
      enableSorting: false,
      header: t('shared.fields.actions'),
      id: 'actions',
    },
  ]
}
