import { EMAIL_STATUS_ENUM, NOTIFICATION_SEVERITY_ENUM, NOTIFICATION_STATUS_ENUM } from './enums'

export const courierMessages = {
  actions: {
    addTranslation: 'Add translation',
    markRead: 'Mark as read',
    openLink: 'Open link',
  },
  enums: {
    emailStatus: EMAIL_STATUS_ENUM,
    notificationSeverity: NOTIFICATION_SEVERITY_ENUM,
    notificationStatus: NOTIFICATION_STATUS_ENUM,
  },
  labels: {
    actionLink: 'Action link',
    allowOptOut: 'Allow recipients to opt out',
    attemptedAt: 'Attempted at',
    body: 'Body',
    communications: 'Communications',
    deliveryAttempts: 'Delivery attempts',
    emailTemplates: 'Email templates',
    isHtml: 'HTML',
    nextAttemptAt: 'Next attempt at',
    notification: 'Notification',
    notifications: 'Notifications',
    preferences: 'Preferences',
    readAt: 'Read at',
    recipient: 'Recipient',
    retryCount: 'Retry count',
    sentAt: 'Sent at',
    severity: 'Severity',
    subject: 'Subject',
    template: 'Template',
    templateKey: 'Template key',
    templates: 'Templates',
    translation: 'Translation',
    translations: 'Translations',
  },
  messages: {
    emails: {
      noAttempts: 'No delivery attempts recorded.',
    },
    templates: {
      deleteConfirm: 'Delete this template and all its translations?',
    },
    userPreferences: {
      communicationsHelp: 'Disable the communications you do not want to receive.',
      enableAllEmail: 'Receive email for all templates',
      enableAllNotification: 'Receive notifications for all templates',
      enableEmail: 'Receive email',
      enableNotification: 'Receive notification',
    },
    validation: {
      templates: {
        channelRequired: 'Enable email, notification, or both.',
        emailBody: 'Email body is required.',
        emailSubject: 'Email subject must contain at least 10 characters.',
        notificationMessage: 'Notification message is required.',
        notificationTitle: 'Notification title is required.',
      },
    },
  },
} as const
