import { appMessages, MODULE_LABELS } from './app'
import { commonMessages } from './common'
import { sentinelMessages } from './02.sentinel'
import { iamMessages } from './03.iam'
import { courierMessages } from './04.courier'

export const enMessages = {
  ...appMessages,
  common: commonMessages,
  features: {
    courier: courierMessages,
    iam: iamMessages,
    sentinel: sentinelMessages,
  },
  modules: MODULE_LABELS,
} as const
