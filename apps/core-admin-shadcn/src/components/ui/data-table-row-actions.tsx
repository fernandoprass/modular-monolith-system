import { Edit, Eye, Trash2 } from 'lucide-react'
import type { ReactNode } from 'react'

import { Button } from './button'

export const DATA_TABLE_ROW_ACTION_KINDS = {
  delete: 'delete',
  edit: 'edit',
  view: 'view',
} as const

export const DATA_TABLE_ROW_ACTION_VARIANTS = {
  default: 'default',
  destructive: 'destructive',
  ghost: 'ghost',
  outline: 'outline',
} as const

type PresetDataTableRowActionKind = typeof DATA_TABLE_ROW_ACTION_KINDS[keyof typeof DATA_TABLE_ROW_ACTION_KINDS]
type DataTableRowActionVariant = typeof DATA_TABLE_ROW_ACTION_VARIANTS[keyof typeof DATA_TABLE_ROW_ACTION_VARIANTS]

type PresetDataTableRowAction = {
  isVisible?: boolean
  kind: PresetDataTableRowActionKind
  label: string
  onClick: () => void
}

type CustomDataTableRowAction = {
  icon: ReactNode
  isVisible?: boolean
  key: string
  label: string
  onClick: () => void
  variant?: DataTableRowActionVariant
}

type DataTableRowAction = CustomDataTableRowAction | PresetDataTableRowAction

type DataTableRowActionsProps = {
  actions: DataTableRowAction[]
}

const ICON_SIZE = 15

const PRESET_ICONS: Record<PresetDataTableRowActionKind, ReactNode> = {
  [DATA_TABLE_ROW_ACTION_KINDS.delete]: <Trash2 size={ICON_SIZE} />,
  [DATA_TABLE_ROW_ACTION_KINDS.edit]: <Edit size={ICON_SIZE} />,
  [DATA_TABLE_ROW_ACTION_KINDS.view]: <Eye size={ICON_SIZE} />,
}

function isCustomAction(action: DataTableRowAction): action is CustomDataTableRowAction {
  return 'key' in action
}

function getActionKey(action: DataTableRowAction): string {
  return isCustomAction(action) ? action.key : action.kind
}

function getActionIcon(action: DataTableRowAction): ReactNode {
  return isCustomAction(action) ? action.icon : PRESET_ICONS[action.kind]
}

function getActionVariant(action: DataTableRowAction): DataTableRowActionVariant {
  if (isCustomAction(action)) {
    return action.variant ?? DATA_TABLE_ROW_ACTION_VARIANTS.ghost
  }

  return DATA_TABLE_ROW_ACTION_VARIANTS.ghost
}

export function DataTableRowActions({ actions }: DataTableRowActionsProps) {
  const visibleActions = actions.filter((action) => action.isVisible ?? true)

  if (visibleActions.length === 0) {
    return null
  }

  return (
    <div className="row-actions">
      {visibleActions.map((action) => (
        <Button
          key={getActionKey(action)}
          onClick={action.onClick}
          size="icon"
          title={action.label}
          type="button"
          variant={getActionVariant(action)}
        >
          {getActionIcon(action)}
        </Button>
      ))}
    </div>
  )
}
