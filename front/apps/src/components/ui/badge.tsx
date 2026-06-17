import type { HTMLAttributes } from 'react'

import { cn } from '../../lib/utils'

type BadgeProps = HTMLAttributes<HTMLSpanElement> & {
  variant?: 'active' | 'inactive'
}

export function Badge({ className, variant = 'inactive', ...props }: BadgeProps) {
  return <span className={cn('badge', `badge-${variant}`, className)} {...props} />
}
