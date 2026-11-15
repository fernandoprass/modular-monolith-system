import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { ParameterLiteDto } from './parameterTypes'

type CreateParameterTableColumnsRequest = {
  canUpdate: boolean
  onEdit: (parameter: ParameterLiteDto) => void
  t: Translate
}

export function createParameterTableColumns({
  canUpdate,
  onEdit,
  t,
}: CreateParameterTableColumnsRequest): ColumnDef<ParameterLiteDto>[] {
  return [
    {
      accessorKey: 'module',
      header: t('common.fields.module'),
    },
    {
      accessorKey: 'group',
      header: t('common.fields.group'),
    },
    {
      accessorKey: 'name',
      header: t('common.fields.name'),
    },
    {
      accessorKey: 'title',
      header: t('common.fields.title'),
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
      header: t('common.fields.actions'),
      id: 'actions',
    },
  ]
}
