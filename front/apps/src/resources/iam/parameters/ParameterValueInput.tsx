import type { ChangeEvent, AriaAttributes } from 'react'

import { useTranslate } from '../../../app/i18n/i18n'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { PARAMETER_TYPE_VALUES } from './parameterTypes'

type ParameterValueInputProps = Pick<AriaAttributes, 'aria-invalid' | 'aria-describedby'> & {
  disabled?: boolean
  id: string
  onBlur: () => void
  onChange: (value: string) => void
  type: string
  value: string
}

function handleInputChange(onChange: (value: string) => void) {
  return (event: ChangeEvent<HTMLInputElement>) => onChange(event.currentTarget.value)
}

function handleTextareaChange(onChange: (value: string) => void) {
  return (event: ChangeEvent<HTMLTextAreaElement>) => onChange(event.currentTarget.value)
}

export function ParameterValueInput({
  disabled = false,
  id,
  onBlur,
  onChange,
  type,
  value,
  ...ariaProps
}: ParameterValueInputProps) {
  const t = useTranslate()

  if (type === PARAMETER_TYPE_VALUES.boolean) {
    return (
      <Select
        {...ariaProps}
        id={id}
        onBlur={onBlur}
        disabled={disabled}
        onValueChange={onChange}
        options={[
          { label: t('common.states.true'), value: 'true' },
          { label: t('common.states.false'), value: 'false' },
        ]}
        value={value.toLowerCase() === 'false' ? 'false' : 'true'}
      />
    )
  }

  if (type === PARAMETER_TYPE_VALUES.integer) {
    return (
      <Input
        {...ariaProps}
        id={id}
        disabled={disabled}
        onBlur={onBlur}
        onChange={handleInputChange(onChange)}
        required
        step={1}
        type="number"
        value={value}
      />
    )
  }

  if (type === PARAMETER_TYPE_VALUES.decimal) {
    return (
      <Input
        {...ariaProps}
        id={id}
        disabled={disabled}
        onBlur={onBlur}
        onChange={handleInputChange(onChange)}
        required
        step="any"
        type="number"
        value={value}
      />
    )
  }

  if (type === PARAMETER_TYPE_VALUES.dateTime) {
    return (
      <Input
        {...ariaProps}
        id={id}
        disabled={disabled}
        onBlur={onBlur}
        onChange={handleInputChange(onChange)}
        required
        type="datetime-local"
        value={value}
      />
    )
  }

  if (type === PARAMETER_TYPE_VALUES.date) {
    return (
      <Input
        {...ariaProps}
        id={id}
        disabled={disabled}
        onBlur={onBlur}
        onChange={handleInputChange(onChange)}
        required
        type="date"
        value={value}
      />
    )
  }

  if (type === PARAMETER_TYPE_VALUES.time) {
    return (
      <Input
        {...ariaProps}
        id={id}
        disabled={disabled}
        onBlur={onBlur}
        onChange={handleInputChange(onChange)}
        required
        type="time"
        value={value}
      />
    )
  }

  if (type === PARAMETER_TYPE_VALUES.character) {
    return (
      <Input
        {...ariaProps}
        id={id}
        disabled={disabled}
        maxLength={1}
        onBlur={onBlur}
        onChange={handleInputChange(onChange)}
        required
        value={value}
      />
    )
  }

  if (type === PARAMETER_TYPE_VALUES.text || type === PARAMETER_TYPE_VALUES.richText) {
    return (
      <Textarea
        {...ariaProps}
        id={id}
        disabled={disabled}
        onBlur={onBlur}
        onChange={handleTextareaChange(onChange)}
        required
        value={value}
      />
    )
  }

  return (
    <Input
        {...ariaProps}
      id={id}
      disabled={disabled}
      onBlur={onBlur}
      onChange={handleInputChange(onChange)}
      required
      value={value}
    />
  )
}
