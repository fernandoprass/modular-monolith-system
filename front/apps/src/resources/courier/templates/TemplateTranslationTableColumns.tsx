import type { ColumnDef } from '@tanstack/react-table'

import type { Translate } from '../../../app/i18n/i18n'
import { Badge } from '../../../components/ui/badge'
import {
  DATA_TABLE_ROW_ACTION_KINDS,
  DataTableRowActions,
} from '../../../components/ui/data-table-row-actions'
import type { TemplateTranslationDto } from './templateTypes'

type CreateTemplateTranslationColumnsRequest = {
  canWrite: boolean
  onDelete: (translation: TemplateTranslationDto) => void
  onEdit: (translation: TemplateTranslationDto) => void
  t: Translate
}

function ChannelBadge({ isConfigured, t }: { isConfigured: boolean; t: Translate }) {
  return (
    <Badge variant={isConfigured ? 'active' : 'inactive'}>
      {t(isConfigured
        ? 'features.courier.templates.channels.configured'
        : 'features.courier.templates.channels.notConfigured')}
    </Badge>
  )
}

export function createTemplateTranslationColumns({
  canWrite,
  onDelete,
  onEdit,
  t,
}: CreateTemplateTranslationColumnsRequest): ColumnDef<TemplateTranslationDto>[] {
  return [
    {
      accessorKey: 'language',
      header: t('shared.fields.language'),
    },
    {
      accessorKey: 'name',
      header: t('shared.fields.name'),
    },
    {
      accessorKey: 'email',
      cell: ({ row }) => <ChannelBadge isConfigured={row.original.email !== null} t={t} />,
      header: t('shared.fields.email'),
    },
    {
      accessorKey: 'notification',
      cell: ({ row }) => <ChannelBadge isConfigured={row.original.notification !== null} t={t} />,
      header: t('features.courier.templates.channels.notification'),
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
