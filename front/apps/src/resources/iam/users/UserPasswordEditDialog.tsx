import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'

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

const passwordEditSchema = z.object({
  passwordConfirm: z.string().min(1),
  passwordNew: z.string().min(1),
  passwordOld: z.string().min(1),
})

type UserPasswordEditDialogProps = {
  isOpen: boolean
  onClose: () => void
}

export function UserPasswordEditDialog({ isOpen, onClose }: UserPasswordEditDialogProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showError, showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const {
    handleSubmit,
    register,
    reset,
  } = useForm<UserPasswordUpdateForm>({
    defaultValues: EMPTY_PASSWORD_FORM,
    resolver: zodResolver(passwordEditSchema),
  })

  useEffect(() => {
    if (isOpen) {
      reset(EMPTY_PASSWORD_FORM)
    }
  }, [isOpen, reset])

  async function handleSave(value: UserPasswordUpdateForm) {
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
  }

  return (
    <Dialog
      backLabel={t('shared.actions.back')}
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={t('features.iam.users.pages.changePassword')}
    >
      <form onSubmit={handleSubmit(handleSave)}>
        <div className="dialog-body">
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor="passwordOld">{t('shared.fields.currentPassword')}</FieldLabel>
              <Input autoComplete="current-password" autoFocus id="passwordOld" required type="password" {...register('passwordOld')} />
            </Field>
            <Field>
              <FieldLabel htmlFor="passwordNew">{t('shared.fields.newPassword')}</FieldLabel>
              <Input autoComplete="new-password" id="passwordNew" required type="password" {...register('passwordNew')} />
            </Field>
            <Field>
              <FieldLabel htmlFor="passwordConfirm">{t('shared.fields.confirmPassword')}</FieldLabel>
              <Input autoComplete="new-password" id="passwordConfirm" required type="password" {...register('passwordConfirm')} />
            </Field>
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
