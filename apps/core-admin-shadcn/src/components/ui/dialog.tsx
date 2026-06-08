import * as DialogPrimitive from '@radix-ui/react-dialog'
import { X } from 'lucide-react'
import type { PropsWithChildren, ReactNode } from 'react'

type DialogProps = PropsWithChildren<{
  onOpenChange: (open: boolean) => void
  open: boolean
  title: string
}>

export function Dialog({ children, onOpenChange, open, title }: DialogProps) {
  return (
    <DialogPrimitive.Root onOpenChange={onOpenChange} open={open}>
      <DialogPrimitive.Portal>
        <DialogPrimitive.Overlay className="dialog-overlay" />
        <DialogPrimitive.Content className="dialog-content">
          <div className="dialog-header">
            <DialogPrimitive.Title className="dialog-title">{title}</DialogPrimitive.Title>
            <DialogPrimitive.Close className="dialog-close">
              <X size={16} />
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
  confirmText,
  onConfirm,
  onOpenChange,
  open,
  title,
}: ConfirmDialogProps) {
  return (
    <Dialog onOpenChange={onOpenChange} open={open} title={title}>
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
