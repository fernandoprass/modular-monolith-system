import { zodResolver } from '@hookform/resolvers/zod'
import type { SortingState } from '@tanstack/react-table'
import { ArrowLeft, Plus } from 'lucide-react'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'
import { useNavigate, useParams } from 'react-router-dom'
import { z } from 'zod'

import { useToast } from '../../../app/ToastProvider'
import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { DataTable } from '../../../components/ui/data-table'
import { ConfirmDialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { COURIER_PERMISSIONS } from '../../../shared/courierConstants'
import { LANGUAGE_OPTIONS, normalizeLanguageCode } from '../../../shared/languages'
import { hasPermissionCode } from '../../../shared/permissions'
import {
  createTemplate,
  deleteTemplateTranslation,
  getTemplate,
  updateTemplate,
} from './templateApi'
import { TemplateTranslationDialog } from './TemplateTranslationDialog'
import { createTemplateTranslationColumns } from './TemplateTranslationTableColumns'
import {
  NOTIFICATION_SEVERITY_OPTIONS,
  RETENTION_POLICY_OPTIONS,
  TEMPLATE_MODULE_OPTIONS,
  type TemplateDto,
  type TemplateForm,
  type TemplateTranslationDto,
} from './templateTypes'
import { toTranslatedTemplateOptions } from './templateUi'

const EMPTY_TEMPLATE_FORM: TemplateForm = {
  isAllowingOptOut: false,
  key: '',
  module: TEMPLATE_MODULE_OPTIONS[0].value,
  retentionPolicy: RETENTION_POLICY_OPTIONS[0].value,
  severity: NOTIFICATION_SEVERITY_OPTIONS[0].value,
}

const templateSchema = z.object({
  isAllowingOptOut: z.boolean(),
  key: z.string().trim().min(5),
  module: z.string().trim().min(2),
  retentionPolicy: z.string().trim().min(1),
  severity: z.string().trim().min(1),
})

type TemplateDetailsFormProps = {
  onSaved: () => Promise<void>
  template: TemplateDto | null
}

function toForm(template: TemplateDto): TemplateForm {
  return {
    isAllowingOptOut: template.isAllowingOptOut,
    key: template.key,
    module: template.module,
    retentionPolicy: String(template.retentionPolicy),
    severity: String(template.severity),
  }
}

export function TemplateEditPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { id } = useParams()
  const [template, setTemplate] = useState<TemplateDto | null>(null)
  const isCreate = id === undefined

  const loadTemplate = useCallback(async () => {
    if (id === undefined) {
      return
    }

    try {
      setTemplate(await getTemplate(id))
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [id, notifyError, t])

  useEffect(() => {
    setTemplate(null)

    if (!isCreate) {
      void loadTemplate()
    }
  }, [isCreate, loadTemplate])

  return (
    <main className="page courier-template-page">
      <div className="page-header">
        <h1 className="page-title">
          {t(isCreate ? 'features.courier.templates.pages.create' : 'features.courier.templates.pages.edit')}
        </h1>
        <Button onClick={() => navigate(APP_ROUTES.templates)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {!isCreate && template === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <TemplateDetailsForm
              key={template?.id ?? 'create'}
              onSaved={loadTemplate}
              template={template}
            />
          )}
        </CardContent>
      </Card>
      {template !== null && (
        <TemplateTranslations onChanged={loadTemplate} template={template} />
      )}
    </main>
  )
}

function TemplateDetailsForm({ onSaved, template }: TemplateDetailsFormProps) {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const isCreate = template === null
  const form = useForm<TemplateForm>({
    defaultValues: template === null ? EMPTY_TEMPLATE_FORM : toForm(template),
    resolver: zodResolver(templateSchema),
  })

  async function handleSave(value: TemplateForm) {
    setIsSaving(true)

    try {
      if (template === null) {
        const created = await createTemplate(value)

        showSuccess(t('features.courier.templates.notifications.created'))
        navigate(APP_ROUTES.templateEdit(created.id))
      } else {
        await updateTemplate(template.id, value)
        showSuccess(t('features.courier.templates.notifications.updated'))
        await onSaved()
      }
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <form className="edit-form" onSubmit={form.handleSubmit(handleSave)}>
      <FieldGroup>
        <div className="form-row-two">
          <Controller
            control={form.control}
            name="module"
            render={({ field }) => (
              <Field>
                <FieldLabel>{t('shared.fields.module')}</FieldLabel>
                <Select
                  onValueChange={field.onChange}
                  options={toTranslatedTemplateOptions(TEMPLATE_MODULE_OPTIONS, t)}
                  value={field.value}
                />
              </Field>
            )}
          />
          <Field>
            <FieldLabel htmlFor="template-key">{t('shared.fields.key')}</FieldLabel>
            <Input id="template-key" required {...form.register('key')} />
          </Field>
        </div>
        <div className="form-row-two">
          <Controller
            control={form.control}
            name="severity"
            render={({ field }) => (
              <Field>
                <FieldLabel>{t('shared.fields.severity')}</FieldLabel>
                <Select
                  onValueChange={field.onChange}
                  options={toTranslatedTemplateOptions(NOTIFICATION_SEVERITY_OPTIONS, t)}
                  value={field.value}
                />
              </Field>
            )}
          />
          <Controller
            control={form.control}
            name="retentionPolicy"
            render={({ field }) => (
              <Field>
                <FieldLabel>{t('shared.fields.retentionPolicy')}</FieldLabel>
                <Select
                  onValueChange={field.onChange}
                  options={toTranslatedTemplateOptions(RETENTION_POLICY_OPTIONS, t)}
                  value={field.value}
                />
              </Field>
            )}
          />
        </div>
        <Controller
          control={form.control}
          name="isAllowingOptOut"
          render={({ field }) => (
            <Checkbox
              checked={field.value}
              label={t('features.courier.templates.fields.allowOptOut')}
              onCheckedChange={field.onChange}
            />
          )}
        />
      </FieldGroup>
      <div className="form-actions">
        <Button disabled={isSaving} type="submit">
          {t(isCreate ? 'shared.actions.create' : 'shared.actions.save')}
        </Button>
      </div>
    </form>
  )
}

type TemplateTranslationsProps = {
  onChanged: () => Promise<void>
  template: TemplateDto
}

function TemplateTranslations({ onChanged, template }: TemplateTranslationsProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const { permissions } = useAuth()
  const [sorting, setSorting] = useState<SortingState>([])
  const [translationToEdit, setTranslationToEdit] = useState<TemplateTranslationDto | null>(null)
  const [translationToDelete, setTranslationToDelete] = useState<TemplateTranslationDto | null>(null)
  const [isTranslationDialogOpen, setIsTranslationDialogOpen] = useState(false)
  const canWrite = hasPermissionCode(permissions, COURIER_PERMISSIONS.templates.write)
  const usedLanguages = template.translations.map((translation) => normalizeLanguageCode(translation.language))
  const canAddLanguage = LANGUAGE_OPTIONS.some((option) => (
    !usedLanguages.includes(normalizeLanguageCode(option.value))
  ))
  const columns = useMemo(() => createTemplateTranslationColumns({
    canWrite,
    onDelete: setTranslationToDelete,
    onEdit: (translation) => {
      setTranslationToEdit(translation)
      setIsTranslationDialogOpen(true)
    },
    t,
  }), [canWrite, t])

  function handleAdd() {
    setTranslationToEdit(null)
    setIsTranslationDialogOpen(true)
  }

  function handleDialogClose() {
    setIsTranslationDialogOpen(false)
    setTranslationToEdit(null)
  }

  async function handleDelete() {
    if (translationToDelete === null) {
      return
    }

    try {
      await deleteTemplateTranslation(template.id, translationToDelete.language)
      showSuccess(t('features.courier.templates.notifications.translationDeleted'))
      setTranslationToDelete(null)
      await onChanged()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }

  return (
    <section className="template-translations-section">
      <div className="section-header">
        <h2 className="detail-section-title">{t('features.courier.templates.sections.translations')}</h2>
        {canWrite && (
          <Button disabled={!canAddLanguage} onClick={handleAdd} type="button" variant="outline">
            <Plus data-icon="inline-start" />
            {t('features.courier.templates.actions.addTranslation')}
          </Button>
        )}
      </div>
      <DataTable
        columns={columns}
        data={template.translations}
        emptyText={t('features.courier.templates.messages.noTranslations')}
        isLoading={false}
        loadingText={t('shared.common.loading')}
        onSortingChange={setSorting}
        sorting={sorting}
      />
      <TemplateTranslationDialog
        isOpen={isTranslationDialogOpen}
        onClose={handleDialogClose}
        onSaved={onChanged}
        templateId={template.id}
        translation={translationToEdit}
        usedLanguages={usedLanguages}
      />
      <ConfirmDialog
        backLabel={t('shared.actions.back')}
        cancelText={t('shared.actions.cancel')}
        confirmText={t('shared.actions.delete')}
        onConfirm={() => void handleDelete()}
        onOpenChange={(open) => !open && setTranslationToDelete(null)}
        open={translationToDelete !== null}
        title={t('features.courier.templates.messages.deleteTranslationTitle')}
      >
        {t('features.courier.templates.messages.deleteTranslationConfirm')}
      </ConfirmDialog>
    </section>
  )
}
