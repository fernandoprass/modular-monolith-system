import { API_PATHS } from '../../../data/apiPaths'
import { getCourierJson, putCourierJson } from '../../../data/httpClient'
import { unwrapResult } from '../../../data/result'
import {
  USER_PREFERENCE_REQUEST_FIELDS,
  type UserPreferenceForm,
  type UserPreferenceTemplateOptionDto,
  type UserPreferenceUpdateRequest,
} from './userPreferenceTypes'

function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === 'object' && value !== null
}

function readString(value: Record<string, unknown>, ...keys: string[]): string {
  for (const key of keys) {
    const data = value[key]

    if (typeof data === 'string') {
      return data
    }
  }

  return ''
}

function readBoolean(value: Record<string, unknown>, ...keys: string[]): boolean {
  for (const key of keys) {
    const data = value[key]

    if (typeof data === 'boolean') {
      return data
    }
  }

  return false
}

function normalizeTemplateOption(value: unknown): UserPreferenceTemplateOptionDto {
  const source = isRecord(value) ? value : {}

  return {
    isEmailEnabled: readBoolean(source, 'isEmailEnabled', 'IsEmailEnabled'),
    isNotificationEnabled: readBoolean(source, 'isNotificationEnabled', 'IsNotificationEnabled'),
    key: readString(source, 'key', 'Key'),
    module: readString(source, 'module', 'Module'),
    name: readString(source, 'name', 'Name'),
  }
}

function toUpdateRequest(form: UserPreferenceForm): UserPreferenceUpdateRequest {
  return {
    [USER_PREFERENCE_REQUEST_FIELDS.templates]: form.templates.map((template) => ({
      [USER_PREFERENCE_REQUEST_FIELDS.isEmailEnabled]: template.isEmailEnabled,
      [USER_PREFERENCE_REQUEST_FIELDS.isNotificationEnabled]: template.isNotificationEnabled,
      [USER_PREFERENCE_REQUEST_FIELDS.key]: template.key,
      [USER_PREFERENCE_REQUEST_FIELDS.module]: template.module,
    })),
  }
}

export async function getUserPreference(): Promise<UserPreferenceTemplateOptionDto[]> {
  const response = await getCourierJson(API_PATHS.courier.userPreferences.list)
  const result = unwrapResult<unknown>(response)

  return Array.isArray(result) ? result.map(normalizeTemplateOption) : []
}

export async function updateUserPreference(form: UserPreferenceForm): Promise<void> {
  await putCourierJson(API_PATHS.courier.userPreferences.list, toUpdateRequest(form))
}
