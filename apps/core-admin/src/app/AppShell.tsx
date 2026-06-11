import {
  Building2,
  ChevronDown,
  ChevronRight,
  Gauge,
  Key,
  LogOut,
  Menu,
  PanelLeftClose,
  PanelLeftOpen,
  Settings,
  Shield,
  UserCog,
  UserRound,
  Users,
} from 'lucide-react'
import type { ReactNode } from 'react'
import { useState } from 'react'
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
import { IAM_RESOURCES } from '../shared/iamConstants'
import { hasResourceAccess } from '../shared/permissions'
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
  const [isIamOpen, setIsIamOpen] = useState(true)
  const { isAuthenticated, logout, permissions, user } = useAuth()
  const canOpenOrganizations = hasResourceAccess(permissions, IAM_RESOURCES.organizations)
  const canOpenUsers = hasResourceAccess(permissions, IAM_RESOURCES.users)
  const canOpenParameters = hasResourceAccess(permissions, IAM_RESOURCES.parameters)
  const canOpenRoles = hasResourceAccess(permissions, IAM_RESOURCES.roles)
  const canOpenPermissions = hasResourceAccess(permissions, IAM_RESOURCES.permissions)
  const canOpenAuthorization = canOpenRoles || canOpenPermissions
  const canOpenIam = canOpenOrganizations || canOpenUsers || canOpenParameters || canOpenAuthorization
  const organizationName = user?.organizationName || APP_CONSTANTS.appName
  const canOpenOrganizationProfile = user?.isOrganizationAdmin === true

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

  function handleUserProfile() {
    navigate(APP_ROUTES.userProfile)
  }

  return (
    <div className={cn('shell', isCollapsed && 'shell-collapsed')}>
      <aside className={cn('sidebar', isMobileOpen && 'sidebar-open')}>
        <div className="sidebar-header">
          {canOpenOrganizationProfile ? (
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
                  <DropdownMenuItem onClick={handleOrganizationProfile}>
                    <UserRound data-icon="inline-start" />
                    {t('navigation.profile')}
                  </DropdownMenuItem>
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
                  label={t('resources.iam.organizations.name')}
                  to={APP_ROUTES.organizations}
                />
              )}
              {canOpenUsers && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.users)}
                  icon={<Users data-icon="inline-start" />}
                  label={t('resources.iam.users.name')}
                  to={APP_ROUTES.users}
                />
              )}
              {canOpenParameters && (
                <NavItem
                  active={location.pathname.startsWith(APP_ROUTES.parameters)}
                  icon={<Settings data-icon="inline-start" />}
                  label={t('resources.iam.parameters.name')}
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
                    label={t('resources.iam.roles.name')}
                    to={APP_ROUTES.roles}
                  />
                )}
                {canOpenPermissions && (
                  <NavItem
                    active={location.pathname.startsWith(APP_ROUTES.permissions)}
                    icon={<Key data-icon="inline-start" />}
                    label={t('resources.iam.permissions.name')}
                    to={APP_ROUTES.permissions}
                  />
                )}
                </NavGroup>
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
                    {t('resources.iam.users.pages.profile')}
                  </DropdownMenuItem>
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
