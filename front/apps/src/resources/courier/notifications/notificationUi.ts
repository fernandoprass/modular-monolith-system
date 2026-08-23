import type { Translate } from '../../../app/i18n/i18n'
import { NOTIFICATION_STATUSES, type NotificationStatus } from './notificationTypes'

type NotificationOption = {
  labelKey: string
  value: string
}

const NOTIFICATION_STATUS_LABEL_KEYS: Record<NotificationStatus, string> = {
  [NOTIFICATION_STATUSES.unread]: 'features.courier.notifications.statuses.unread',
  [NOTIFICATION_STATUSES.read]: 'features.courier.notifications.statuses.read',
}

export function getNotificationStatusLabel(value: NotificationStatus, t: Translate): string {
  return t(NOTIFICATION_STATUS_LABEL_KEYS[value])
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
