import type { PlainTranslationKey, Translate } from '../../../app/i18n/i18n'
import { NOTIFICATION_STATUSES, type NotificationStatus } from './notificationTypes'

type NotificationOption = {
  labelKey: PlainTranslationKey
  value: string
}

const NOTIFICATION_STATUS_ENUM_KEYS: Record<NotificationStatus, PlainTranslationKey> = {
  [NOTIFICATION_STATUSES.unread]: 'features.courier.notifications.statuses.unread',
  [NOTIFICATION_STATUSES.read]: 'features.courier.notifications.statuses.read',
}

export function getNotificationStatusEnum(value: NotificationStatus, t: Translate): string {
  return t(NOTIFICATION_STATUS_ENUM_KEYS[value])
}

export function getNotificationStatusClassName(value: NotificationStatus): string {
  return value === NOTIFICATION_STATUSES.read ? 'notification-status-read' : 'notification-status-unread'
}

export function toTranslatedNotificationOptions(options: readonly NotificationOption[], t: Translate) {
  return options.map((option) => ({
    label: t(option.labelKey),
    value: option.value,
  }))
}
