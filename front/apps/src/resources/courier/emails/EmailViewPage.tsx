import { ArrowLeft } from 'lucide-react'
import { useEffect, useState, type ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useLocale, useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Badge } from '../../../components/ui/badge'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { formatUserDateTime } from '../../../shared/dateFormat'
import { getEmail } from './emailApi'
import type { EmailDto } from './emailTypes'
import { getEmailStatusClassName, getEmailStatusEnum } from './emailUi'

function formatOptionalDate(value: string | null, language?: string): string {
  return value === null ? '' : formatUserDateTime(value, language)
}

export function EmailViewPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const locale = useLocale()
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
        notifyError(error, t('common.messages.errors.generic'))
      }
    }

    void loadEmail()
  }, [id, notifyError, t])

  return (
    <main className="page courier-email-page">
      <div className="page-header">
        <h1 className="page-title">{t('common.pages.show', { entity: t('common.labels.email') })}</h1>
        <Button onClick={() => navigate(APP_ROUTES.emails)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('common.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {email === null ? (
            <p className="page-subtitle">{t('common.states.loading')}</p>
          ) : (
            <div className="detail-grid-one-column">
              <div className="detail-grid-two-columns">
                <DetailField label={t('common.labels.id')}>{email.id}</DetailField>
                <DetailField label={t('common.labels.status')}>
                  <Badge className={getEmailStatusClassName(email.status)}>
                    {getEmailStatusEnum(email.status, t)}
                  </Badge>
                </DetailField>
                <DetailField label={t('features.iam.labels.organizationId')}>{email.organizationId}</DetailField>
                <DetailField label={t('features.iam.labels.userId')}>{email.userId}</DetailField>
                <DetailField label={t('common.labels.module')}>{email.module}</DetailField>
                <DetailField label={t('common.labels.feature')}>{email.feature}</DetailField>
                <DetailField label={t('features.courier.labels.templateKey')}>{email.templateKey}</DetailField>
                <DetailField label={t('features.courier.labels.recipient')}>{email.recipient}</DetailField>
                <DetailField label={t('common.labels.createdAt')}>{formatUserDateTime(email.createdAt, locale)}</DetailField>
                <DetailField label={t('features.courier.labels.sentAt')}>{formatOptionalDate(email.sentAt, locale)}</DetailField>
                <DetailField label={t('common.labels.expiresAt')}>{formatUserDateTime(email.expiresAt, locale)}</DetailField>
                <DetailField label={t('features.courier.labels.nextAttemptAt')}>{formatOptionalDate(email.nextAttemptAt, locale)}</DetailField>
                <DetailField label={t('features.courier.labels.retryCount')}>{email.retryCount}</DetailField>
                <DetailField label={t('features.courier.labels.isHtml')}>{email.isHtml ? t('common.states.yes') : t('common.states.no')}</DetailField>
              </div>
              <DetailField label={t('features.courier.labels.subject')}>{email.subject}</DetailField>
              <DetailField label={t('features.courier.labels.body')}>
                <pre className="detail-code email-body">{email.body}</pre>
              </DetailField>
              <div className="email-attempts">
                <h2 className="detail-section-title">{t('features.courier.labels.deliveryAttempts')}</h2>
                {email.attempts.length === 0 ? (
                  <p className="page-subtitle">{t('features.courier.messages.emails.noAttempts')}</p>
                ) : email.attempts.map((attempt, index) => (
                  <Card key={`${attempt.attemptedAt}.${index}`}>
                    <CardContent>
                      <div className="detail-grid-one-column">
                        <DetailField label={t('features.courier.labels.attemptedAt')}>
                          {formatUserDateTime(attempt.attemptedAt, locale)}
                        </DetailField>
                        <DetailField label={t('common.labels.errorMessage')}>{attempt.errorMessage}</DetailField>
                        {attempt.stackTrace !== null && (
                          <DetailField label={t('common.labels.stackTrace')}>
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
