import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { TemplateLiteDto } from './templateTypes'
import { getRetentionPolicyLabel, getTemplateTypeLabel } from './templateUi'

type CreateTemplateTableColumnsRequest = {
  canDelete: boolean
  canUpdate: boolean
  onDelete: (template: TemplateLiteDto) => void
  onEdit: (template: TemplateLiteDto) => void
  t: Translate
}

export function createTemplateTableColumns({
  canDelete,
  canUpdate,
  onDelete,
  onEdit,
  t,
}: CreateTemplateTableColumnsRequest): ColumnDef<TemplateLiteDto>[] {
  return [
    {
      accessorKey: 'key',
      header: t('shared.fields.key'),
    },
    {
      accessorKey: 'name',
      header: t('shared.fields.name'),
    },
    {
      accessorKey: 'type',
      cell: ({ row }) => getTemplateTypeLabel(row.original.type, t),
      header: t('shared.fields.type'),
    },
    {
      accessorKey: 'retentionPolicy',
      cell: ({ row }) => getRetentionPolicyLabel(row.original.retentionPolicy, t),
      header: t('shared.fields.retentionPolicy'),
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
      enableHiding: false,
      enableSorting: false,
      header: t('shared.fields.actions'),
      id: 'actions',
    },
  ]
}
