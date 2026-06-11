import { ArrowLeft } from 'lucide-react'
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Badge } from '../../../components/ui/badge'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { getOrganization } from './organizationApi'
import type { OrganizationDto } from './organizationTypes'
import { getLanguageLabel, getOrganizationTypeLabel } from './organizationUi'

export function OrganizationViewPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { id } = useParams()
  const [organization, setOrganization] = useState<OrganizationDto | null>(null)

  useEffect(() => {
    if (id === undefined) {
      return
    }

    const organizationId = id

    async function loadOrganization() {
      try {
        setOrganization(await getOrganization(organizationId))
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      }
    }

    void loadOrganization()
  }, [id, notifyError, t])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('resources.iam.organizations.pages.show')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.organizations)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {organization === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <div className="detail-grid">
              <Field label={t('resources.iam.organizations.fields.type')}>
                {getOrganizationTypeLabel(organization.type, t)}
              </Field>
              <Field label={t('resources.iam.organizations.fields.code')}>{organization.code}</Field>
              <Field label={t('resources.iam.organizations.fields.name')}>{organization.name}</Field>
              <Field label={t('resources.iam.organizations.fields.defaultLanguage')}>
                {getLanguageLabel(organization.defaultLanguage, t)}
              </Field>
              <Field label={t('resources.iam.organizations.fields.description')}>
                {organization.description ?? ''}
              </Field>
              <Field label={t('resources.iam.organizations.fields.isActive')}>
                <Badge variant={organization.isActive ? 'active' : 'inactive'}>
                  {organization.isActive ? t('shared.status.active') : t('shared.status.inactive')}
                </Badge>
              </Field>
            </div>
          )}
        </CardContent>
      </Card>
    </main>
  )
}

type FieldProps = {
  children: ReactNode
  label: string
}

function Field({ children, label }: FieldProps) {
  return (
    <div className="detail-field">
      <span className="detail-label">{label}</span>
      <span className="detail-value">{children}</span>
    </div>
  )
}
