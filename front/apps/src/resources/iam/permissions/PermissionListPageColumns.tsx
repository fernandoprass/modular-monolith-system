import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import { Badge } from '../../../components/ui/badge'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { PermissionDto } from '../../../shared/permissions'

type CreatePermissionTableColumnsRequest = {
  canUpdate: boolean
  onEdit: (permission: PermissionDto) => void
  t: Translate
}

export function createPermissionTableColumns({
  canUpdate,
  onEdit,
  t,
}: CreatePermissionTableColumnsRequest): ColumnDef<PermissionDto>[] {
  return [
    {
      accessorKey: 'module',
      header: t('common.labels.module'),
    },
    {
      accessorKey: 'resource',
      header: t('common.labels.resource'),
    },
    {
      accessorKey: 'action',
      header: t('common.labels.action'),
    },
    {
      accessorKey: 'title',
      header: t('common.labels.title'),
    },
    {
      accessorKey: 'isActive',
      cell: ({ row }) => (
        <Badge variant={row.original.isActive ? 'active' : 'inactive'}>
          {row.original.isActive ? t('common.states.active') : t('common.states.inactive')}
        </Badge>
      ),
      header: t('common.labels.isActive'),
    },
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              isVisible: canUpdate,
              kind: DATA_TABLE_ROW_ACTION_KINDS.edit,
              label: t('common.actions.edit'),
              onClick: () => onEdit(row.original),
            },
          ]}
        />
      ),
      enableHiding: false,
      enableSorting: false,
      header: t('common.labels.actions'),
      id: 'actions',
    },
  ]
}
