import type { PlainTranslationKey, Translate } from '../../../app/i18n/i18n'
import { EMAIL_STATUSES, type EmailStatus } from './emailTypes'

const EMAIL_STATUS_ENUM_KEYS: Record<EmailStatus, PlainTranslationKey> = {
  [EMAIL_STATUSES.pending]: 'features.courier.enums.emailStatus.pending',
  [EMAIL_STATUSES.processing]: 'features.courier.enums.emailStatus.processing',
  [EMAIL_STATUSES.sent]: 'features.courier.enums.emailStatus.sent',
  [EMAIL_STATUSES.failed]: 'features.courier.enums.emailStatus.failed',
}

export function getEmailStatusEnum(value: EmailStatus, t: Translate): string {
  return t(EMAIL_STATUS_ENUM_KEYS[value])
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
