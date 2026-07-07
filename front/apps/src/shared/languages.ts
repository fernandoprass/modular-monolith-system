export const LANGUAGE_CODES = {
  english: 'en-US',
  portugueseBrazil: 'pt-BR',
  spanish: 'es-ES',
} as const

export const LANGUAGE_OPTIONS = [
  {
    labelKey: 'shared.languages.en',
    value: LANGUAGE_CODES.english,
  },
  {
    labelKey: 'shared.languages.ptbr',
    value: LANGUAGE_CODES.portugueseBrazil,
  },
  {
    labelKey: 'shared.languages.es',
    value: LANGUAGE_CODES.spanish,
  },
] as const

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
