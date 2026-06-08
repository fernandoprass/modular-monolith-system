import { createContext, useContext } from 'react'

import { enMessages } from './en'

type Messages = typeof enMessages
type MessageTree = {
  [key: string]: string | MessageTree
}

const messages = enMessages as MessageTree

function readMessage(path: string): string {
  const value = path.split('.').reduce<string | MessageTree | undefined>((current, part) => {
    if (typeof current !== 'object' || current === null) {
      return undefined
    }

    return current[part]
  }, messages)

  return typeof value === 'string' ? value : path
}

function interpolate(value: string, variables?: Record<string, string | number>): string {
  if (variables === undefined) {
    return value
  }

  return Object.entries(variables).reduce(
    (text, [key, variable]) => text.replaceAll(`{{${key}}}`, String(variable)),
    value,
  )
}

export type Translate = (key: string, variables?: Record<string, string | number>) => string

export const I18nContext = createContext<Translate>((key) => key)

export function translate(key: string, variables?: Record<string, string | number>): string {
  return interpolate(readMessage(key), variables)
}

export function useTranslate(): Translate {
  return useContext(I18nContext)
}

export type { Messages }
