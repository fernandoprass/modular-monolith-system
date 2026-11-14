import { appMessages, MODULE_LABELS, RESOURCE_LABELS } from './app'
import { commonMessages } from './common'
import { sentinelMessages } from './02.sentinel'
import { iamMessages } from './03.iam'
import { courierMessages } from './04.courier'

export const enMessages = {
  ...appMessages,
  features: {
    courier: courierMessages,
    iam: iamMessages,
    sentinel: sentinelMessages,
  },
  common: commonMessages,
  modules: MODULE_LABELS,
  resources: RESOURCE_LABELS,
} as const
