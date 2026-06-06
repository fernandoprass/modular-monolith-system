import polyglotI18nProvider from 'ra-i18n-polyglot'

import { enMessages } from './en'

export const i18nProvider = polyglotI18nProvider(() => enMessages, 'en')
