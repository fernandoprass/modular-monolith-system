import type { PlainTranslationKey, Translate } from '../../../app/i18n/i18n'
import { NOTIFICATION_STATUSES, type NotificationStatus } from './notificationTypes'

const NOTIFICATION_STATUS_ENUM_KEYS: Record<NotificationStatus, PlainTranslationKey> = {
  [NOTIFICATION_STATUSES.unread]: 'features.courier.enums.notificationStatus.unread',
  [NOTIFICATION_STATUSES.read]: 'features.courier.enums.notificationStatus.read',
}

export function getNotificationStatusEnum(value: NotificationStatus, t: Translate): string {
  return t(NOTIFICATION_STATUS_ENUM_KEYS[value])
}

export function getNotificationStatusClassName(value: NotificationStatus): string {
  return value === NOTIFICATION_STATUSES.read ? 'notification-status-read' : 'notification-status-unread'
}
