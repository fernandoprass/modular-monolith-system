export const LANGUAGE_CODES = {
  english: 'en',
  portugueseBrazil: 'pt-br',
  spanish: 'es',
} as const

export const languageChoices = [
  {
    id: LANGUAGE_CODES.english,
    name: 'shared.languages.en',
  },
  {
    id: LANGUAGE_CODES.portugueseBrazil,
    name: 'shared.languages.ptBr',
  },
  {
    id: LANGUAGE_CODES.spanish,
    name: 'shared.languages.es',
  },
]
