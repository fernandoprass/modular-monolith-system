import { zodResolver } from '@hookform/resolvers/zod'
import { useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Dialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { Textarea } from '../../../components/ui/textarea'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'
import { addTemplateTranslation, updateTemplateTranslation } from './templateApi'
import type { TemplateEmailTranslationDto, TemplateTranslationForm } from './templateTypes'
import { toTranslatedTemplateOptions } from './templateUi'

type TemplateTranslationDialogProps = {
  isOpen: boolean
  onClose: () => void
  onSaved: () => Promise<void>
  templateId: string
  translation: TemplateEmailTranslationDto | null
  usedLanguages: string[]
}

const translationSchema = z.object({
  body: z.string().trim().min(1),
  language: z.string().trim().min(2).max(5),
  subject: z.string().trim().min(10),
})

const EMPTY_TRANSLATION_FORM: TemplateTranslationForm = {
  body: '',
  language: '',
  subject: '',
}

function toForm(translation: TemplateEmailTranslationDto | null): TemplateTranslationForm {
  return translation === null
    ? EMPTY_TRANSLATION_FORM
    : {
      body: translation.body,
      language: translation.language,
      subject: translation.subject,
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
  const form = useForm<TemplateTranslationForm>({
    defaultValues: toForm(translation),
    resolver: zodResolver(translationSchema),
  })
  const body = form.watch('body')
  const languageOptions = LANGUAGE_OPTIONS.filter((option) => (
    option.value === translation?.language || !usedLanguages.includes(option.value)
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
      className="dialog-content-wide"
      onOpenChange={(open) => !open && onClose()}
      open={isOpen}
      title={translation === null
        ? t('features.courier.templates.actions.addTranslation')
        : t('features.courier.templates.actions.editTranslation')}
    >
      <form onSubmit={form.handleSubmit(handleSave)}>
        <div className="dialog-body template-translation-dialog-body">
          <div className="template-translation-editor">
            <FieldGroup>
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
                <FieldLabel htmlFor="template-translation-subject">{t('shared.fields.subject')}</FieldLabel>
                <Input id="template-translation-subject" required {...form.register('subject')} />
              </Field>
              <Field>
                <FieldLabel htmlFor="template-translation-body">{t('shared.fields.body')}</FieldLabel>
                <Textarea
                  className="template-translation-body-input"
                  id="template-translation-body"
                  required
                  {...form.register('body')}
                />
              </Field>
            </FieldGroup>
          </div>
          <div className="template-preview-panel">
            <span className="detail-label">{t('shared.fields.preview')}</span>
            <iframe
              className="template-preview-frame"
              sandbox=""
              srcDoc={body}
              title={t('shared.fields.preview')}
            />
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
