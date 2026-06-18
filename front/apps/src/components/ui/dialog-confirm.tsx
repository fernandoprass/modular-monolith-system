import * as DialogPrimitive from '@radix-ui/react-dialog'
import { X } from 'lucide-react'
import type { PropsWithChildren, ReactNode } from 'react'

type DialogProps = PropsWithChildren<{
  backLabel: string
  onOpenChange: (open: boolean) => void
  open: boolean
  title: string
}>

export function Dialog({ children, backLabel, onOpenChange, open, title }: DialogProps) {
  return (
    <DialogPrimitive.Root onOpenChange={onOpenChange} open={open}>
      <DialogPrimitive.Portal>
        <DialogPrimitive.Overlay className="dialog-overlay" />
        <DialogPrimitive.Content className="dialog-content">
          <div className="dialog-header">
            <DialogPrimitive.Title className="dialog-title">{title}</DialogPrimitive.Title>
            <DialogPrimitive.Close aria-label={backLabel} className="dialog-close">
              <X />
            </DialogPrimitive.Close>
          </div>
          {children}
        </DialogPrimitive.Content>
      </DialogPrimitive.Portal>
    </DialogPrimitive.Root>
  )
}

type ConfirmDialogProps = {
  cancelText: string
  backLabel: string
  confirmText: string
  children: ReactNode
  onConfirm: () => void
  onOpenChange: (open: boolean) => void
  open: boolean
  title: string
}

export function ConfirmDialog({
  cancelText,
  children,
  backLabel,
  confirmText,
  onConfirm,
  onOpenChange,
  open,
  title,
}: ConfirmDialogProps) {
  return (
    <Dialog backLabel={backLabel} onOpenChange={onOpenChange} open={open} title={title}>
      <div className="dialog-body">{children}</div>
      <div className="dialog-actions">
        <button className="btn btn-sm btn-outline" onClick={() => onOpenChange(false)} type="button">
          {cancelText}
        </button>
        <button className="btn btn-sm btn-destructive" onClick={onConfirm} type="button">
          {confirmText}
        </button>
      </div>
    </Dialog>
  )
}
