import type { Translate } from '../../../app/i18n/i18n'
import { SYSTEM_LOG_LEVELS, SYSTEM_LOG_STATUSES, type SystemLogLevel, type SystemLogStatus } from './systemLogTypes'

const SYSTEM_LOG_LEVEL_LABEL_KEYS: Record<SystemLogLevel, string> = {
  [SYSTEM_LOG_LEVELS.debug]: 'features.sentinel.systemLogs.levels.debug',
  [SYSTEM_LOG_LEVELS.information]: 'features.sentinel.systemLogs.levels.information',
  [SYSTEM_LOG_LEVELS.warning]: 'features.sentinel.systemLogs.levels.warning',
  [SYSTEM_LOG_LEVELS.error]: 'features.sentinel.systemLogs.levels.error',
  [SYSTEM_LOG_LEVELS.critical]: 'features.sentinel.systemLogs.levels.critical',
}

const SYSTEM_LOG_STATUS_LABEL_KEYS: Record<SystemLogStatus, string> = {
  [SYSTEM_LOG_STATUSES.unknown]: 'features.sentinel.systemLogs.statuses.unknown',
  [SYSTEM_LOG_STATUSES.success]: 'features.sentinel.systemLogs.statuses.success',
  [SYSTEM_LOG_STATUSES.failure]: 'features.sentinel.systemLogs.statuses.failure',
  [SYSTEM_LOG_STATUSES.unauthorized]: 'features.sentinel.systemLogs.statuses.unauthorized',
}

export function getSystemLogLevelLabel(value: SystemLogLevel, t: Translate): string {
  return t(SYSTEM_LOG_LEVEL_LABEL_KEYS[value] ?? 'features.sentinel.systemLogs.levels.unknown')
}

export function getSystemLogStatusLabel(value: SystemLogStatus, t: Translate): string {
  return t(SYSTEM_LOG_STATUS_LABEL_KEYS[value] ?? 'features.sentinel.systemLogs.statuses.unknown')
}
