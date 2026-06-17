import type { HTMLAttributes } from 'react'

import { cn } from '../../lib/utils'

export function Empty({ className, ...props }: HTMLAttributes<HTMLDivElement>) {
  return <div className={cn('empty', className)} {...props} />
}

export function EmptyDescription({ className, ...props }: HTMLAttributes<HTMLParagraphElement>) {
  return <p className={cn('empty-description', className)} {...props} />
}
