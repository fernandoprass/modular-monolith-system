import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings'
import BusinessIcon from '@mui/icons-material/Business'
import ExpandLessIcon from '@mui/icons-material/ExpandLess'
import ExpandMoreIcon from '@mui/icons-material/ExpandMore'
import KeyIcon from '@mui/icons-material/Key'
import ManageAccountsIcon from '@mui/icons-material/ManageAccounts'
import PeopleIcon from '@mui/icons-material/People'
import SettingsIcon from '@mui/icons-material/Settings'
import { Box, Collapse, List, ListItemIcon, MenuItem, Typography } from '@mui/material'
import { useState } from 'react'
import { Menu, usePermissions, useTranslate } from 'react-admin'

import { APP_ROUTES } from './routes'
import { IAM_RESOURCES } from '../shared/iamConstants'
import { hasResourceAccess, type PermissionDto } from '../shared/permissions'

export function AppMenu() {
  const translate = useTranslate()
  const { permissions } = usePermissions<PermissionDto[]>()
  const [isAuthorizationOpen, setIsAuthorizationOpen] = useState(false)
  const userPermissions = permissions ?? []
  const canOpenOrganizations = hasResourceAccess(userPermissions, IAM_RESOURCES.organizations)
  const canOpenUsers = hasResourceAccess(userPermissions, IAM_RESOURCES.users)
  const canOpenParameters = hasResourceAccess(userPermissions, IAM_RESOURCES.parameters)
  const canOpenRoles = hasResourceAccess(userPermissions, IAM_RESOURCES.roles)
  const canOpenPermissions = hasResourceAccess(userPermissions, IAM_RESOURCES.permissions)
  const canOpenAuthorization = canOpenRoles || canOpenPermissions

  return (
    <Menu>
      <Menu.DashboardItem />
      <Typography className="menu-section-label">
        {translate('navigation.groups.iam')}
      </Typography>
      {canOpenOrganizations && (
        <Menu.Item
          leftIcon={<BusinessIcon />}
          primaryText="resources.iam.organizations.name"
          to={APP_ROUTES.organizations}
        />
      )}
      {canOpenUsers && (
        <Menu.Item
          leftIcon={<PeopleIcon />}
          primaryText="resources.iam.users.name"
          to={APP_ROUTES.users}
        />
      )}
      {canOpenParameters && (
        <Menu.Item
          leftIcon={<SettingsIcon />}
          primaryText="resources.iam.parameters.name"
          to={APP_ROUTES.parameters}
        />
      )}
      {canOpenAuthorization && (
        <>
          <MenuItem onClick={() => setIsAuthorizationOpen((isOpen) => !isOpen)}>
            <ListItemIcon>
              <AdminPanelSettingsIcon />
            </ListItemIcon>
            <Typography noWrap sx={{ flexGrow: 1 }}>
              {translate('navigation.groups.authorization')}
            </Typography>
            {isAuthorizationOpen ? <ExpandLessIcon /> : <ExpandMoreIcon />}
          </MenuItem>
          <Collapse in={isAuthorizationOpen} timeout="auto" unmountOnExit>
            <List component="div" disablePadding>
              <Box className="menu-nested-items">
                {canOpenRoles && (
                  <Menu.Item
                    leftIcon={<ManageAccountsIcon />}
                    primaryText="resources.iam.roles.name"
                    to={APP_ROUTES.roles}
                  />
                )}
                {canOpenPermissions && (
                  <Menu.Item
                    leftIcon={<KeyIcon />}
                    primaryText="resources.iam.permissions.name"
                    to={APP_ROUTES.permissions}
                  />
                )}
              </Box>
            </List>
          </Collapse>
        </>
      )}
    </Menu>
  )
}
