import type { Translate } from '../../../app/i18n/i18n'
import { EMAIL_STATUSES, type EmailStatus } from './emailTypes'

type EmailOption = {
  labelKey: string
  value: string
}

const EMAIL_STATUS_LABEL_KEYS: Record<EmailStatus, string> = {
  [EMAIL_STATUSES.pending]: 'features.courier.emails.statuses.pending',
  [EMAIL_STATUSES.processing]: 'features.courier.emails.statuses.processing',
  [EMAIL_STATUSES.sent]: 'features.courier.emails.statuses.sent',
  [EMAIL_STATUSES.failed]: 'features.courier.emails.statuses.failed',
}

export function getEmailStatusLabel(value: EmailStatus, t: Translate): string {
  return t(EMAIL_STATUS_LABEL_KEYS[value])
}

export function getEmailStatusClassName(value: EmailStatus): string {
  if (value === EMAIL_STATUSES.sent) {
    return 'email-status-sent'
  }

  if (value === EMAIL_STATUSES.failed) {
    return 'email-status-failed'
  }

  if (value === EMAIL_STATUSES.processing) {
    return 'email-status-processing'
  }

  return 'email-status-pending'
}

export function toTranslatedEmailOptions(options: readonly EmailOption[], t: Translate) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}
