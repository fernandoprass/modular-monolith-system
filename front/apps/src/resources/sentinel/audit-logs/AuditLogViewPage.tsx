import { formatUserDateTime } from '../../../shared/dateFormat'
import { ArrowLeft } from 'lucide-react'
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useLocale, useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { getAuditLog } from './auditLogApi'
import type { AuditLogDto } from './auditLogTypes'
import { getAuditPrivacyLevelEnum } from './auditLogUi'

function formatMetadata(value: string): string {
  if (value.trim().length === 0) {
    return ''
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

export function AuditLogViewPage() {
  const t = useTranslate()
  const locale = useLocale()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { id } = useParams()
  const [auditLog, setAuditLog] = useState<AuditLogDto | null>(null)

  useEffect(() => {
    if (id === undefined) {
      return
    }

    const auditLogId = id

    async function loadAuditLog() {
      try {
        setAuditLog(await getAuditLog(auditLogId))
      } catch (error) {
        notifyError(error, t('common.messages.errors.generic'))
      }
    }

    void loadAuditLog()
  }, [id, notifyError, t])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('common.pages.show', { entity: t('features.sentinel.labels.auditLog') })}</h1>
        <Button onClick={() => navigate(APP_ROUTES.auditLogs)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('common.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {auditLog === null ? (
            <p className="page-subtitle">{t('common.states.loading')}</p>
          ) : (
            <div className="detail-grid-one-column">
              <div className="detail-grid-two-columns">
                <DetailField label={t('common.labels.id')}>{auditLog.id}</DetailField>
                <DetailField label={t('features.sentinel.labels.privacyLevel')}>{getAuditPrivacyLevelEnum(auditLog.privacyLevel, t)}</DetailField>
                <DetailField label={t('common.labels.module')}>{auditLog.module}</DetailField>
                <DetailField label={t('common.labels.feature')}>{auditLog.feature}</DetailField>
                <DetailField label={t('common.labels.action')}>{auditLog.action}</DetailField>
                <DetailField label={t('features.iam.labels.userId')}>{auditLog.userId}</DetailField>
                <DetailField label={t('features.iam.labels.organizationId')}>{auditLog.organizationId}</DetailField>
                <DetailField label={t('features.sentinel.labels.targetId')}>{auditLog.targetId}</DetailField>
                <DetailField label={t('features.sentinel.labels.ipAddress')}>{auditLog.ipAddress ?? ''}</DetailField>
                <DetailField label={t('common.labels.createdAt')}>{formatUserDateTime(auditLog.createdAt, locale)}</DetailField>
                <DetailField label={t('common.labels.expiresAt')}>{formatUserDateTime(auditLog.expiresAt, locale)}</DetailField>
              </div>
              <DetailField label={t('common.labels.description')}>{auditLog.description}</DetailField>
              <DetailField label={t('features.sentinel.labels.userAgent')}>{auditLog.userAgent ?? ''}</DetailField>
              <DetailField label={t('features.sentinel.labels.metadata')}>
                <pre className="detail-code">{formatMetadata(auditLog.metadata)}</pre>
              </DetailField>
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
