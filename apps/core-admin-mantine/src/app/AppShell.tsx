import {
  AppShell as MantineAppShell,
  Burger,
  Group,
  Menu,
  NavLink,
  Text,
  UnstyledButton,
} from '@mantine/core'
import { useDisclosure } from '@mantine/hooks'
import {
  IconBuilding,
  IconChevronDown,
  IconDashboard,
  IconKey,
  IconLogout,
  IconSettings,
  IconShieldLock,
  IconUserCog,
  IconUsers,
} from '@tabler/icons-react'
import { Navigate, Outlet, useLocation, useNavigate } from 'react-router-dom'
import type { ReactNode } from 'react'

import { useAuth } from '../auth/AuthProvider'
import { IAM_RESOURCES } from '../shared/iamConstants'
import { hasResourceAccess } from '../shared/permissions'
import { APP_CONSTANTS } from './appConstants'
import { useTranslate } from './i18n/i18n'
import { APP_ROUTES } from './routes'

const ICON_SIZE = 18

export function AppLayout() {
  const t = useTranslate()
  const navigate = useNavigate()
  const location = useLocation()
  const [opened, { toggle }] = useDisclosure()
  const { isAuthenticated, logout, permissions, user } = useAuth()
  const canOpenOrganizations = hasResourceAccess(permissions, IAM_RESOURCES.organizations)
  const canOpenUsers = hasResourceAccess(permissions, IAM_RESOURCES.users)
  const canOpenParameters = hasResourceAccess(permissions, IAM_RESOURCES.parameters)
  const canOpenRoles = hasResourceAccess(permissions, IAM_RESOURCES.roles)
  const canOpenPermissions = hasResourceAccess(permissions, IAM_RESOURCES.permissions)
  const canOpenAuthorization = canOpenRoles || canOpenPermissions

  if (!isAuthenticated) {
    return <Navigate to={APP_ROUTES.login} replace />
  }

  function handleLogout() {
    logout()
    navigate(APP_ROUTES.login)
  }

  return (
    <MantineAppShell
      header={{ height: 48 }}
      navbar={{
        breakpoint: 'sm',
        collapsed: { mobile: !opened },
        width: 240,
      }}
      padding="md"
    >
      <MantineAppShell.Header>
        <Group h="100%" px="sm" justify="space-between">
          <Group gap="xs">
            <Burger opened={opened} onClick={toggle} hiddenFrom="sm" size="sm" />
            <Text fw={700} size="sm">{APP_CONSTANTS.appName}</Text>
          </Group>
          <Menu position="bottom-end" width={180}>
            <Menu.Target>
              <UnstyledButton className="user-menu-button">
                <Group gap={6}>
                  <Text size="sm" fw={500}>{user?.fullName}</Text>
                  <IconChevronDown size={14} />
                </Group>
              </UnstyledButton>
            </Menu.Target>
            <Menu.Dropdown>
              <Menu.Item leftSection={<IconLogout size={ICON_SIZE} />} onClick={handleLogout}>
                {t('auth.userMenu.logout')}
              </Menu.Item>
            </Menu.Dropdown>
          </Menu>
        </Group>
      </MantineAppShell.Header>

      <MantineAppShell.Navbar p="xs">
        <NavLink
          active={location.pathname === APP_ROUTES.dashboard}
          href={APP_ROUTES.dashboard}
          label={t('navigation.dashboard')}
          leftSection={<IconDashboard size={ICON_SIZE} />}
          onClick={(event) => {
            event.preventDefault()
            navigate(APP_ROUTES.dashboard)
          }}
        />
        <Text className="menu-section-label">{t('navigation.groups.iam')}</Text>
        {canOpenOrganizations && (
          <NavItem
            active={location.pathname.startsWith(APP_ROUTES.organizations)}
            icon={<IconBuilding size={ICON_SIZE} />}
            label={t('resources.iam.organizations.name')}
            to={APP_ROUTES.organizations}
          />
        )}
        {canOpenUsers && (
          <NavItem
            active={location.pathname.startsWith(APP_ROUTES.users)}
            icon={<IconUsers size={ICON_SIZE} />}
            label={t('resources.iam.users.name')}
            to={APP_ROUTES.users}
          />
        )}
        {canOpenParameters && (
          <NavItem
            active={location.pathname.startsWith(APP_ROUTES.parameters)}
            icon={<IconSettings size={ICON_SIZE} />}
            label={t('resources.iam.parameters.name')}
            to={APP_ROUTES.parameters}
          />
        )}
        {canOpenAuthorization && (
          <NavLink
            defaultOpened
            label={t('navigation.groups.authorization')}
            leftSection={<IconShieldLock size={ICON_SIZE} />}
          >
            {canOpenRoles && (
              <NavItem
                active={location.pathname.startsWith(APP_ROUTES.roles)}
                icon={<IconUserCog size={ICON_SIZE} />}
                label={t('resources.iam.roles.name')}
                to={APP_ROUTES.roles}
              />
            )}
            {canOpenPermissions && (
              <NavItem
                active={location.pathname.startsWith(APP_ROUTES.permissions)}
                icon={<IconKey size={ICON_SIZE} />}
                label={t('resources.iam.permissions.name')}
                to={APP_ROUTES.permissions}
              />
            )}
          </NavLink>
        )}
      </MantineAppShell.Navbar>

      <MantineAppShell.Main>
        <Outlet />
      </MantineAppShell.Main>
    </MantineAppShell>
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
    <NavLink
      active={active}
      href={to}
      label={label}
      leftSection={icon}
      onClick={(event) => {
        event.preventDefault()
        navigate(to)
      }}
    />
  )
}
