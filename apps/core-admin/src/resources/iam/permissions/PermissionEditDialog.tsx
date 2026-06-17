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
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import type { PermissionDto } from '../../../shared/permissions'
import { updatePermission } from './permissionApi'
import {
  PERMISSION_ACTION_OPTIONS,
  PERMISSION_MODULE_OPTIONS,
  PERMISSION_RESOURCE_OPTIONS,
  type PermissionUpdateForm,
} from './permissionTypes'
import { toTranslatedOptions } from './permissionUi'

type PermissionEditDialogProps = {
  isOpen: boolean
  onClose: () => void
  onSaved: () => Promise<void>
  permission: PermissionDto
}

export function PermissionEditDialog({
  isOpen,
  onClose,
  onSaved,
  permission,
}: PermissionEditDialogProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const form = useForm({
    defaultValues: toForm(permission),
    onSubmit: async ({ value }) => {
      setIsSaving(true)

      try {
        await updatePermission(permission.id, value)
        showSuccess(t('features.iam.permissions.notifications.updated'))
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
      form.reset(toForm(permission))
    }
  }, [form, isOpen, permission])

  return (
    <Dialog
      backLabel={t('shared.actions.back')}
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={t('shared.actions.edit')}
    >
      <form onSubmit={(event) => {
        event.preventDefault()
        void form.handleSubmit()
      }}>
        <div className="dialog-body">
          <FieldGroup>
            <form.Field name="module">
              {(field) => (
                <Field>
                  <FieldLabel>{t('shared.fields.module')}</FieldLabel>
                  <Select
                    onValueChange={field.handleChange}
                    options={toTranslatedOptions(PERMISSION_MODULE_OPTIONS, t)}
                    value={field.state.value}
                  />
                </Field>
              )}
            </form.Field>
            <form.Field name="resource">
              {(field) => (
                <Field>
                  <FieldLabel>{t('shared.fields.resource')}</FieldLabel>
                  <Select
                    onValueChange={field.handleChange}
                    options={toTranslatedOptions(PERMISSION_RESOURCE_OPTIONS, t)}
                    value={field.state.value}
                  />
                </Field>
              )}
            </form.Field>
            <form.Field name="action">
              {(field) => (
                <Field>
                  <FieldLabel>{t('shared.fields.action')}</FieldLabel>
                  <Input id={field.name} onBlur={field.handleBlur} onChange={(event) => field.handleChange(event.currentTarget.value)} required value={field.state.value} />
                </Field>
              )}
            </form.Field>
            <form.Field name="title">
              {(field) => (
                <Field>
                  <FieldLabel htmlFor={field.name}>{t('shared.fields.title')}</FieldLabel>
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

function toForm(permission: PermissionDto): PermissionUpdateForm {
  return {
    action: permission.action,
    description: permission.description,
    isActive: permission.isActive,
    module: permission.module,
    resource: permission.resource,
    title: permission.title,
  }
}
