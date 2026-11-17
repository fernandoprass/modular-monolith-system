import type { PlainTranslationKey, Translate } from '../../../app/i18n/i18n'
import { SYSTEM_LOG_LEVELS, SYSTEM_LOG_STATUSES, type SystemLogLevel, type SystemLogStatus } from './systemLogTypes'

const SYSTEM_LOG_LEVEL_ENUM_KEYS: Record<SystemLogLevel, PlainTranslationKey> = {
  [SYSTEM_LOG_LEVELS.debug]: 'features.sentinel.enums.systemLogLevel.debug',
  [SYSTEM_LOG_LEVELS.information]: 'features.sentinel.enums.systemLogLevel.information',
  [SYSTEM_LOG_LEVELS.warning]: 'features.sentinel.enums.systemLogLevel.warning',
  [SYSTEM_LOG_LEVELS.error]: 'features.sentinel.enums.systemLogLevel.error',
  [SYSTEM_LOG_LEVELS.critical]: 'features.sentinel.enums.systemLogLevel.critical',
}

const SYSTEM_LOG_STATUS_ENUM_KEYS: Record<SystemLogStatus, PlainTranslationKey> = {
  [SYSTEM_LOG_STATUSES.unknown]: 'features.sentinel.enums.systemLogStatus.unknown',
  [SYSTEM_LOG_STATUSES.success]: 'features.sentinel.enums.systemLogStatus.success',
  [SYSTEM_LOG_STATUSES.failure]: 'features.sentinel.enums.systemLogStatus.failure',
  [SYSTEM_LOG_STATUSES.unauthorized]: 'features.sentinel.enums.systemLogStatus.unauthorized',
}

export function getSystemLogLevelEnum(value: SystemLogLevel, t: Translate): string {
  return t(SYSTEM_LOG_LEVEL_ENUM_KEYS[value] ?? 'features.sentinel.enums.systemLogLevel.unknown')
}

export function getSystemLogStatusEnum(value: SystemLogStatus, t: Translate): string {
  return t(SYSTEM_LOG_STATUS_ENUM_KEYS[value] ?? 'features.sentinel.enums.systemLogStatus.unknown')
}
