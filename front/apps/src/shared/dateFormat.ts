import { resolveLocale } from '../app/i18n/i18n'

const USER_DATE_TIME_FORMAT_OPTIONS: Intl.DateTimeFormatOptions = {
  day: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
  month: '2-digit',
  second: '2-digit',
  year: 'numeric',
}

export function formatUserDateTime(value: string, language?: string): string {
  const date = new Date(value)

  if (Number.isNaN(date.getTime())) {
    return value
  }

  return new Intl.DateTimeFormat(
    resolveLocale(language),
    USER_DATE_TIME_FORMAT_OPTIONS,
  ).format(date)
}
