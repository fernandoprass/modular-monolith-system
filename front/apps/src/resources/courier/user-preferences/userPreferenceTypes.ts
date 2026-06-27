export const USER_PREFERENCE_REQUEST_FIELDS = {
  isEmailEnabled: 'IsEmailEnabled',
  isNotificationEnabled: 'IsNotificationEnabled',
  key: 'Key',
  module: 'Module',
  templates: 'Templates',
} as const

export type UserPreferenceTemplateOptionDto = {
  isEmailEnabled: boolean
  isNotificationEnabled: boolean
  key: string
  module: string
  name: string
}

export type UserPreferenceTemplateForm = UserPreferenceTemplateOptionDto

export type UserPreferenceForm = {
  templates: UserPreferenceTemplateForm[]
}

export type UserPreferenceUpdateRequest = {
  Templates: UserPreferenceTemplateUpdateRequest[]
}

export type UserPreferenceTemplateUpdateRequest = {
  IsEmailEnabled: boolean
  IsNotificationEnabled: boolean
  Key: string
  Module: string
}
