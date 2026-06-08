import { MantineProvider } from '@mantine/core'
import { ModalsProvider } from '@mantine/modals'
import { Notifications } from '@mantine/notifications'
import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'

import { AuthProvider } from '../auth/AuthProvider'
import { LoginPage } from '../auth/LoginPage'
import { OrganizationEditPage } from '../resources/iam/organizations/OrganizationEditPage'
import { OrganizationListPage } from '../resources/iam/organizations/OrganizationListPage'
import { OrganizationShowPage } from '../resources/iam/organizations/OrganizationShowPage'
import { PublicOrganizationCreatePage } from '../resources/iam/organizations/PublicOrganizationCreatePage'
import { I18nContext, translate } from './i18n/i18n'
import { AppLayout } from './AppShell'
import { APP_ROUTES } from './routes'
import { appTheme } from './theme'

function App() {
  return (
    <MantineProvider theme={appTheme}>
      <ModalsProvider>
        <Notifications position="top-right" />
        <I18nContext.Provider value={translate}>
          <BrowserRouter>
            <AuthProvider>
              <Routes>
                <Route path={APP_ROUTES.login} element={<LoginPage />} />
                <Route path={APP_ROUTES.registerOrganization} element={<PublicOrganizationCreatePage />} />
                <Route element={<AppLayout />}>
                  <Route index element={<DashboardPage />} />
                  <Route path={APP_ROUTES.organizations.slice(1)} element={<OrganizationListPage />} />
                  <Route path="organizations/:id" element={<OrganizationEditPage />} />
                  <Route path="organizations/:id/show" element={<OrganizationShowPage />} />
                  <Route path="*" element={<Navigate to={APP_ROUTES.dashboard} replace />} />
                </Route>
              </Routes>
            </AuthProvider>
          </BrowserRouter>
        </I18nContext.Provider>
      </ModalsProvider>
    </MantineProvider>
  )
}

function DashboardPage() {
  const t = translate

  return (
    <main className="page">
      <h1 className="page-title">{t('app.dashboard.title')}</h1>
      <p className="page-subtitle">{t('app.dashboard.subtitle')}</p>
    </main>
  )
}

export default App
