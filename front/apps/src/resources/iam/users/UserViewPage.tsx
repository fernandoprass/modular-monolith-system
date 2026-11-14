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
import { UserAccessTabs } from './UserAccessTabs'
import { getUser } from './userApi'
import type { UserDto } from './userTypes'
import { getLanguageLabel } from './userUi'

export function UserViewPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { id } = useParams()
  const [user, setUser] = useState<UserDto | null>(null)

  useEffect(() => {
    if (id === undefined) {
      return
    }

    const userId = id

    async function loadUser() {
      try {
        setUser(await getUser(userId))
      } catch (error) {
        notifyError(error, t('common.errors.generic'))
      }
    }

    void loadUser()
  }, [id, notifyError, t])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.users.pages.show')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.users)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('common.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {user === null ? (
            <p className="page-subtitle">{t('common.common.loading')}</p>
          ) : (
            <div className="detail-stack">
                <div className="detail-grid-two-columns">
                  <Field label={t('common.fields.organization')}>{user.organizationName}</Field>
                  <Field label={t('common.fields.language')}>{getLanguageLabel(user.language, t)}</Field>
                  <Field label={t('common.fields.name')}>{user.name}</Field>
                  <Field label={t('common.fields.email')}>{user.email}</Field>
                  <Field label={t('common.fields.isSystemAdmin')}>
                    {user.isSystemAdmin ? t('common.common.yes') : t('common.common.no')}
                  </Field>
                  <Field label={t('common.fields.isOrganizationAdmin')}>
                    {user.isOrganizationAdmin ? t('common.common.yes') : t('common.common.no')}
                  </Field>
                  <Field label={t('common.fields.isActive')}>
                    <Badge variant={user.isActive ? 'active' : 'inactive'}>
                      {user.isActive ? t('common.status.active') : t('common.status.inactive')}
                    </Badge>
                  </Field>
                </div>
              <UserAccessTabs userId={user.id} />
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
