import { RESOURCE_LABELS } from './app'
import { EMAIL_STATUS_ENUM, NOTIFICATION_STATUS_ENUM } from './enums'

const NOTIFICATION_LABEL = 'Notifications'

export const courierMessages = {
  emails: {
    actions: {
      create: 'Create email',
    },
    messages: {
      empty: 'No emails found.',
      noAttempts: 'No delivery attempts recorded.',
    },
    name: RESOURCE_LABELS.courier.emails,
    pages: {
      create: 'Create email',
      list: RESOURCE_LABELS.courier.emails,
      show: 'Email details',
    },
    sections: {
      attempts: 'Delivery attempts',
    },
    statuses: EMAIL_STATUS_ENUM,
  },
  notifications: {
    actions: {
      markRead: 'Mark as read',
      openLink: 'Open link',
    },
    messages: {
      deleteConfirm: 'Delete this notification?',
      deleteTitle: 'Delete notification',
      empty: 'No notifications found.',
    },
    name: NOTIFICATION_LABEL,
    pages: {
      list: NOTIFICATION_LABEL,
    },
    statuses: NOTIFICATION_STATUS_ENUM,
  },
  userPreferences: {
    fields: {
      enableAllEmail: 'Receive email for all templates',
      enableAllNotification: 'Receive notifications for all templates',
      enableEmail: 'Receive email',
      enableNotification: 'Receive notification',
      communications: 'Communications',
    },
    messages: {
      empty: 'No opt-out templates found.',
      communicationsHelp: 'Disable the communications you do not want to receive.',
    },
    pages: {
      edit: 'Preferences',
    },
  },
  templates: {
    actions: {
      addTranslation: 'Add translation',
      create: 'Create template',
      editTranslation: 'Edit translation',
    },
    channels: {
      configured: 'Configured',
      notConfigured: 'Not configured',
      notification: 'Notification',
      
    },
    fields: {
      actionLink: 'Action link',
      allowOptOut: 'Allow recipients to opt out',
    },
    messages: {
      deleteConfirm: 'Delete this template and all its translations?',
      deleteTitle: 'Delete template',
      deleteTranslationConfirm: 'Delete this language translation?',
      deleteTranslationTitle: 'Delete translation',
      empty: 'No templates found.',
      noTranslations: 'No translations found.',
    },
    name: RESOURCE_LABELS.courier.templates,
    pages: {
      create: 'Create template',
      edit: 'Edit template',
      list: RESOURCE_LABELS.courier.templates,
    },
    placeholders: {
      language: 'Select language',
    },
    sections: {
      translations: 'Translations',
    },
    validation: {
      channelRequired: 'Enable email, notification, or both.',
      emailBody: 'Email body is required.',
      emailSubject: 'Email subject must contain at least 10 characters.',
      notificationMessage: 'Notification message is required.',
      notificationTitle: 'Notification title is required.',
    },
  },
} as const
