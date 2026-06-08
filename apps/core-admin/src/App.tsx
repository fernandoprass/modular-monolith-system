import { Admin, CustomRoutes, Resource, Title, useTranslate } from 'react-admin'
import { Route } from 'react-router-dom'

import { APP_CONSTANTS } from './app/appConstants'
import { AppLayout } from './app/AppLayout'
import { i18nProvider } from './app/i18n/i18nProvider'
import { APP_ROUTES } from './app/routes'
import { appTheme } from './app/theme'
import { authProvider } from './auth/authProvider'
import { LoginPage } from './auth/LoginPage'
import { dataProvider } from './data/dataProvider'
import { organizationResource } from './resources/iam/organizations/organizationResource'
import { PublicOrganizationCreatePage } from './resources/iam/organizations/PublicOrganizationCreatePage'

function Dashboard() {
  const translate = useTranslate()

  return (
    <>
      <Title title={translate('app.dashboard.title')} />
      <main className="dashboard">
        <h1>{translate('app.dashboard.title')}</h1>
        <p>{translate('app.dashboard.subtitle')}</p>
      </main>
    </>
  )
}

function App() {
  if (window.location.pathname === APP_ROUTES.registerOrganization) {
    return <PublicOrganizationCreatePage />
  }

  return (
    <Admin
      authProvider={authProvider}
      dashboard={Dashboard}
      dataProvider={dataProvider}
      i18nProvider={i18nProvider}
      layout={AppLayout}
      loginPage={LoginPage}
      theme={appTheme}
      title={APP_CONSTANTS.appName}
    >
      <Resource {...organizationResource} />
      <CustomRoutes>
        <Route path="/" element={<Dashboard />} />
      </CustomRoutes>
    </Admin>
  )
}

export default App
