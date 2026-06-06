import { Admin, CustomRoutes, Layout, Title, useTranslate } from 'react-admin'
import { Route } from 'react-router-dom'

import { APP_CONSTANTS } from './app/appConstants'
import { i18nProvider } from './app/i18n/i18nProvider'
import { appTheme } from './app/theme'
import { authProvider } from './auth/authProvider'
import { LoginPage } from './auth/LoginPage'
import { dataProvider } from './data/dataProvider'

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
  return (
    <Admin
      authProvider={authProvider}
      dashboard={Dashboard}
      dataProvider={dataProvider}
      i18nProvider={i18nProvider}
      layout={Layout}
      loginPage={LoginPage}
      theme={appTheme}
      title={APP_CONSTANTS.appName}
    >
      <CustomRoutes>
        <Route path="/" element={<Dashboard />} />
      </CustomRoutes>
    </Admin>
  )
}

export default App
