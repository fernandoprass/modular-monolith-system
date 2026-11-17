import { toTranslatedOptions } from '../../../shared/translatedOptions'
import { zodResolver } from '@hookform/resolvers/zod'
import { createValidationErrorMap } from '../../../shared/validation'
import { useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Checkbox } from '../../../components/ui/checkbox'
import { Dialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
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
    formState: { errors },
    control,
    handleSubmit,
    register,
    reset,
  } = useForm<PermissionUpdateForm>({
    defaultValues: toForm(permission),
    resolver: zodResolver(permissionEditSchema, { error: createValidationErrorMap(t) }),
  })

  useEffect(() => {
    if (isOpen) {
      reset(toForm(permission))
    }
  }, [isOpen, permission, reset])

  async function handleSave(value: PermissionUpdateForm) {
    setIsSaving(true)

    try {
      const message = await updatePermission(permission.id, value)
      showSuccess(message)
      await onSaved()
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
      title={t('common.actions.edit')}
    >
      <form noValidate onSubmit={handleSubmit(handleSave)}>
        <div className="dialog-body">
          <FieldGroup>
            <Controller
              control={control}
              name="module"
              render={({ field }) => (
                <Field data-invalid={errors.module !== undefined}>
                  <FieldLabel htmlFor="PermissionEditDialog-module">{t('common.labels.module')}</FieldLabel>
                  <Select
                    onBlur={field.onBlur}
                    ref={field.ref}
                    aria-invalid={errors.module !== undefined}
                    aria-describedby={errors.module ? 'PermissionEditDialog-module-error' : undefined}
                    id="PermissionEditDialog-module"
                    onValueChange={field.onChange}
                    options={toTranslatedOptions(PERMISSION_MODULE_OPTIONS, t)}
                    value={field.value}
                  />
                  <FieldError id="PermissionEditDialog-module-error">{errors.module?.message}</FieldError>
                </Field>
              )}
            />
            <Controller
              control={control}
              name="resource"
              render={({ field }) => (
                <Field data-invalid={errors.resource !== undefined}>
                  <FieldLabel htmlFor="PermissionEditDialog-resource">{t('common.labels.resource')}</FieldLabel>
                  <Select
                    onBlur={field.onBlur}
                    ref={field.ref}
                    aria-invalid={errors.resource !== undefined}
                    aria-describedby={errors.resource ? 'PermissionEditDialog-resource-error' : undefined}
                    id="PermissionEditDialog-resource"
                    onValueChange={field.onChange}
                    options={toTranslatedOptions(PERMISSION_RESOURCE_OPTIONS, t)}
                    value={field.value}
                  />
                  <FieldError id="PermissionEditDialog-resource-error">{errors.resource?.message}</FieldError>
                </Field>
              )}
            />
            <Field data-invalid={errors.action !== undefined}>
              <FieldLabel htmlFor="action">{t('common.labels.action')}</FieldLabel>
              <Input
                aria-invalid={errors.action !== undefined}
                aria-describedby={errors.action ? 'PermissionEditDialog-action-error' : undefined}
                id="action"
                required
                {...register('action')}
              />
              <FieldError id="PermissionEditDialog-action-error">{errors.action?.message}</FieldError>
            </Field>
            <Field data-invalid={errors.title !== undefined}>
              <FieldLabel htmlFor="title">{t('common.labels.title')}</FieldLabel>
              <Input
                aria-invalid={errors.title !== undefined}
                aria-describedby={errors.title ? 'PermissionEditDialog-title-error' : undefined}
                id="title"
                required
                {...register('title')}
              />
              <FieldError id="PermissionEditDialog-title-error">{errors.title?.message}</FieldError>
            </Field>
            <Field data-invalid={errors.description !== undefined}>
              <FieldLabel htmlFor="description">{t('common.labels.description')}</FieldLabel>
              <Textarea
                aria-invalid={errors.description !== undefined}
                aria-describedby={errors.description ? 'PermissionEditDialog-description-error' : undefined}
                id="description"
                required
                {...register('description')}
              />
              <FieldError id="PermissionEditDialog-description-error">{errors.description?.message}</FieldError>
            </Field>
            <Controller
              control={control}
              name="isActive"
              render={({ field }) => (
                <Checkbox
                  checked={field.value}
                  label={t('common.labels.isActive')}
                  onCheckedChange={field.onChange}
                />
              )}
            />
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
