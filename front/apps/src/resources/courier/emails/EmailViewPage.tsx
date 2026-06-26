import { ArrowLeft } from 'lucide-react'
import { useEffect, useState, type ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useAuth, useNotifyError } from '../../../auth/AuthProvider'
import { Badge } from '../../../components/ui/badge'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { formatUserDateTime } from '../../../shared/dateFormat'
import { getEmail } from './emailApi'
import type { EmailDto } from './emailTypes'
import { getEmailStatusClassName, getEmailStatusLabel } from './emailUi'

function formatOptionalDate(value: string | null, language?: string): string {
  return value === null ? '' : formatUserDateTime(value, language)
}

export function EmailViewPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { user } = useAuth()
  const { id } = useParams()
  const [email, setEmail] = useState<EmailDto | null>(null)

  useEffect(() => {
    if (id === undefined) {
      return
    }

    const emailId = id

    async function loadEmail() {
      try {
        setEmail(await getEmail(emailId))
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      }
    }

    void loadEmail()
  }, [id, notifyError, t])

  return (
    <main className="page courier-email-page">
      <div className="page-header">
        <h1 className="page-title">{t('features.courier.emails.pages.show')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.emails)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {email === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <div className="detail-grid-one-column">
              <div className="detail-grid-two-columns">
                <DetailField label={t('shared.fields.id')}>{email.id}</DetailField>
                <DetailField label={t('shared.fields.status')}>
                  <Badge className={getEmailStatusClassName(email.status)}>
                    {getEmailStatusLabel(email.status, t)}
                  </Badge>
                </DetailField>
                <DetailField label={t('shared.fields.organizationId')}>{email.organizationId}</DetailField>
                <DetailField label={t('shared.fields.userId')}>{email.userId}</DetailField>
                <DetailField label={t('shared.fields.module')}>{email.module}</DetailField>
                <DetailField label={t('shared.fields.feature')}>{email.feature}</DetailField>
                <DetailField label={t('shared.fields.templateKey')}>{email.templateKey}</DetailField>
                <DetailField label={t('shared.fields.recipient')}>{email.recipient}</DetailField>
                <DetailField label={t('shared.fields.createdAt')}>{formatUserDateTime(email.createdAt, user?.language)}</DetailField>
                <DetailField label={t('shared.fields.sentAt')}>{formatOptionalDate(email.sentAt, user?.language)}</DetailField>
                <DetailField label={t('shared.fields.expiresAt')}>{formatUserDateTime(email.expiresAt, user?.language)}</DetailField>
                <DetailField label={t('shared.fields.nextAttemptAt')}>{formatOptionalDate(email.nextAttemptAt, user?.language)}</DetailField>
                <DetailField label={t('shared.fields.retryCount')}>{email.retryCount}</DetailField>
                <DetailField label={t('shared.fields.isHtml')}>{email.isHtml ? t('shared.common.yes') : t('shared.common.no')}</DetailField>
              </div>
              <DetailField label={t('shared.fields.subject')}>{email.subject}</DetailField>
              <DetailField label={t('shared.fields.body')}>
                <pre className="detail-code email-body">{email.body}</pre>
              </DetailField>
              <div className="email-attempts">
                <h2 className="detail-section-title">{t('features.courier.emails.sections.attempts')}</h2>
                {email.attempts.length === 0 ? (
                  <p className="page-subtitle">{t('features.courier.emails.messages.noAttempts')}</p>
                ) : email.attempts.map((attempt, index) => (
                  <Card key={`${attempt.attemptedAt}.${index}`}>
                    <CardContent>
                      <div className="detail-grid-one-column">
                        <DetailField label={t('shared.fields.attemptedAt')}>
                          {formatUserDateTime(attempt.attemptedAt, user?.language)}
                        </DetailField>
                        <DetailField label={t('shared.fields.errorMessage')}>{attempt.errorMessage}</DetailField>
                        {attempt.stackTrace !== null && (
                          <DetailField label={t('shared.fields.stackTrace')}>
                            <pre className="detail-code">{attempt.stackTrace}</pre>
                          </DetailField>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>
    </main>
  )
}

type DetailFieldProps = {
  children: ReactNode
  label: string
}

function DetailField({ children, label }: DetailFieldProps) {
  return (
    <div className="detail-field">
      <span className="detail-label">{label}</span>
      <div className="detail-value">{children}</div>
    </div>
  )
}
