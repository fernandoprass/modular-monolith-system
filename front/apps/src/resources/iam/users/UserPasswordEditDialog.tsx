import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../../../shared/validation'
import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Dialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
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
    formState: { errors },
    handleSubmit,
    register,
    reset,
  } = useForm<UserPasswordUpdateForm>({
    defaultValues: EMPTY_PASSWORD_FORM,
    resolver: zodResolver(passwordEditSchema, { error: createValidationErrorMap(t) }),
  })

  useEffect(() => {
    if (isOpen) {
      reset(EMPTY_PASSWORD_FORM)
    }
  }, [isOpen, reset])

  async function handleSave(value: UserPasswordUpdateForm) {
    if (value.passwordNew !== value.passwordConfirm) {
      showError(t('features.iam.messages.users.passwordsDoNotMatch'))
      return
    }

    setIsSaving(true)

    try {
      const message = await updateCurrentUserPassword({
        [USER_REQUEST_FIELDS.passwordNew]: value.passwordNew,
        [USER_REQUEST_FIELDS.passwordOld]: value.passwordOld,
      })
      showSuccess(message)
      onClose()
    } catch (error) {
      notifyError(error, t('common.messages.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog
      backLabel={t('common.actions.back')}
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={t('features.iam.actions.changePassword')}
    >
      <form noValidate onSubmit={handleSubmit(handleSave)}>
        <div className="dialog-body">
          <FieldGroup>
            <Field data-invalid={errors.passwordOld !== undefined}>
              <FieldLabel htmlFor="passwordOld">{t('features.iam.labels.currentPassword')}</FieldLabel>
              <Input
                aria-invalid={errors.passwordOld !== undefined}
                aria-describedby={errors.passwordOld ? 'UserPasswordEditDialog-passwordOld-error' : undefined}
                autoComplete="current-password"
                autoFocus
                id="passwordOld"
                required
                type="password"
                {...register('passwordOld')}
              />
              <FieldError id="UserPasswordEditDialog-passwordOld-error">{errors.passwordOld?.message}</FieldError>
            </Field>
            <Field data-invalid={errors.passwordNew !== undefined}>
              <FieldLabel htmlFor="passwordNew">{t('features.iam.labels.newPassword')}</FieldLabel>
              <Input
                aria-invalid={errors.passwordNew !== undefined}
                aria-describedby={errors.passwordNew ? 'UserPasswordEditDialog-passwordNew-error' : undefined}
                autoComplete="new-password"
                id="passwordNew"
                required
                type="password"
                {...register('passwordNew')}
              />
              <FieldError id="UserPasswordEditDialog-passwordNew-error">{errors.passwordNew?.message}</FieldError>
            </Field>
            <Field data-invalid={errors.passwordConfirm !== undefined}>
              <FieldLabel htmlFor="passwordConfirm">{t('features.iam.labels.confirmPassword')}</FieldLabel>
              <Input
                aria-invalid={errors.passwordConfirm !== undefined}
                aria-describedby={errors.passwordConfirm ? 'UserPasswordEditDialog-passwordConfirm-error' : undefined}
                autoComplete="new-password"
                id="passwordConfirm"
                required
                type="password"
                {...register('passwordConfirm')}
              />
              <FieldError id="UserPasswordEditDialog-passwordConfirm-error">{errors.passwordConfirm?.message}</FieldError>
            </Field>
          </FieldGroup>
        </div>
        <div className="dialog-actions">
          <Button onClick={onClose} type="button" variant="outline">
            {t('common.actions.cancel')}
          </Button>
          <Button disabled={isSaving} type="submit">
            {t('common.actions.save')}
          </Button>
        </div>
      </form>
    </Dialog>
  )
}
