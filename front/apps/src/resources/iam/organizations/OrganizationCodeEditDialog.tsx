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
import { ORGANIZATION_REQUEST_FIELDS, type OrganizationDto } from './organizationTypes'
import { updateOrganizationCode } from './organizationApi'

type OrganizationCodeEditDialogProps = {
  isOpen: boolean
  onClose: () => void
  onSaved: () => Promise<void>
  organization: OrganizationDto
}

type OrganizationCodeForm = {
  code: string
}

const organizationCodeSchema = z.object({
  code: z.string().trim().min(1),
})

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
  const {
    handleSubmit,
    register,
    reset,
  } = useForm<OrganizationCodeForm>({
    defaultValues: {
      code: organization.code,
    },
    resolver: zodResolver(organizationCodeSchema),
  })

  useEffect(() => {
    if (isOpen) {
      reset({ code: organization.code })
    }
  }, [isOpen, organization.code, reset])

  async function handleSave(value: OrganizationCodeForm) {
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
  }

  return (
    <Dialog
      backLabel={t('shared.actions.back')}
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={t('shared.actions.editCode')}
    >
      <form onSubmit={handleSubmit(handleSave)}>
        <div className="dialog-body">
          <FieldGroup>
            <Field>
              <FieldLabel htmlFor="code">{t('shared.fields.code')}</FieldLabel>
              <Input autoFocus id="code" required {...register('code')} />
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
