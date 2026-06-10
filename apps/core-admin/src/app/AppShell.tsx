import {
  Building2,
  ChevronDown,
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
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '../components/ui/dropdown-menu'
import { IAM_RESOURCES } from '../shared/iamConstants'
import { hasResourceAccess } from '../shared/permissions'
import { APP_CONSTANTS } from './appConstants'
import { useTranslate } from './i18n/i18n'
import { APP_ROUTES } from './routes'

const ICON_SIZE = 17

export function AppLayout() {
  const t = useTranslate()
  const navigate = useNavigate()
  const location = useLocation()
  const [isMobileOpen, setIsMobileOpen] = useState(false)
  const [isCollapsed, setIsCollapsed] = useState(false)
  const { isAuthenticated, logout, permissions, user } = useAuth()
  const canOpenOrganizations = hasResourceAccess(permissions, IAM_RESOURCES.organizations)
  const canOpenUsers = hasResourceAccess(permissions, IAM_RESOURCES.users)
  const canOpenParameters = hasResourceAccess(permissions, IAM_RESOURCES.parameters)
  const canOpenRoles = hasResourceAccess(permissions, IAM_RESOURCES.roles)
  const canOpenPermissions = hasResourceAccess(permissions, IAM_RESOURCES.permissions)
  const canOpenAuthorization = canOpenRoles || canOpenPermissions
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
    <div className={`shell ${isCollapsed ? 'shell-collapsed' : ''}`}>
      <aside className={`sidebar ${isMobileOpen ? 'sidebar-open' : ''}`}>
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
                  <ChevronDown className="sidebar-user-chevron" size={14} />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="start">
                <DropdownMenuItem onClick={handleOrganizationProfile}>
                  <UserRound size={16} />
                  {t('navigation.profile')}
                </DropdownMenuItem>
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
            icon={<Gauge size={ICON_SIZE} />}
            label={t('navigation.dashboard')}
            to={APP_ROUTES.dashboard}
          />
          <div className="menu-section-label">{t('navigation.groups.iam')}</div>
          {canOpenOrganizations && (
            <NavItem
              active={location.pathname.startsWith(APP_ROUTES.organizations)}
              icon={<Building2 size={ICON_SIZE} />}
              label={t('resources.iam.organizations.name')}
              to={APP_ROUTES.organizations}
            />
          )}
          {canOpenUsers && (
            <NavItem
              active={location.pathname.startsWith(APP_ROUTES.users)}
              icon={<Users size={ICON_SIZE} />}
              label={t('resources.iam.users.name')}
              to={APP_ROUTES.users}
            />
          )}
          {canOpenParameters && (
            <NavItem
              active={location.pathname.startsWith(APP_ROUTES.parameters)}
              icon={<Settings size={ICON_SIZE} />}
              label={t('resources.iam.parameters.name')}
              to={APP_ROUTES.parameters}
            />
          )}
          {canOpenAuthorization && (
            <>
              <div className="nav-group-title">
                <Shield size={ICON_SIZE} />
                <span>{t('navigation.groups.authorization')}</span>
              </div>
              <div className="nav-nested">
                {canOpenRoles && (
                  <NavItem
                    active={location.pathname.startsWith(APP_ROUTES.roles)}
                    icon={<UserCog size={ICON_SIZE} />}
                    label={t('resources.iam.roles.name')}
                    to={APP_ROUTES.roles}
                  />
                )}
                {canOpenPermissions && (
                  <NavItem
                    active={location.pathname.startsWith(APP_ROUTES.permissions)}
                    icon={<Key size={ICON_SIZE} />}
                    label={t('resources.iam.permissions.name')}
                    to={APP_ROUTES.permissions}
                  />
                )}
              </div>
            </>
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
              <Menu size={18} />
            </Button>
            <Button
              aria-label={t('navigation.toggleSidebar')}
              className="desktop-menu"
              onClick={() => setIsCollapsed((current) => !current)}
              size="icon"
              variant="ghost"
            >
              {isCollapsed ? <PanelLeftOpen size={18} /> : <PanelLeftClose size={18} />}
            </Button>
            <span className="shell-current-section">{APP_CONSTANTS.appName}</span>
          </div>
          <div className="header-right">
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button className="sidebar-user-button" variant="ghost">
                  <span className="sidebar-user-avatar">{user?.fullName.slice(0, 1)}</span>
                  <span className="sidebar-user-name">{user?.fullName}</span>
                  <ChevronDown className="sidebar-user-chevron" size={14} />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={handleUserProfile}>
                  <UserRound size={16} />
                  {t('resources.iam.users.pages.profile')}
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleLogout}>
                  <LogOut size={16} />
                  {t('auth.userMenu.logout')}
                </DropdownMenuItem>
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

type NavItemProps = {
  active: boolean
  icon: ReactNode
  label: string
  to: string
}

function NavItem({ active, icon, label, to }: NavItemProps) {
  const navigate = useNavigate()

  return (
    <button className={`nav-item ${active ? 'nav-item-active' : ''}`} onClick={() => navigate(to)} type="button">
      {icon}
      <span>{label}</span>
    </button>
  )
}
