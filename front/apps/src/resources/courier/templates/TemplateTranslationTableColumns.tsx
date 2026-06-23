import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import { Badge } from '../../../components/ui/badge'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { TemplateEmailTranslationDto } from './templateTypes'

type CreateTemplateTranslationColumnsRequest = {
  canWrite: boolean
  onDelete: (translation: TemplateEmailTranslationDto) => void
  onEdit: (translation: TemplateEmailTranslationDto) => void
  t: Translate
}

export function createTemplateTranslationColumns({
  canWrite,
  onDelete,
  onEdit,
  t,
}: CreateTemplateTranslationColumnsRequest): ColumnDef<TemplateEmailTranslationDto>[] {
  return [
    {
      accessorKey: 'language',
      header: t('shared.fields.language'),
    },
    {
      accessorKey: 'subject',
      header: t('shared.fields.subject'),
    },
    {
      accessorKey: 'isHtml',
      cell: ({ row }) => (
        <Badge variant={row.original.isHtml ? 'active' : 'inactive'}>
          {row.original.isHtml
            ? t('features.courier.templates.formats.html')
            : t('features.courier.templates.formats.text')}
        </Badge>
      ),
      header: t('shared.fields.format'),
    },
    {
      cell: ({ row }) => (
        <DataTableRowActions
          actions={[
            {
              isVisible: canWrite,
              kind: DATA_TABLE_ROW_ACTION_KINDS.edit,
              label: t('shared.actions.edit'),
              onClick: () => onEdit(row.original),
            },
            {
              isVisible: canWrite,
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
