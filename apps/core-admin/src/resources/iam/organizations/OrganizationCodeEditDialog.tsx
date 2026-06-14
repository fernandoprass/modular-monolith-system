import { useForm } from '@tanstack/react-form'
import { useEffect, useState } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Dialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'
import { updateOrganizationCode } from './organizationApi'

type OrganizationCodeEditDialogProps = {
  isOpen: boolean
  onClose: () => void
  onSaved: () => Promise<void>
  organization: OrganizationDto
}

export function OrganizationCodeEditDialog({
  isOpen,
  onClose,
  onSaved,
  organization,
}: OrganizationCodeEditDialogProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const form = useForm({
    defaultValues: {
      code: organization.code,
    },
    onSubmit: async ({ value }) => {
      setIsSaving(true)

      try {
        await updateOrganizationCode(organization.id, {
          [ORGANIZATION_REQUEST_FIELDS.code]: value.code,
        })
        showSuccess(t('features.iam.organizations.notifications.codeUpdated'))
        await onSaved()
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
      form.reset({ code: organization.code })
    }
  }, [form, isOpen, organization.code])

  return (
    <Dialog
      backLabel={t('shared.actions.back')}
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={t('shared.actions.editCode')}
    >
      <form onSubmit={(event) => {
        event.preventDefault()
        void form.handleSubmit()
      }}>
        <div className="dialog-body">
          <FieldGroup>
            <form.Field name="code">
              {(field) => (
                <Field>
                  <FieldLabel htmlFor={field.name}>{t('shared.fields.code')}</FieldLabel>
                  <Input autoFocus id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
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
