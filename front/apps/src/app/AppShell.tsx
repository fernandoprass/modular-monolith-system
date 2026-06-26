import {
  Bell,
  Building2,
  ChevronDown,
  ChevronRight,
  FileText,
  Gauge,
  Key,
  LogOut,
  Mail,
  Menu,
  PanelLeftClose,
  PanelLeftOpen,
  LockKeyhole,
  ScrollText,
  Settings,
  Shield,
  ShieldCheck,
  UserCog,
  UserRound,
  Users,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { useEffect, useState } from 'react'
import { Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom'

import { useAuth } from '../auth/AuthProvider'
import { Button } from '../components/ui/button'
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuGroup,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '../components/ui/dropdown-menu'
import { cn } from '../lib/utils'
import { getUnreadNotificationCount } from '../resources/courier/notifications/notificationApi'
import { UserPasswordEditDialog } from '../resources/iam/users/UserPasswordEditDialog'
import { COURIER_PERMISSIONS } from '../shared/courierConstants'
import { IAM_PERMISSIONS, IAM_RESOURCES } from '../shared/iamConstants'
import { hasPermissionCode, hasResourceAccess } from '../shared/permissions'
import { SENTINEL_PERMISSIONS } from '../shared/sentinelConstants'
import { APP_CONSTANTS } from './appConstants'
import { useTranslate } from './i18n/i18n'
import { APP_ROUTES } from './routes'

export function AppLayout() {
  const t = useTranslate()
  const navigate = useNavigate()
  const location = useLocation()
  const [isMobileOpen, setIsMobileOpen] = useState(false)
  const [isCollapsed, setIsCollapsed] = useState(false)
  const [isAuthorizationOpen, setIsAuthorizationOpen] = useState(true)
  const [isCourierOpen, setIsCourierOpen] = useState(true)
  const [isIamOpen, setIsIamOpen] = useState(true)
  const [isSentinelOpen, setIsSentinelOpen] = useState(true)
  const [isPasswordDialogOpen, setIsPasswordDialogOpen] = useState(false)
  const [unreadNotificationCount, setUnreadNotificationCount] = useState(0)
  const { isAuthenticated, logout, permissions, user } = useAuth()
  const canOpenOrganizations = hasResourceAccess(permissions, IAM_RESOURCES.organizations)
  const canOpenUsers = hasResourceAccess(permissions, IAM_RESOURCES.users)
  const canOpenParameters = hasResourceAccess(permissions, IAM_RESOURCES.parameters)
  const canOpenRoles = hasResourceAccess(permissions, IAM_RESOURCES.roles)
  const canOpenPermissions = hasResourceAccess(permissions, IAM_RESOURCES.permissions)
  const canOpenUserAccess = hasPermissionCode(permissions, IAM_PERMISSIONS.roles.assign)
  const canOpenOrganizationSettings = hasPermissionCode(permissions, IAM_PERMISSIONS.organizationProfile.parameters)
  const canOpenUserSettings = hasPermissionCode(permissions, IAM_PERMISSIONS.userProfile.parameters)
  const canOpenAuditLogs = hasPermissionCode(permissions, SENTINEL_PERMISSIONS.auditLogs.read)
  const canOpenSystemLogs = hasPermissionCode(permissions, SENTINEL_PERMISSIONS.systemLogs.read)
  const canOpenEmails = hasPermissionCode(permissions, COURIER_PERMISSIONS.emails.read)
  const canOpenNotifications = hasPermissionCode(permissions, COURIER_PERMISSIONS.notifications.read)
  const canOpenTemplates = hasPermissionCode(permissions, COURIER_PERMISSIONS.templates.read)
  const canOpenAuthorization = canOpenRoles || canOpenPermissions || canOpenUserAccess
  const canOpenIam = canOpenOrganizations || canOpenUsers || canOpenParameters || canOpenAuthorization
  const canOpenCourier = canOpenEmails || canOpenNotifications || canOpenTemplates
  const canOpenSentinel = canOpenAuditLogs || canOpenSystemLogs
  const organizationName = user?.organizationName || APP_CONSTANTS.appName
  const canOpenOrganizationProfile = user?.isOrganizationAdmin === true
  const canOpenOrganizationMenu = canOpenOrganizationProfile || canOpenOrganizationSettings

  useEffect(() => {
    if (!isAuthenticated || !canOpenNotifications) {
      setUnreadNotificationCount(0)
      return
    }

    async function loadUnreadNotificationCount() {
      try {
        setUnreadNotificationCount(await getUnreadNotificationCount())
      } catch {
        setUnreadNotificationCount(0)
      }
    }

    void loadUnreadNotificationCount()
  }, [canOpenNotifications, isAuthenticated, location.pathname])

  if (!isAuthenticated) {
    return <Navigate to={APP_ROUTES.login} replace />
  }

  function handleLogout() {
    logout()
    navigate(APP_ROUTES.login)
  }

  function handleOrganizationProfile() {
    navigate(APP_ROUTES.organizationProfile)
  }

  function handleOrganizationSettings() {
    navigate(APP_ROUTES.organizationSettings)
  }

  function handleUserProfile() {
    navigate(APP_ROUTES.userProfile)
  }

  function handleUserSettings() {
    navigate(APP_ROUTES.userSettings)
  }

  return (
    <div className={cn('shell', isCollapsed && 'shell-collapsed')}>
      <aside className={cn('sidebar', isMobileOpen && 'sidebar-open')}>
        <div className="sidebar-header">
          {canOpenOrganizationMenu ? (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button className="sidebar-organization-button" variant="ghost">
                  <span className="sidebar-brand-mark">{organizationName.slice(0, 2)}</span>
                  <span className="sidebar-brand-text">
                    <span className="brand">{organizationName}</span>
                    <span>{APP_CONSTANTS.appName}</span>
                  </span>
                  <ChevronDown className="sidebar-user-chevron" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="start">
                <DropdownMenuGroup>
                  {canOpenOrganizationProfile && (
                    <DropdownMenuItem onClick={handleOrganizationProfile}>
                      <UserRound data-icon="inline-start" />
                      {t('navigation.profile')}
                    </DropdownMenuItem>
                  )}
                  {canOpenOrganizationSettings && (
                    <DropdownMenuItem onClick={handleOrganizationSettings}>
                      <Settings data-icon="inline-start" />
                      {t('navigation.settings')}
                    </DropdownMenuItem>
                  )}
                </DropdownMenuGroup>
              </DropdownMenuContent>
            </DropdownMenu>
          ) : (
            <>
              <div className="sidebar-brand-mark">{organizationName.slice(0, 2)}</div>
              <div className="sidebar-brand-text">
                <span className="brand">{organizationName}</span>
                <span>{APP_CONSTANTS.appName}</span>
              </div>
            </>
          )}
        </div>
        <nav className="sidebar-content">
          <NavItem
            active={location.pathname === APP_ROUTES.dashboard}
            icon={<Gauge data-icon="inline-start" />}
            label={t('navigation.dashboard')}
            to={APP_ROUTES.dashboard}
          />
          {canOpenIam && (
            <NavGroup
              icon={<Shield data-icon="inline-start" />}
              isOpen={isIamOpen}
              label={t('navigation.groups.iam')}
              onToggle={() => setIsIamOpen((current) => !current)}
            >
              {canOpenOrganizations && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.organizations)}
                  icon={<Building2 data-icon="inline-start" />}
                  label={t('features.iam.organizations.name')}
                  to={APP_ROUTES.organizations}
                />
              )}
              {canOpenUsers && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.users)}
                  icon={<Users data-icon="inline-start" />}
                  label={t('features.iam.users.name')}
                  to={APP_ROUTES.users}
                />
              )}
              {canOpenParameters && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.parameters)}
                  icon={<Settings data-icon="inline-start" />}
                  label={t('features.iam.parameters.name')}
                  to={APP_ROUTES.parameters}
                />
              )}
              {canOpenAuthorization && (
                <NavGroup
                  icon={<Key data-icon="inline-start" />}
                  isNested
                  isOpen={isAuthorizationOpen}
                  label={t('navigation.groups.authorization')}
                  onToggle={() => setIsAuthorizationOpen((current) => !current)}
                >
                {canOpenRoles && (
                  <NavItem
                    active={location.pathname.startsWith(APP_ROUTES.roles)}
                    icon={<UserCog data-icon="inline-start" />}
                    label={t('features.iam.roles.name')}
                    to={APP_ROUTES.roles}
                  />
                )}
                {canOpenPermissions && (
                  <NavItem
                    active={location.pathname.startsWith(APP_ROUTES.permissions)}
                    icon={<Key data-icon="inline-start" />}
                    label={t('features.iam.permissions.name')}
                    to={APP_ROUTES.permissions}
                  />
                )}
                {canOpenUserAccess && (
                  <NavItem
                    active={location.pathname.startsWith(APP_ROUTES.userAccess)}
                    icon={<ShieldCheck data-icon="inline-start" />}
                    label={t('features.iam.userAccess.name')}
                    to={APP_ROUTES.userAccess}
                  />
                )}
                </NavGroup>
              )}
            </NavGroup>
          )}
          {canOpenCourier && (
            <NavGroup
              icon={<Mail data-icon="inline-start" />}
              isOpen={isCourierOpen}
              label={t('navigation.groups.courier')}
              onToggle={() => setIsCourierOpen((current) => !current)}
            >
              {canOpenEmails && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.emails)}
                  icon={<Mail data-icon="inline-start" />}
                  label={t('features.courier.emails.name')}
                  to={APP_ROUTES.emails}
                />
              )}
              {canOpenNotifications && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.notifications)}
                  icon={<Bell data-icon="inline-start" />}
                  label={t('features.courier.notifications.name')}
                  to={APP_ROUTES.notifications}
                />
              )}
              {canOpenTemplates && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.templates)}
                  icon={<FileText data-icon="inline-start" />}
                  label={t('features.courier.templates.name')}
                  to={APP_ROUTES.templates}
                />
              )}
            </NavGroup>
          )}
          {canOpenSentinel && (
            <NavGroup
              icon={<ScrollText data-icon="inline-start" />}
              isOpen={isSentinelOpen}
              label={t('navigation.groups.sentinel')}
              onToggle={() => setIsSentinelOpen((current) => !current)}
            >
              {canOpenAuditLogs && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.auditLogs)}
                  icon={<ScrollText data-icon="inline-start" />}
                  label={t('features.sentinel.auditLogs.name')}
                  to={APP_ROUTES.auditLogs}
                />
              )}
              {canOpenSystemLogs && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.systemLogs)}
                  icon={<ScrollText data-icon="inline-start" />}
                  label={t('features.sentinel.systemLogs.name')}
                  to={APP_ROUTES.systemLogs}
                />
              )}
            </NavGroup>
          )}
        </nav>
        <div className="sidebar-footer">
          <span className="shell-current-section">{APP_CONSTANTS.appName}</span>
        </div>
      </aside>
      <div className="shell-inset">
        <header className="shell-topbar">
          <div className="header-left">
            <Button
              aria-label={t('navigation.menu')}
              className="mobile-menu"
              onClick={() => setIsMobileOpen((current) => !current)}
              size="icon"
              variant="ghost"
            >
              <Menu data-icon="inline-start" />
            </Button>
            <Button
              aria-label={t('navigation.toggleSidebar')}
              className="desktop-menu"
              onClick={() => setIsCollapsed((current) => !current)}
              size="icon"
              variant="ghost"
            >
              {isCollapsed ? <PanelLeftOpen data-icon="inline-start" /> : <PanelLeftClose data-icon="inline-start" />}
            </Button>
            <span className="shell-current-section">{APP_CONSTANTS.appName}</span>
          </div>
          <div className="header-right">
            {canOpenNotifications && (
              <Button
                aria-label={t('features.courier.notifications.pages.list')}
                className="notification-bell-button"
                onClick={() => navigate(APP_ROUTES.notifications)}
                size="icon"
                type="button"
                variant="ghost"
              >
                <Bell data-icon="inline-start" />
                {unreadNotificationCount > 0 && (
                  <span className="notification-count-badge">
                    {unreadNotificationCount > 99 ? '99+' : unreadNotificationCount}
                  </span>
                )}
              </Button>
            )}
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button className="sidebar-user-button" variant="ghost">
                  <span className="sidebar-user-avatar">{user?.fullName.slice(0, 1)}</span>
                  <span className="sidebar-user-name">{user?.fullName}</span>
                  <ChevronDown className="sidebar-user-chevron" />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuGroup>
                  <DropdownMenuItem onClick={handleUserProfile}>
                    <UserRound data-icon="inline-start" />
                    {t('features.iam.users.pages.profile')}
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => setIsPasswordDialogOpen(true)}>
                    <LockKeyhole data-icon="inline-start" />
                    {t('features.iam.users.pages.changePassword')}
                  </DropdownMenuItem>
                  {canOpenUserSettings && (
                    <DropdownMenuItem onClick={handleUserSettings}>
                      <Settings data-icon="inline-start" />
                      {t('navigation.settings')}
                    </DropdownMenuItem>
                  )}
                  <DropdownMenuItem onClick={handleLogout}>
                    <LogOut data-icon="inline-start" />
                    {t('auth.userMenu.logout')}
                  </DropdownMenuItem>
                </DropdownMenuGroup>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        </header>
        <main className="shell-main">
          <Outlet />
        </main>
      </div>
      {isMobileOpen && (
        <button
          aria-label={t('navigation.toggleSidebar')}
          className="sidebar-scrim"
          onClick={() => setIsMobileOpen(false)}
          type="button"
        />
      )}
      <UserPasswordEditDialog
        isOpen={isPasswordDialogOpen}
        onClose={() => setIsPasswordDialogOpen(false)}
      />
    </div>
  )
}

type NavGroupProps = {
  children: ReactNode
  icon: ReactNode
  isNested?: boolean
  isOpen: boolean
  label: string
  onToggle: () => void
}

function NavGroup({ children, icon, isNested = false, isOpen, label, onToggle }: NavGroupProps) {
  return (
    <div className={cn('nav-group', isNested && 'nav-group-nested')}>
      <button
        aria-expanded={isOpen}
        className={cn('nav-group-title', isOpen && 'nav-group-title-open')}
        onClick={onToggle}
        type="button"
      >
        {icon}
        <span>{label}</span>
        {isOpen ? (
          <ChevronDown className="nav-group-chevron" data-icon="inline-end" />
        ) : (
          <ChevronRight className="nav-group-chevron" data-icon="inline-end" />
        )}
      </button>
      {isOpen && <div className="nav-nested">{children}</div>}
    </div>
  )
}

type NavItemProps = {
  active: boolean
  icon: ReactNode
  label: string
  to: string
}

function NavItem({ active, icon, label, to }: NavItemProps) {
  const navigate = useNavigate()

  return (
    <button className={cn('nav-item', active && 'nav-item-active')} onClick={() => navigate(to)} type="button">
      {icon}
      <span>{label}</span>
    </button>
  )
}
