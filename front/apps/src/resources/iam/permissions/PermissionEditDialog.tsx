import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

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

const permissionEditSchema = z.object({
  action: z.string().trim().min(1),
  description: z.string().trim().min(1),
  isActive: z.boolean(),
  module: z.string().trim().min(1),
  resource: z.string().trim().min(1),
  title: z.string().trim().min(1),
})

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
  const {
    control,
    handleSubmit,
    register,
    reset,
  } = useForm<PermissionUpdateForm>({
    defaultValues: toForm(permission),
    resolver: zodResolver(permissionEditSchema),
  })

  useEffect(() => {
    if (isOpen) {
      reset(toForm(permission))
    }
  }, [isOpen, permission, reset])

  async function handleSave(value: PermissionUpdateForm) {
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
  }

  return (
    <Dialog
      backLabel={t('shared.actions.back')}
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={t('shared.actions.edit')}
    >
      <form onSubmit={handleSubmit(handleSave)}>
        <div className="dialog-body">
          <FieldGroup>
            <Controller
              control={control}
              name="module"
              render={({ field }) => (
                <Field>
                  <FieldLabel>{t('shared.fields.module')}</FieldLabel>
                  <Select
                    onValueChange={field.onChange}
                    options={toTranslatedOptions(PERMISSION_MODULE_OPTIONS, t)}
                    value={field.value}
                  />
                </Field>
              )}
            />
            <Controller
              control={control}
              name="resource"
              render={({ field }) => (
                <Field>
                  <FieldLabel>{t('shared.fields.resource')}</FieldLabel>
                  <Select
                    onValueChange={field.onChange}
                    options={toTranslatedOptions(PERMISSION_RESOURCE_OPTIONS, t)}
                    value={field.value}
                  />
                </Field>
              )}
            />
            <Field>
              <FieldLabel htmlFor="action">{t('shared.fields.action')}</FieldLabel>
              <Input id="action" required {...register('action')} />
            </Field>
            <Field>
              <FieldLabel htmlFor="title">{t('shared.fields.title')}</FieldLabel>
              <Input id="title" required {...register('title')} />
            </Field>
            <Field>
              <FieldLabel htmlFor="description">{t('shared.fields.description')}</FieldLabel>
              <Textarea id="description" required {...register('description')} />
            </Field>
            <Controller
              control={control}
              name="isActive"
              render={({ field }) => (
                <Checkbox
                  checked={field.value}
                  label={t('shared.fields.isActive')}
                  onCheckedChange={field.onChange}
                />
              )}
            />
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
