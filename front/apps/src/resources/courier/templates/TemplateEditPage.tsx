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
import { DataTable } from '../../../components/ui/data-table'
import { ConfirmDialog } from '../../../components/ui/dialog-confirm'
import { Field, FieldGroup, FieldLabel } from '../../../components/ui/form'
import { Input } from '../../../components/ui/input'
import { Select } from '../../../components/ui/select'
import { COURIER_PERMISSIONS } from '../../../shared/courierConstants'
import { LANGUAGE_OPTIONS } from '../../../shared/languages'
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
  RETENTION_POLICY_OPTIONS,
  TEMPLATE_TYPES,
  TEMPLATE_TYPE_OPTIONS,
  type TemplateDto,
  type TemplateEmailTranslationDto,
  type TemplateForm,
} from './templateTypes'
import { toTranslatedTemplateOptions } from './templateUi'

const EMPTY_TEMPLATE_FORM: TemplateForm = {
  key: '',
  name: '',
  retentionPolicy: RETENTION_POLICY_OPTIONS[0].value,
  type: TEMPLATE_TYPE_OPTIONS[1].value,
}

const templateSchema = z.object({
  key: z.string().trim().min(5),
  name: z.string().trim().min(1),
  retentionPolicy: z.string().trim().min(1),
  type: z.string().trim().min(1),
})

type TemplateDetailsFormProps = {
  onSaved: () => Promise<void>
  template: TemplateDto | null
}

function toForm(template: TemplateDto): TemplateForm {
  return {
    key: template.key,
    name: template.name,
    retentionPolicy: String(template.retentionPolicy),
    type: String(template.type),
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
      {template !== null && template.type === TEMPLATE_TYPES.email && (
        <TemplateTranslations onChanged={loadTemplate} template={template} />
      )}
      {template !== null && template.type !== TEMPLATE_TYPES.email && (
        <p className="page-subtitle template-type-note">
          {t('features.courier.templates.messages.translationsEmailOnly')}
        </p>
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
  const hasTranslations = (template?.emailTranslations.length ?? 0) > 0
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
          <Field>
            <FieldLabel htmlFor="template-key">{t('shared.fields.key')}</FieldLabel>
            <Input id="template-key" required {...form.register('key')} />
          </Field>
          <Field>
            <FieldLabel htmlFor="template-name">{t('shared.fields.name')}</FieldLabel>
            <Input id="template-name" required {...form.register('name')} />
          </Field>
        </div>
        <div className="form-row-two">
          <Controller
            control={form.control}
            name="type"
            render={({ field }) => (
              <Field>
                <FieldLabel>{t('shared.fields.type')}</FieldLabel>
                <Select
                  disabled={hasTranslations}
                  onValueChange={field.onChange}
                  options={toTranslatedTemplateOptions(TEMPLATE_TYPE_OPTIONS, t)}
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
  const [translationToEdit, setTranslationToEdit] = useState<TemplateEmailTranslationDto | null>(null)
  const [translationToDelete, setTranslationToDelete] = useState<TemplateEmailTranslationDto | null>(null)
  const [isTranslationDialogOpen, setIsTranslationDialogOpen] = useState(false)
  const canWrite = hasPermissionCode(permissions, COURIER_PERMISSIONS.templates.write)
  const usedLanguages = template.emailTranslations.map((translation) => translation.language)
  const canAddLanguage = LANGUAGE_OPTIONS.some((option) => !usedLanguages.includes(option.value))
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
        data={template.emailTranslations}
        emptyText={t('features.courier.templates.messages.noTranslations')}
        isLoading={false}
        loadingText={t('shared.common.loading')}
        onSortingChange={setSorting}
        sorting={sorting}
      />
      <TemplateTranslationDialog
        isOpen={isTranslationDialogOpen}
        onClose={() => setIsTranslationDialogOpen(false)}
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
