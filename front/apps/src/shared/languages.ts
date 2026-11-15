export const LANGUAGE_CODES = {
  english: 'en-US',
  portugueseBrazil: 'pt-BR',
  french: 'fr-FR',
  spanish: 'es-ES',
} as const

export type LanguageCode = typeof LANGUAGE_CODES[keyof typeof LANGUAGE_CODES]

/** Known and planned languages, including those not yet accepted by the backend. */
export const ALL_LANGUAGE_OPTIONS = [
  {
    labelKey: 'common.languages.en',
    value: LANGUAGE_CODES.english,
  },
  {
    labelKey: 'common.languages.ptbr',
    value: LANGUAGE_CODES.portugueseBrazil,
  },
  {
    labelKey: 'common.languages.fr',
    value: LANGUAGE_CODES.french,
  },
  {
    labelKey: 'common.languages.es',
    value: LANGUAGE_CODES.spanish,
  },
] as const

/** Preference/content languages accepted by Shared.Domain.LanguageOptions. */
export const LANGUAGE_OPTIONS = ALL_LANGUAGE_OPTIONS.filter(option => option.value !== LANGUAGE_CODES.french)

export function normalizeLanguageCode(language: string): string {
  const [languageCode = '', ...subtags] = language.trim().split('-')

  return [
    languageCode.toLowerCase(),
    ...subtags.map((subtag) => (
      subtag.length === 2 || subtag.length === 3
        ? subtag.toUpperCase()
        : subtag
    )),
  ].join('-')
}
