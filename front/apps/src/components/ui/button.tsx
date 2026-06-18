import { Slot } from '@radix-ui/react-slot'
import { cva, type VariantProps } from 'class-variance-authority'
import type { ButtonHTMLAttributes } from 'react'
import { forwardRef } from 'react'

import { cn } from '../../lib/utils'

const buttonVariants = cva('btn', {
  defaultVariants: {
    size: 'sm',
    variant: 'default',
  },
  variants: {
    size: {
      icon: 'btn-icon',
      sm: 'btn-sm',
    },
    variant: {
      default: 'btn-default',
      destructive: 'btn-destructive',
      ghost: 'btn-ghost',
      outline: 'btn-outline',
    },
  },
})

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement>
  & VariantProps<typeof buttonVariants>
  & {
    asChild?: boolean
  }

const Button = forwardRef<HTMLButtonElement, ButtonProps>(function Button({
  asChild,
  className,
  size,
  variant,
  ...props
}, ref) {
  const Component = asChild ? Slot : 'button'

  return <Component className={cn(buttonVariants({ size, variant }), className)} ref={ref} {...props} />
})

export { Button }
