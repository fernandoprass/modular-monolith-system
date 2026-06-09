import { createContext, useCallback, useContext, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'
import { X } from 'lucide-react'

import { useTranslate } from './i18n/i18n'

type Toast = {
  id: number
  message: string
  type: 'error' | 'success'
}

type ToastContextValue = {
  showError: (message: string) => void
  showSuccess: (message: string) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

export function ToastProvider({ children }: PropsWithChildren) {
  const t = useTranslate()
  const [toasts, setToasts] = useState<Toast[]>([])

  const removeToast = useCallback((id: number) => {
    setToasts((current) => current.filter((toast) => toast.id !== id))
  }, [])

  const showToast = useCallback((message: string, type: Toast['type']) => {
    const id = Date.now()
    setToasts((current) => [...current, { id, message, type }])
    window.setTimeout(() => removeToast(id), 4000)
  }, [removeToast])

  const value = useMemo<ToastContextValue>(() => ({
    showError: (message) => showToast(message, 'error'),
    showSuccess: (message) => showToast(message, 'success'),
  }), [showToast])

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div className="toast-region">
        {toasts.map((toast) => (
          <div className={`toast toast-${toast.type}`} key={toast.id}>
            <span>{toast.message}</span>
            <button
              aria-label={t('shared.actions.back')}
              className="toast-close"
              onClick={() => removeToast(toast.id)}
              type="button"
            >
              <X size={14} />
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}

export function useToast(): ToastContextValue {
  const context = useContext(ToastContext)

  if (context === null) {
    throw new Error('Toast context missing.')
  }

  return context
}
