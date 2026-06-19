import { ArrowLeft } from 'lucide-react'
import { useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import { useNavigate, useParams } from 'react-router-dom'

import { useTranslate } from '../../../app/i18n/i18n'
import { APP_ROUTES } from '../../../app/routes'
import { useNotifyError } from '../../../auth/AuthProvider'
import { Button } from '../../../components/ui/button'
import { Card, CardContent } from '../../../components/ui/card'
import { getSystemLog } from './systemLogApi'
import type { SystemLogDto } from './systemLogTypes'
import { getSystemLogLevelLabel, getSystemLogStatusLabel } from './systemLogUi'

function formatJson(value: string): string {
  if (value.trim().length === 0) {
    return ''
  }

  try {
    return JSON.stringify(JSON.parse(value), null, 2)
  } catch {
    return value
  }
}

export function SystemLogViewPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const notifyError = useNotifyError()
  const { id } = useParams()
  const [systemLog, setSystemLog] = useState<SystemLogDto | null>(null)

  useEffect(() => {
    if (id === undefined) {
      return
    }

    const systemLogId = id

    async function loadSystemLog() {
      try {
        setSystemLog(await getSystemLog(systemLogId))
      } catch (error) {
        notifyError(error, t('shared.errors.generic'))
      }
    }

    void loadSystemLog()
  }, [id, notifyError, t])

  return (
    <main className="page">
      <div className="page-header">
        <h1 className="page-title">{t('features.sentinel.systemLogs.pages.show')}</h1>
        <Button onClick={() => navigate(APP_ROUTES.systemLogs)} type="button" variant="outline">
          <ArrowLeft data-icon="inline-start" />
          {t('shared.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {systemLog === null ? (
            <p className="page-subtitle">{t('shared.common.loading')}</p>
          ) : (
            <div className="detail-grid-one-column">
              <div className="detail-grid-two-columns">
                <DetailField label={t('shared.fields.id')}>{systemLog.id}</DetailField>
                <DetailField label={t('shared.fields.level')}>{getSystemLogLevelLabel(systemLog.level, t)}</DetailField>
                <DetailField label={t('shared.fields.status')}>{getSystemLogStatusLabel(systemLog.status, t)}</DetailField>
                <DetailField label={t('shared.fields.module')}>{systemLog.module}</DetailField>
                <DetailField label={t('shared.fields.createdAt')}>{systemLog.createdAt}</DetailField>
                <DetailField label={t('shared.fields.expiresAt')}>{systemLog.expiresAt}</DetailField>
                <DetailField label={t('shared.fields.requestId')}>{systemLog.requestId ?? ''}</DetailField>
                <DetailField label={t('shared.fields.userId')}>{systemLog.userId ?? ''}</DetailField>
                <DetailField label={t('shared.fields.organizationId')}>{systemLog.organizationId ?? ''}</DetailField>
              </div>
              <DetailField label={t('shared.fields.message')}>{systemLog.message}</DetailField>
              <DetailField label={t('shared.fields.exception')}>{systemLog.exception ?? ''}</DetailField>
              <DetailField label={t('shared.fields.stackTrace')}>
                <pre className="detail-code">{systemLog.stackTrace ?? ''}</pre>
              </DetailField>
              <DetailField label={t('shared.fields.propertiesJson')}>
                <pre className="detail-code">{formatJson(systemLog.propertiesJson)}</pre>
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
