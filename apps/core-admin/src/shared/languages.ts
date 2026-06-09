export const LANGUAGE_CODES = {
  english: 'en',
  portugueseBrazil: 'pt-br',
  spanish: 'es',
} as const

export const LANGUAGE_OPTIONS = [
  {
    labelKey: 'shared.languages.en',
    value: LANGUAGE_CODES.english,
  },
  {
    labelKey: 'shared.languages.ptBr',
    value: LANGUAGE_CODES.portugueseBrazil,
  },
  {
    labelKey: 'shared.languages.es',
    value: LANGUAGE_CODES.spanish,
  },
] as const
