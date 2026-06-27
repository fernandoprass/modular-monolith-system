import { Save } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Controller, useForm } from 'react-hook-form'

import { useTranslate } from '../../../app/i18n/i18n'
import { useToast } from '../../../app/ToastProvider'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { Checkbox } from '../../../components/ui/checkbox'
import { getUserPreference, updateUserPreference } from './userPreferenceApi'
import type {
  UserPreferenceForm,
  UserPreferenceTemplateOptionDto,
} from './userPreferenceTypes'

function toForm(templates: UserPreferenceTemplateOptionDto[]): UserPreferenceForm {
  return {
    templates,
  }
}

type UserPreferencePageProps = {
  embedded?: boolean
}

export function UserPreferencePage({ embedded = false }: UserPreferencePageProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const [templates, setTemplates] = useState<UserPreferenceTemplateOptionDto[] | null>(null)

  const loadPreferences = useCallback(async () => {
    try {
      setTemplates(await getUserPreference())
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    }
  }, [notifyError, t])

  useEffect(() => {
    void loadPreferences()
  }, [loadPreferences])

  const content = (
    <>
      {!embedded && (
        <div className="page-header">
          <h1 className="page-title">{t('features.courier.userPreferences.pages.edit')}</h1>
        </div>
      )}
      {templates === null ? (
        <p className="page-subtitle">{t('shared.common.loading')}</p>
      ) : (
        <UserPreferenceFormPanel
          key={templates
            .map((template) => `${template.module}.${template.key}.${template.isEmailEnabled}.${template.isNotificationEnabled}`)
            .join('|')}
          templates={templates}
          onSaved={loadPreferences}
        />
      )}
    </>
  )

  return embedded
    ? <section className="user-preference-page">{content}</section>
    : <main className="page user-preference-page">{content}</main>
}

type UserPreferenceFormPanelProps = {
  templates: UserPreferenceTemplateOptionDto[]
  onSaved: () => Promise<void>
}

function UserPreferenceFormPanel({ templates: initialTemplates, onSaved }: UserPreferenceFormPanelProps) {
  const t = useTranslate()
  const notifyError = useNotifyError()
  const { showSuccess } = useToast()
  const [isSaving, setIsSaving] = useState(false)
  const form = useForm<UserPreferenceForm>({
    defaultValues: toForm(initialTemplates),
  })
  const templates = form.watch('templates')
  const allEmailEnabled = templates.every((template) => template.isEmailEnabled)
  const someEmailEnabled = templates.some((template) => template.isEmailEnabled)
  const allNotificationsEnabled = templates.every((template) => template.isNotificationEnabled)
  const someNotificationsEnabled = templates.some((template) => template.isNotificationEnabled)

  function setAllEmailEnabled(isEnabled: boolean) {
    form.setValue(
      'templates',
      templates.map((template) => ({ ...template, isEmailEnabled: isEnabled })),
      { shouldDirty: true },
    )
  }

  function setAllNotificationsEnabled(isEnabled: boolean) {
    form.setValue(
      'templates',
      templates.map((template) => ({ ...template, isNotificationEnabled: isEnabled })),
      { shouldDirty: true },
    )
  }

  async function handleSave(value: UserPreferenceForm) {
    setIsSaving(true)

    try {
      await updateUserPreference(value)
      showSuccess(t('features.courier.userPreferences.notifications.updated'))
      await onSaved()
    } catch (error) {
      notifyError(error, t('shared.errors.generic'))
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <form className="user-preference-form" onSubmit={form.handleSubmit(handleSave)}>
      <Card>
        <CardContent>
          <div className="section-header">
            <div>
              <h2 className="card-title">{t('features.courier.userPreferences.fields.communications')}</h2>
              <p className="card-description">{t('features.courier.userPreferences.messages.communicationsHelp')}</p>
            </div>
          </div>
          {templates.length === 0 ? (
            <p className="page-subtitle">{t('features.courier.userPreferences.messages.empty')}</p>
          ) : (
            <div className="access-table-wrap user-preference-table-wrap">
              <table className="access-table user-preference-table">
                <thead>
                  <tr>
                    <th>{t('shared.fields.module')}</th>
                    <th>{t('shared.fields.name')}</th>
                    <th>
                      <div className="user-preference-column-header">
                        <Checkbox
                          aria-label={t('features.courier.userPreferences.fields.enableAllEmail')}
                          checked={
                            allEmailEnabled
                              ? true
                              : someEmailEnabled
                                ? 'indeterminate'
                                : false
                          }
                          onCheckedChange={setAllEmailEnabled}
                        />
                        <span>{t('shared.fields.email')}</span>
                      </div>
                    </th>
                    <th>
                      <div className="user-preference-column-header">
                        <Checkbox
                          aria-label={t('features.courier.userPreferences.fields.enableAllNotification')}
                          checked={
                            allNotificationsEnabled
                              ? true
                              : someNotificationsEnabled
                                ? 'indeterminate'
                                : false
                          }
                          onCheckedChange={setAllNotificationsEnabled}
                        />
                        <span>{t('shared.fields.notification')}</span>
                      </div>
                    </th>
                  </tr>
                </thead>
                <tbody>
                  {templates.map((template, index) => (
                    <tr key={`${template.module}.${template.key}`}>
                      <td>{template.module}</td>
                      <td>{template.name}</td>
                      <td>
                        <Controller
                          control={form.control}
                          name={`templates.${index}.isEmailEnabled`}
                          render={({ field }) => (
                            <Checkbox
                              aria-label={t('features.courier.userPreferences.fields.enableEmail')}
                              checked={field.value}
                              onCheckedChange={field.onChange}
                            />
                          )}
                        />
                      </td>
                      <td>
                        <Controller
                          control={form.control}
                          name={`templates.${index}.isNotificationEnabled`}
                          render={({ field }) => (
                            <Checkbox
                              aria-label={t('features.courier.userPreferences.fields.enableNotification')}
                              checked={field.value}
                              onCheckedChange={field.onChange}
                            />
                          )}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </CardContent>
      </Card>

      <div className="form-actions">
        <Button disabled={isSaving} type="submit">
          <Save data-icon="inline-start" />
          {t('shared.actions.save')}
        </Button>
      </div>
    </form>
  )
}
