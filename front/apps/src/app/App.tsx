import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'

import { AuthProvider } from '../auth/AuthProvider'
import { LoginPage } from '../auth/LoginPage'
import { AuditLogListPage } from '../resources/sentinel/audit-logs/AuditLogListPage'
import { AuditLogViewPage } from '../resources/sentinel/audit-logs/AuditLogViewPage'
import { SystemLogListPage } from '../resources/sentinel/system-logs/SystemLogListPage'
import { SystemLogViewPage } from '../resources/sentinel/system-logs/SystemLogViewPage'
import { OrganizationEditPage } from '../resources/iam/organizations/OrganizationEditPage'
import { OrganizationListPage } from '../resources/iam/organizations/OrganizationListPage'
import { OrganizationProfilePage } from '../resources/iam/organizations/OrganizationProfilePage'
import { OrganizationViewPage } from '../resources/iam/organizations/OrganizationViewPage'
import { PublicOrganizationCreatePage } from '../resources/iam/organizations/PublicOrganizationCreatePage'
import { ParameterEditPage } from '../resources/iam/parameters/ParameterEditPage'
import { ParameterListPage } from '../resources/iam/parameters/ParameterListPage'
import { ParameterSettingsPage } from '../resources/iam/parameters/ParameterSettingsPage'
import { PermissionListPage } from '../resources/iam/permissions/PermissionListPage'
import { RoleEditPage } from '../resources/iam/roles/RoleEditPage'
import { RoleListPage } from '../resources/iam/roles/RoleListPage'
import { UserAccessPage } from '../resources/iam/user-access/UserAccessPage'
import { UserEditPage } from '../resources/iam/users/UserEditPage'
import { UserListPage } from '../resources/iam/users/UserListPage'
import { UserProfilePage } from '../resources/iam/users/UserProfilePage'
import { UserViewPage } from '../resources/iam/users/UserViewPage'
import { ToastProvider } from './ToastProvider'
import { AppLayout } from './AppShell'
import { I18nContext, translate } from './i18n/i18n'
import { APP_ROUTES } from './routes'

function App() {
  return (
    <I18nContext.Provider value={translate}>
      <ToastProvider>
        <BrowserRouter>
          <AuthProvider>
            <Routes>
              <Route path={APP_ROUTES.login} element={<LoginPage />} />
              <Route path={APP_ROUTES.registerOrganization} element={<PublicOrganizationCreatePage />} />
              <Route element={<AppLayout />}>
                <Route index element={<DashboardPage />} />
                <Route path={APP_ROUTES.organizationProfile.slice(1)} element={<OrganizationProfilePage />} />
                <Route path={APP_ROUTES.organizationSettings.slice(1)} element={<ParameterSettingsPage owner="organization" />} />
                <Route path={APP_ROUTES.organizations.slice(1)} element={<OrganizationListPage />} />
                <Route path="organizations/:id" element={<OrganizationEditPage />} />
                <Route path="organizations/:id/show" element={<OrganizationViewPage />} />
                <Route path={APP_ROUTES.users.slice(1)} element={<UserListPage />} />
                <Route path="users/create" element={<UserEditPage />} />
                <Route path="users/:id" element={<UserEditPage />} />
                <Route path="users/:id/show" element={<UserViewPage />} />
                <Route path={APP_ROUTES.userProfile.slice(1)} element={<UserProfilePage />} />
                <Route path={APP_ROUTES.userSettings.slice(1)} element={<ParameterSettingsPage owner="user" />} />
                <Route path={APP_ROUTES.auditLogs.slice(1)} element={<AuditLogListPage />} />
                <Route path="audit-logs/:id/show" element={<AuditLogViewPage />} />
                <Route path={APP_ROUTES.systemLogs.slice(1)} element={<SystemLogListPage />} />
                <Route path="system-logs/:id/show" element={<SystemLogViewPage />} />
                <Route path={APP_ROUTES.roles.slice(1)} element={<RoleListPage />} />
                <Route path="roles/create" element={<RoleEditPage />} />
                <Route path="roles/:id" element={<RoleEditPage />} />
                <Route path={APP_ROUTES.userAccess.slice(1)} element={<UserAccessPage />} />
                <Route path={APP_ROUTES.parameters.slice(1)} element={<ParameterListPage />} />
                <Route path="parameters/:id" element={<ParameterEditPage />} />
                <Route path={APP_ROUTES.permissions.slice(1)} element={<PermissionListPage />} />
                <Route path="*" element={<Navigate to={APP_ROUTES.dashboard} replace />} />
              </Route>
            </Routes>
          </AuthProvider>
        </BrowserRouter>
      </ToastProvider>
    </I18nContext.Provider>
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
