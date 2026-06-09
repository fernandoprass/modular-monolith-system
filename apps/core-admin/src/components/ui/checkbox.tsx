import * as CheckboxPrimitive from '@radix-ui/react-checkbox'
import { Check } from 'lucide-react'

type CheckboxProps = {
  checked: boolean
  label: string
  onCheckedChange: (checked: boolean) => void
}

export function Checkbox({ checked, label, onCheckedChange }: CheckboxProps) {
  return (
    <label className="checkbox-row">
      <CheckboxPrimitive.Root
        checked={checked}
        className="checkbox"
        onCheckedChange={(value) => onCheckedChange(value === true)}
      >
        <CheckboxPrimitive.Indicator>
          <Check size={13} />
        </CheckboxPrimitive.Indicator>
      </CheckboxPrimitive.Root>
      <span>{label}</span>
    </label>
  )
}
