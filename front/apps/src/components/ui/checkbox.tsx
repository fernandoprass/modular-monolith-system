import * as CheckboxPrimitive from '@radix-ui/react-checkbox'
import { Check, Minus } from 'lucide-react'
import type { ComponentProps } from 'react'

import { cn } from '../../lib/utils'

type CheckboxProps = ComponentProps<typeof CheckboxPrimitive.Root> & {
  label?: string
}

export function Checkbox({ className, label, onCheckedChange, ...props }: CheckboxProps) {
  return (
    <label className="checkbox-row">
      <CheckboxPrimitive.Root
        className={cn('checkbox', className)}
        onCheckedChange={(value) => onCheckedChange?.(value === true)}
        {...props}
      >
        <CheckboxPrimitive.Indicator className="checkbox-indicator">
          {props.checked === 'indeterminate' ? (
            <Minus data-icon="inline-start" />
          ) : (
            <Check data-icon="inline-start" />
          )}
        </CheckboxPrimitive.Indicator>
      </CheckboxPrimitive.Root>
      {label !== undefined && label.length > 0 && <span>{label}</span>}
    </label>
  )
}
