import type { z } from 'zod'

import type { Translate } from '../app/i18n/i18n'

/** Per-parse messages leave schema-specific messages and backend errors untouched. */
export function createValidationErrorMap(t: Translate): z.core.$ZodErrorMap {
  return (issue) => {
    if (issue.code === 'invalid_type' && (issue.input === undefined || issue.input === null)) {
      return t('common.validation.required')
    }

    if (issue.code === 'invalid_format' && issue.format === 'email') {
      return t('common.validation.email')
    }

    if (issue.code === 'too_small' && issue.origin === 'string' && issue.inclusive) {
      return issue.minimum === 1
        ? t('common.validation.required')
        : t('common.validation.minLength', { min: String(issue.minimum) })
    }

    if (issue.code === 'too_big' && issue.origin === 'string' && issue.inclusive) {
      return t('common.validation.maxLength', { max: String(issue.maximum) })
    }

    return t('common.validation.invalid')
  }
}
