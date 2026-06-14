import { useForm } from '@tanstack/react-form'
import { useEffect, useState } from 'react'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Checkbox } from '../../../components/ui/checkbox'
import { Dialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Textarea } from '../../../components/ui/textarea'
import { OrganizationSelect } from '../organizations/OrganizationSelect'
import { createRole, updateRole } from './roleApi'
import type { RoleDto, RoleForm } from './roleTypes'

type RoleEditDialogProps = {
  isOpen: boolean
  onClose: () => void
  onSaved: () => Promise<void>
  role: RoleDto | null
}

const EMPTY_ROLE_FORM: RoleForm = {
  description: '',
  isActive: true,
  isDefault: false,
  name: '',
  organizationId: '',
}

export function RoleEditDialog({ isOpen, onClose, onSaved, role }: RoleEditDialogProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const isCreate = role === null
  const form = useForm({
    defaultValues: EMPTY_ROLE_FORM,
    onSubmit: async ({ value }) => {
      setIsSaving(true)

      try {
        if (role === null) {
          await createRole(value)
          showSuccess(t('features.iam.roles.notifications.created'))
        } else {
          await updateRole(role.id, value)
          showSuccess(t('features.iam.roles.notifications.updated'))
        }

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
    if (!isOpen) {
      return
    }

    form.reset(role === null ? EMPTY_ROLE_FORM : {
      description: role.description,
      isActive: role.isActive,
      isDefault: role.isDefault,
      name: role.name,
      organizationId: role.organizationId ?? '',
    })
  }, [form, isOpen, role])

  return (
    <Dialog
      backLabel={t('shared.actions.back')}
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={isCreate ? t('shared.actions.create') : t('shared.actions.edit')}
    >
      <form onSubmit={(event) => {
        event.preventDefault()
        void form.handleSubmit()
      }}>
        <div className="dialog-body">
          <FieldGroup>
            <form.Field name="organizationId">
              {(field) => (
                <Field data-disabled={!isCreate}>
                  <FieldLabel>{t('shared.fields.organization')}</FieldLabel>
                  <OrganizationSelect
                    clearable
                    disabled={!isCreate}
                    includeInactive
                    onValueChange={field.handleChange}
                    value={field.state.value}
                  />
                </Field>
              )}
            </form.Field>
            <form.Field name="name">
              {(field) => (
                <Field>
                  <FieldLabel htmlFor={field.name}>{t('shared.fields.name')}</FieldLabel>
                  <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                </Field>
              )}
            </form.Field>
            <form.Field name="description">
              {(field) => (
                <Field>
                  <FieldLabel htmlFor={field.name}>{t('shared.fields.description')}</FieldLabel>
                  <Textarea id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                </Field>
              )}
            </form.Field>
            <form.Field name="isActive">
              {(field) => (
                <Checkbox
                  checked={field.state.value}
                  label={t('shared.fields.isActive')}
                  onCheckedChange={(checked) => field.handleChange(checked === true)}
                />
              )}
            </form.Field>
            <form.Field name="isDefault">
              {(field) => (
                <Checkbox
                  checked={field.state.value}
                  label={t('shared.fields.isDefault')}
                  onCheckedChange={(checked) => field.handleChange(checked === true)}
                />
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
