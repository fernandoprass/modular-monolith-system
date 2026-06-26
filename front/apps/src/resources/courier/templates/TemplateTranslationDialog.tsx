import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { type Translate, useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Checkbox } from '../../../components/ui/checkbox'
import { Dialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldError, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { LANGUAGE_OPTIONS, normalizeLanguageCode } from '../../../shared/languages'
import { addTemplateTranslation, updateTemplateTranslation } from './templateApi'
import type { TemplateTranslationDto, TemplateTranslationForm } from './templateTypes'
import { toTranslatedTemplateOptions } from './templateUi'

type TemplateTranslationDialogProps = {
  isOpen: boolean
  onClose: () => void
  onSaved: () => Promise<void>
  templateId: string
  translation: TemplateTranslationDto | null
  usedLanguages: string[]
}

function createTranslationSchema(t: Translate) {
  return z.object({
    emailBody: z.string(),
    emailEnabled: z.boolean(),
    emailSubject: z.string(),
    language: z.string().trim().min(2).max(35),
    name: z.string().trim().min(1),
    notificationActionLink: z.string(),
    notificationEnabled: z.boolean(),
    notificationMessage: z.string(),
    notificationTitle: z.string(),
  }).superRefine((value, context) => {
    if (!value.emailEnabled && !value.notificationEnabled) {
      context.addIssue({
        code: 'custom',
        message: t('features.courier.templates.validation.channelRequired'),
        path: ['emailEnabled'],
      })
    }

    if (value.emailEnabled) {
      if (value.emailSubject.trim().length < 10) {
        context.addIssue({
          code: 'custom',
          message: t('features.courier.templates.validation.emailSubject'),
          path: ['emailSubject'],
        })
      }

      if (value.emailBody.trim().length === 0) {
        context.addIssue({
          code: 'custom',
          message: t('features.courier.templates.validation.emailBody'),
          path: ['emailBody'],
        })
      }
    }

    if (value.notificationEnabled) {
      if (value.notificationTitle.trim().length === 0) {
        context.addIssue({
          code: 'custom',
          message: t('features.courier.templates.validation.notificationTitle'),
          path: ['notificationTitle'],
        })
      }

      if (value.notificationMessage.trim().length === 0) {
        context.addIssue({
          code: 'custom',
          message: t('features.courier.templates.validation.notificationMessage'),
          path: ['notificationMessage'],
        })
      }
    }
  })
}

const EMPTY_TRANSLATION_FORM: TemplateTranslationForm = {
  emailBody: '',
  emailEnabled: true,
  emailSubject: '',
  language: '',
  name: '',
  notificationActionLink: '',
  notificationEnabled: true,
  notificationMessage: '',
  notificationTitle: '',
}

function toForm(translation: TemplateTranslationDto | null): TemplateTranslationForm {
  return translation === null
    ? EMPTY_TRANSLATION_FORM
    : {
      emailBody: translation.email?.body ?? '',
      emailEnabled: translation.email !== null,
      emailSubject: translation.email?.subject ?? '',
      language: normalizeLanguageCode(translation.language),
      name: translation.name,
      notificationActionLink: translation.notification?.actionLink ?? '',
      notificationEnabled: translation.notification !== null,
      notificationMessage: translation.notification?.message ?? '',
      notificationTitle: translation.notification?.title ?? '',
    }
}

export function TemplateTranslationDialog({
  isOpen,
  onClose,
  onSaved,
  templateId,
  translation,
  usedLanguages,
}: TemplateTranslationDialogProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const translationSchema = useMemo(() => createTranslationSchema(t), [t])
  const form = useForm<TemplateTranslationForm>({
    defaultValues: toForm(translation),
    resolver: zodResolver(translationSchema),
  })
  const emailBody = form.watch('emailBody')
  const emailEnabled = form.watch('emailEnabled')
  const notificationEnabled = form.watch('notificationEnabled')
  const normalizedUsedLanguages = usedLanguages.map(normalizeLanguageCode)
  const languageOptions = LANGUAGE_OPTIONS.filter((option) => (
    option.value === normalizeLanguageCode(translation?.language ?? '')
    || !normalizedUsedLanguages.includes(normalizeLanguageCode(option.value))
  ))

  useEffect(() => {
    if (isOpen) {
      form.reset(toForm(translation))
    }
  }, [form, isOpen, translation])

  async function handleSave(value: TemplateTranslationForm) {
    setIsSaving(true)

    try {
      if (translation === null) {
        await addTemplateTranslation(templateId, value)
        showSuccess(t('features.courier.templates.notifications.translationAdded'))
      } else {
        await updateTemplateTranslation(templateId, translation.language, value)
        showSuccess(t('features.courier.templates.notifications.translationUpdated'))
      }

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
      className="dialog-content-wide template-translation-dialog"
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={translation === null
        ? t('features.courier.templates.actions.addTranslation')
        : t('features.courier.templates.actions.editTranslation')}
    >
      <form onSubmit={form.handleSubmit(handleSave)}>
        <div className="dialog-body template-translation-dialog-body">
          <div className="template-translation-identity">
            <Controller
              control={form.control}
              name="language"
              render={({ field }) => (
                <Field>
                  <FieldLabel>{t('shared.fields.language')}</FieldLabel>
                  <Select
                    disabled={translation !== null}
                    onValueChange={field.onChange}
                    options={toTranslatedTemplateOptions(languageOptions, t)}
                    placeholder={t('features.courier.templates.placeholders.language')}
                    value={field.value}
                  />
                </Field>
              )}
            />
            <Field>
              <FieldLabel htmlFor="template-translation-name">{t('shared.fields.name')}</FieldLabel>
              <Input id="template-translation-name" required {...form.register('name')} />
            </Field>
          </div>

          <div className="template-channel-grid">
            <section className="template-channel-section">
              <Controller
                control={form.control}
                name="emailEnabled"
                render={({ field }) => (
                  <Checkbox
                    checked={field.value}
                    label={t('shared.fields.email')}
                    onCheckedChange={field.onChange}
                  />
                )}
              />
              <FieldError>{form.formState.errors.emailEnabled?.message}</FieldError>
              <FieldGroup>
                <Field>
                  <FieldLabel htmlFor="template-email-subject">{t('shared.fields.subject')}</FieldLabel>
                  <Input
                    disabled={!emailEnabled}
                    id="template-email-subject"
                    {...form.register('emailSubject')}
                  />
                  <FieldError>{form.formState.errors.emailSubject?.message}</FieldError>
                </Field>
                <Field>
                  <FieldLabel htmlFor="template-email-body">{t('shared.fields.body')}</FieldLabel>
                  <Textarea
                    className="template-translation-body-input"
                    disabled={!emailEnabled}
                    id="template-email-body"
                    {...form.register('emailBody')}
                  />
                  <FieldError>{form.formState.errors.emailBody?.message}</FieldError>
                </Field>
                <div className="template-preview-panel">
                  <span className="detail-label">{t('shared.fields.preview')}</span>
                  <iframe
                    className="template-preview-frame"
                    sandbox=""
                    srcDoc={emailEnabled ? emailBody : ''}
                    title={t('shared.fields.preview')}
                  />
                </div>
              </FieldGroup>
            </section>

            <section className="template-channel-section">
              <Controller
                control={form.control}
                name="notificationEnabled"
                render={({ field }) => (
                  <Checkbox
                    checked={field.value}
                    label={t('features.courier.templates.channels.notification')}
                    onCheckedChange={field.onChange}
                  />
                )}
              />
              <FieldGroup>
                <Field>
                  <FieldLabel htmlFor="template-notification-title">{t('shared.fields.title')}</FieldLabel>
                  <Input
                    disabled={!notificationEnabled}
                    id="template-notification-title"
                    {...form.register('notificationTitle')}
                  />
                  <FieldError>{form.formState.errors.notificationTitle?.message}</FieldError>
                </Field>
                <Field>
                  <FieldLabel htmlFor="template-notification-message">{t('shared.fields.message')}</FieldLabel>
                  <Textarea
                    className="template-notification-message-input"
                    disabled={!notificationEnabled}
                    id="template-notification-message"
                    {...form.register('notificationMessage')}
                  />
                  <FieldError>{form.formState.errors.notificationMessage?.message}</FieldError>
                </Field>
                <Field>
                  <FieldLabel htmlFor="template-notification-action-link">
                    {t('features.courier.templates.fields.actionLink')}
                  </FieldLabel>
                  <Input
                    disabled={!notificationEnabled}
                    id="template-notification-action-link"
                    {...form.register('notificationActionLink')}
                  />
                </Field>
              </FieldGroup>
            </section>
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
