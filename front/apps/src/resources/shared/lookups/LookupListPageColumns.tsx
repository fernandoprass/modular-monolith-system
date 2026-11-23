import type { ColumnDef } from '@tanstack/react-table'
import type { Translate } from '../../../app/i18n/i18n'
import type { LookupLiteDto } from './lookupTypes'
import { getLookupTypeEnum } from './lookupUi'

export function createLookupTableColumns(t: Translate): ColumnDef<LookupLiteDto>[] {
  return [
    { accessorKey: 'type', header: t('common.labels.type'), cell: ({ row }) => getLookupTypeEnum(row.original.type, t), enableSorting: false },
    { accessorKey: 'code', header: t('common.labels.code'), enableSorting: false },
    { accessorKey: 'value', header: t('common.labels.value'), enableSorting: false },
    { accessorKey: 'description', header: t('common.labels.description'), enableSorting: false },
    { accessorKey: 'languageCode', header: t('common.labels.language'), enableSorting: false },
  ]
}
