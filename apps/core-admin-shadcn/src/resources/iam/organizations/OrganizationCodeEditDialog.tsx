import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Dialog } from '../../../components/ui/dialog'
import { Input } from '../../../components/ui/input'
import { Label } from '../../../components/ui/label'
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
  const [code, setCode] = useState(organization.code)
  const [isSaving, setIsSaving] = useState(false)

  useEffect(() => {
    if (isOpen) {
      setCode(organization.code)
    }
  }, [isOpen, organization.code])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setIsSaving(true)

    try {
      await updateOrganizationCode(organization.id, {
        [ORGANIZATION_REQUEST_FIELDS.code]: code,
      })
      showSuccess(t('resources.iam.organizations.notifications.codeUpdated'))
      await onSaved()
      onClose()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <Dialog onOpenChange={(open) => !open && onClose()} open={isOpen} title={t('resources.iam.organizations.actions.editCode')}>
      <form onSubmit={handleSubmit}>
        <div className="dialog-body form-stack">
          <div className="field">
            <Label>{t('resources.iam.organizations.fields.code')}</Label>
            <Input autoFocus onChange={(event) => setCode(event.currentTarget.value)} required value={code} />
          </div>
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
