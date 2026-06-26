import { ArrowLeft } from 'lucide-react'
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { getAuditLog } from './auditLogApi'
import type { AuditLogDto } from './auditLogTypes'
import { getAuditPrivacyLevelLabel } from './auditLogUi'

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
        notifyError(error, t('shared.errors.generic'))
      }
    }

    void loadAuditLog()
  }, [id, notifyError, t])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.sentinel.auditLogs.pages.show')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.auditLogs)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {auditLog === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <div className="detail-grid-one-column">
              <div className="detail-grid-two-columns">
                <DetailField label={t('shared.fields.id')}>{auditLog.id}</DetailField>
                <DetailField label={t('shared.fields.privacyLevel')}>{getAuditPrivacyLevelLabel(auditLog.privacyLevel, t)}</DetailField>
                <DetailField label={t('shared.fields.module')}>{auditLog.module}</DetailField>
                <DetailField label={t('shared.fields.feature')}>{auditLog.feature}</DetailField>
                <DetailField label={t('shared.fields.action')}>{auditLog.action}</DetailField>
                <DetailField label={t('shared.fields.userId')}>{auditLog.userId}</DetailField>
                <DetailField label={t('shared.fields.organizationId')}>{auditLog.organizationId}</DetailField>
                <DetailField label={t('shared.fields.targetId')}>{auditLog.targetId}</DetailField>
                <DetailField label={t('shared.fields.ipAddress')}>{auditLog.ipAddress ?? ''}</DetailField>
                <DetailField label={t('shared.fields.createdAt')}>{auditLog.createdAt}</DetailField>
                <DetailField label={t('shared.fields.expiresAt')}>{auditLog.expiresAt}</DetailField>
              </div>
              <DetailField label={t('shared.fields.description')}>{auditLog.description}</DetailField>
              <DetailField label={t('shared.fields.userAgent')}>{auditLog.userAgent ?? ''}</DetailField>
              <DetailField label={t('shared.fields.metadata')}>
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
