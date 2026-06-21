import * as SelectPrimitive from '@radix-ui/react-select'
import { Check, ChevronDown } from 'lucide-react'
import type { ComponentProps } from 'react'

import { cn } from '../../lib/utils'

export type SelectOption = {
  label: string
  value: string
}

type SelectProps = {
  disabled?: boolean
  onValueChange: (value: string) => void
  options: SelectOption[]
  placeholder?: string
  value?: string
}

export function Select({ disabled = false, onValueChange, options, placeholder, value }: SelectProps) {
  const selectedOption = options.find((option) => option.value === value)

  return (
    <SelectPrimitive.Root disabled={disabled} onValueChange={onValueChange} value={value}>
      <SelectPrimitive.Trigger className="select-trigger">
        <span>{selectedOption?.label ?? placeholder}</span>
        <SelectPrimitive.Icon>
          <ChevronDown />
        </SelectPrimitive.Icon>
      </SelectPrimitive.Trigger>
      <SelectPrimitive.Portal>
        <SelectPrimitive.Content className="select-content" position="popper">
          <SelectPrimitive.Viewport>
            <SelectPrimitive.Group>
              {options.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectPrimitive.Group>
          </SelectPrimitive.Viewport>
        </SelectPrimitive.Content>
      </SelectPrimitive.Portal>
    </SelectPrimitive.Root>
  )
}

function SelectItem({ children, className, ...props }: ComponentProps<typeof SelectPrimitive.Item>) {
  return (
    <SelectPrimitive.Item className={cn('select-item', className)} {...props}>
      <SelectPrimitive.ItemText>{children}</SelectPrimitive.ItemText>
      <SelectPrimitive.ItemIndicator className="select-indicator">
        <Check />
      </SelectPrimitive.ItemIndicator>
    </SelectPrimitive.Item>
  )
}
