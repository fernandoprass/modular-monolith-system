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
import { getSystemLog } from './systemLogApi'
import type { SystemLogDto } from './systemLogTypes'
import { getSystemLogLevelEnum, getSystemLogStatusEnum } from './systemLogUi'

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
  const locale = useLocale()
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
        notifyError(error, t('common.errors.generic'))
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
          {t('common.actions.back')}
        </Button>
      </div>
      <Card>
        <CardContent>
          {systemLog === null ? (
            <p className="page-subtitle">{t('common.states.loading')}</p>
          ) : (
            <div className="detail-grid-one-column">
              <div className="detail-grid-two-columns">
                <DetailField label={t('common.fields.id')}>{systemLog.id}</DetailField>
                <DetailField label={t('common.fields.level')}>{getSystemLogLevelEnum(systemLog.level, t)}</DetailField>
                <DetailField label={t('common.fields.status')}>{getSystemLogStatusEnum(systemLog.status, t)}</DetailField>
                <DetailField label={t('common.fields.module')}>{systemLog.module}</DetailField>
                <DetailField label={t('common.fields.createdAt')}>{formatUserDateTime(systemLog.createdAt, locale)}</DetailField>
                <DetailField label={t('common.fields.expiresAt')}>{formatUserDateTime(systemLog.expiresAt, locale)}</DetailField>
                <DetailField label={t('common.fields.requestId')}>{systemLog.requestId ?? ''}</DetailField>
                <DetailField label={t('common.fields.userId')}>{systemLog.userId ?? ''}</DetailField>
                <DetailField label={t('common.fields.organizationId')}>{systemLog.organizationId ?? ''}</DetailField>
              </div>
              <DetailField label={t('common.fields.message')}>{systemLog.message}</DetailField>
              <DetailField label={t('common.fields.exception')}>{systemLog.exception ?? ''}</DetailField>
              <DetailField label={t('common.fields.stackTrace')}>
                <pre className="detail-code">{systemLog.stackTrace ?? ''}</pre>
              </DetailField>
              <DetailField label={t('common.fields.propertiesJson')}>
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
