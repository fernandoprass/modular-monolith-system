import { Navigate, useNavigate, useParams } from 'react-router-dom'

import { useAuth } from '../auth/AuthProvider'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs'
import { UserPreferencePage } from '../resources/courier/user-preferences/UserPreferencePage'
import { ParameterSettingsPage } from '../resources/iam/parameters/ParameterSettingsPage'
import { UserProfileSection, UserSecuritySection } from '../resources/iam/users/UserProfilePage'
import { COURIER_PERMISSIONS } from '../shared/courierConstants'
import { IAM_PERMISSIONS } from '../shared/iamConstants'
import { hasPermissionCode } from '../shared/permissions'
import { useTranslate } from './i18n/i18n'
import { APP_ROUTES } from './routes'

const USER_ACCOUNT_SECTIONS = {
  communication: 'communication',
  profile: 'profile',
  security: 'security',
  settings: 'settings',
} as const

type UserAccountSection = typeof USER_ACCOUNT_SECTIONS[keyof typeof USER_ACCOUNT_SECTIONS]

export function UserAccountPage() {
  const t = useTranslate()
  const navigate = useNavigate()
  const { permissions } = useAuth()
  const { section } = useParams<{ section: string }>()
  const canViewSecurity = hasPermissionCode(permissions, IAM_PERMISSIONS.userProfile.viewAccess)
  const canViewSettings = hasPermissionCode(permissions, IAM_PERMISSIONS.userProfile.parameters)
  const canViewCommunication = hasPermissionCode(permissions, COURIER_PERMISSIONS.userPreferences.read)
  const availableSections: UserAccountSection[] = [
    USER_ACCOUNT_SECTIONS.profile,
    ...(canViewSecurity ? [USER_ACCOUNT_SECTIONS.security] : []),
    ...(canViewSettings ? [USER_ACCOUNT_SECTIONS.settings] : []),
    ...(canViewCommunication ? [USER_ACCOUNT_SECTIONS.communication] : []),
  ]

  if (!availableSections.includes(section as UserAccountSection)) {
    return <Navigate replace to={APP_ROUTES.userProfile} />
  }

  return (
    <main className="page user-account-page">
      <div className="page-header">
        <h1 className="page-title">{t('features.iam.users.account.title')}</h1>
      </div>
      <Tabs
        className="user-account-tabs"
        onValueChange={(value) => navigate(APP_ROUTES.userProfileSection(value))}
        value={section}
      >
        <TabsList>
          <TabsTrigger value={USER_ACCOUNT_SECTIONS.profile}>
            {t('features.iam.users.account.profile')}
          </TabsTrigger>
          {canViewSecurity && (
            <TabsTrigger value={USER_ACCOUNT_SECTIONS.security}>
              {t('features.iam.users.account.security')}
            </TabsTrigger>
          )}
          {canViewSettings && (
            <TabsTrigger value={USER_ACCOUNT_SECTIONS.settings}>
              {t('features.iam.users.account.settings')}
            </TabsTrigger>
          )}
          {canViewCommunication && (
            <TabsTrigger value={USER_ACCOUNT_SECTIONS.communication}>
              {t('features.iam.users.account.communication')}
            </TabsTrigger>
          )}
        </TabsList>
        <TabsContent value={USER_ACCOUNT_SECTIONS.profile}>
          <UserProfileSection />
        </TabsContent>
        {canViewSecurity && (
          <TabsContent value={USER_ACCOUNT_SECTIONS.security}>
            <UserSecuritySection />
          </TabsContent>
        )}
        {canViewSettings && (
          <TabsContent value={USER_ACCOUNT_SECTIONS.settings}>
            <ParameterSettingsPage embedded owner="user" />
          </TabsContent>
        )}
        {canViewCommunication && (
          <TabsContent value={USER_ACCOUNT_SECTIONS.communication}>
            <UserPreferencePage embedded />
          </TabsContent>
        )}
      </Tabs>
    </main>
  )
}
