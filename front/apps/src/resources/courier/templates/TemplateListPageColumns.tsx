import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { TemplateLiteDto } from './templateTypes'
import { getNotificationSeverityEnum, getRetentionPolicyEnum } from './templateUi'

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
      accessorKey: 'module',
      header: t('common.labels.module'),
    },
    {
      accessorKey: 'key',
      header: t('common.labels.key'),
    },
    {
      accessorKey: 'name',
      header: t('common.labels.name'),
    },
    {
      accessorKey: 'severity',
      cell: ({ row }) => getNotificationSeverityEnum(row.original.severity, t),
      header: t('features.courier.labels.severity'),
    },
    {
      accessorKey: 'retentionPolicy',
      cell: ({ row }) => getRetentionPolicyEnum(row.original.retentionPolicy, t),
      header: t('common.labels.retentionPolicy'),
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
            {
              isVisible: canDelete,
              kind: DATA_TABLE_ROW_ACTION_KINDS.delete,
              label: t('common.actions.delete'),
              onClick: () => onDelete(row.original),
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
