import { z } from 'zod'
import type { Translate } from '../../../app/i18n/messages'
import { PARAMETER_TYPE_VALUES } from './parameterTypes'

export function createParameterEditSchema(t: Translate) {
  return z.object({
    description: z.string().trim().min(1),
    externalListEndpoint: z.string(),
    group: z.string().trim().min(1),
    isVisible: z.boolean(),
    listItems: z.string(),
    module: z.string().trim().min(1),
    name: z.string().trim().min(1),
    overrideType: z.string().trim().min(1),
    title: z.string().trim().min(1),
    type: z.string().trim().min(1),
    value: z.string().min(1),
  }).superRefine((value, context) => {
    if (value.type === PARAMETER_TYPE_VALUES.list && !value.listItems.trim()) {
      context.addIssue({ code: 'custom', path: ['listItems'], message: t('common.validation.required') })
    }
    if (value.type === PARAMETER_TYPE_VALUES.referenceId && !value.externalListEndpoint.trim()) {
      context.addIssue({ code: 'custom', path: ['externalListEndpoint'], message: t('common.validation.required') })
    }
    if (!value.value) return

    const number = Number(value.value)
    const invalidInteger = value.type === PARAMETER_TYPE_VALUES.integer && !Number.isInteger(number)
    const invalidNumber = (value.type === PARAMETER_TYPE_VALUES.integer || value.type === PARAMETER_TYPE_VALUES.decimal)
      && (!value.value.trim() || !Number.isFinite(number))
    const invalidCharacter = value.type === PARAMETER_TYPE_VALUES.character && value.value.length > 1
    if (invalidInteger || invalidNumber || invalidCharacter) {
      context.addIssue({ code: 'custom', path: ['value'], message: t('common.validation.invalid') })
    }
  })
}
