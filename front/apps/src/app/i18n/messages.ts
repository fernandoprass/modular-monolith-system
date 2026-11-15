import { LANGUAGE_CODES, ALL_LANGUAGE_OPTIONS, normalizeLanguageCode, type LanguageCode } from '../../shared/languages'
import { enMessages } from './en'

type MessageTree = {
  [key: string]: string | MessageTree
}

type DictionaryShape<T> = {
  [K in keyof T]: T[K] extends string ? string : DictionaryShape<T[K]>
}

/** Every locale must provide the same message tree as English. */
export type MessageDictionary = DictionaryShape<typeof enMessages>

type LeafPaths<T> = {
  [K in keyof T & string]: T[K] extends string ? K : `${K}.${LeafPaths<T[K]>}`
}[keyof T & string]

export type TranslationKey = LeafPaths<typeof enMessages>

type MessageAtPath<T, K extends string> = K extends `${infer Head}.${infer Tail}`
  ? Head extends keyof T ? MessageAtPath<T[Head], Tail> : never
  : K extends keyof T ? T[K] : never

type Placeholders<T> = T extends `${string}{{${infer Name}}}${infer Rest}`
  ? Name | Placeholders<Rest>
  : never

type VariablesFor<K extends TranslationKey> = Record<
  Placeholders<MessageAtPath<typeof enMessages, K>>,
  string | number
>

/** Keys suitable for labels and other messages without interpolation. */
export type PlainTranslationKey = {
  [K in TranslationKey]: keyof VariablesFor<K> extends never ? K : never
}[TranslationKey]

type InterpolatedTranslationKey = Exclude<TranslationKey, PlainTranslationKey>

type TranslationArguments = [key: PlainTranslationKey] | {
  [K in InterpolatedTranslationKey]: [key: K, variables: VariablesFor<K>]
}[InterpolatedTranslationKey]

export const DEFAULT_LOCALE = LANGUAGE_CODES.english

export const LOCALE_DICTIONARIES = {
  [DEFAULT_LOCALE]: enMessages,
} as const satisfies Partial<Record<LanguageCode, MessageDictionary>>

export type SupportedLocale = keyof typeof LOCALE_DICTIONARIES

/** Only languages with a registered UI dictionary are available for UI selection. */
export const UI_LANGUAGE_OPTIONS = ALL_LANGUAGE_OPTIONS.filter(
  (option): option is Extract<typeof ALL_LANGUAGE_OPTIONS[number], { value: SupportedLocale }> => (
    Object.hasOwn(LOCALE_DICTIONARIES, option.value)
  ),
)

export function resolveLocale(locale?: string): SupportedLocale {
  const normalized = normalizeLanguageCode(locale ?? DEFAULT_LOCALE)

  return Object.hasOwn(LOCALE_DICTIONARIES, normalized)
    ? normalized as SupportedLocale
    : DEFAULT_LOCALE
}

const warnedMissingMessages = new Set<string>()

function warnMissingMessage(locale: SupportedLocale, key: string): void {
  if (!import.meta.env?.DEV) {
    return
  }

  const warningKey = `${locale}:${key}`

  if (!warnedMissingMessages.has(warningKey)) {
    warnedMissingMessages.add(warningKey)
    console.warn(`[i18n] Missing translation: ${warningKey}`)
  }
}

function readMessage(messages: MessageTree, path: string): string | undefined {
  const value = path.split('.').reduce<string | MessageTree | undefined>((current, part) => {
    if (typeof current !== 'object' || current === null) {
      return undefined
    }

    return Object.hasOwn(current, part) ? current[part] : undefined
  }, messages)

  return typeof value === 'string' ? value : undefined
}

function interpolate(value: string, locale: SupportedLocale, variables?: Record<string, string | number>): string {
  if (variables === undefined) {
    return value
  }

  return value.replace(/\{\{([^{}]+)\}\}/g, (placeholder: string, key: string) => {
    const variable = Object.hasOwn(variables, key) ? variables[key] : undefined

    if (variable === undefined) {
      return placeholder
    }

    return typeof variable === 'number' ? new Intl.NumberFormat(locale).format(variable) : variable
  })
}

export type Translate = (...args: TranslationArguments) => string

export function createTranslate(locale: string = DEFAULT_LOCALE): Translate {
  const resolvedLocale = resolveLocale(locale)
  const messages: MessageTree = LOCALE_DICTIONARIES[resolvedLocale]

  return (...[key, variables]: TranslationArguments): string => {
    let message = readMessage(messages, key)

    if (message === undefined) {
      warnMissingMessage(resolvedLocale, key)

      if (resolvedLocale !== DEFAULT_LOCALE) {
        message = readMessage(LOCALE_DICTIONARIES[DEFAULT_LOCALE], key)

        if (message === undefined) {
          warnMissingMessage(DEFAULT_LOCALE, key)
        }
      }
    }

    return interpolate(message ?? key, resolvedLocale, variables)
  }
}

export const translate = createTranslate()

