import * as CheckboxPrimitive from '@radix-ui/react-checkbox'
import { Check } from 'lucide-react'
import type { ComponentProps } from 'react'

import { cn } from '../../lib/utils'

type CheckboxProps = ComponentProps<typeof CheckboxPrimitive.Root> & {
  label: string
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
          <Check data-icon="inline-start" />
        </CheckboxPrimitive.Indicator>
      </CheckboxPrimitive.Root>
      <span>{label}</span>
    </label>
  )
}
