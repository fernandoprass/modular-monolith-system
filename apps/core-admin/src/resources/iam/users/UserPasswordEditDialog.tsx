import { useForm } from '@tanstack/react-form'
import { useEffect, useState } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Dialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { USER_REQUEST_FIELDS, type UserPasswordUpdateForm } from './userTypes'
import { updateCurrentUserPassword } from './userApi'

const EMPTY_PASSWORD_FORM: UserPasswordUpdateForm = {
  passwordConfirm: '',
  passwordNew: '',
  passwordOld: '',
}

type UserPasswordEditDialogProps = {
  isOpen: boolean
  onClose: () => void
}

export function UserPasswordEditDialog({ isOpen, onClose }: UserPasswordEditDialogProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showError, showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const form = useForm({
    defaultValues: EMPTY_PASSWORD_FORM,
    onSubmit: async ({ value }) => {
      if (value.passwordNew !== value.passwordConfirm) {
        showError(t('features.iam.users.messages.passwordsDoNotMatch'))
        return
      }

      setIsSaving(true)

      try {
        await updateCurrentUserPassword({
          [USER_REQUEST_FIELDS.passwordNew]: value.passwordNew,
          [USER_REQUEST_FIELDS.passwordOld]: value.passwordOld,
        })
        showSuccess(t('features.iam.users.notifications.passwordUpdated'))
        onClose()
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      } finally {
        setIsSaving(false)
      }
    },
  })

  useEffect(() => {
    if (isOpen) {
      form.reset(EMPTY_PASSWORD_FORM)
    }
  }, [form, isOpen])

  return (
    <Dialog
      backLabel={t('shared.actions.back')}
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={t('features.iam.users.pages.changePassword')}
    >
      <form onSubmit={(event) => {
        event.preventDefault()
        void form.handleSubmit()
      }}>
        <div className="dialog-body">
          <FieldGroup>
            <form.Field name="passwordOld">
              {(field) => (
                <Field>
                  <FieldLabel htmlFor={field.name}>{t('shared.fields.currentPassword')}</FieldLabel>
                  <Input autoComplete="current-password" autoFocus id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="password" value={field.state.value} />
                </Field>
              )}
            </form.Field>
            <form.Field name="passwordNew">
              {(field) => (
                <Field>
                  <FieldLabel htmlFor={field.name}>{t('shared.fields.newPassword')}</FieldLabel>
                  <Input autoComplete="new-password" id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="password" value={field.state.value} />
                </Field>
              )}
            </form.Field>
            <form.Field name="passwordConfirm">
              {(field) => (
                <Field>
                  <FieldLabel htmlFor={field.name}>{t('shared.fields.confirmPassword')}</FieldLabel>
                  <Input autoComplete="new-password" id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required type="password" value={field.state.value} />
                </Field>
              )}
            </form.Field>
          </FieldGroup>
        </div>
        <div className="dialog-actions">
          <Button onClick={onClose} type="button" variant="outline">
            {t('shared.actions.cancel')}
          </Button>
          <Button disabled={isSaving} type="submit">
            {t('shared.actions.save')}
          </Button>
        </div>
      </form>
    </Dialog>
  )
}
