import { RESOURCE_LABELS } from './app'
import { ORGANIZATION_TYPE_ENUM } from './enums'

export const iamMessages = {
  organizations: {
    messages: {
      deleteConfirm: 'Delete this organization?',
      empty: 'No organizations found.',
    },
    name: RESOURCE_LABELS.iam.organizations,
    pages: {
      edit: 'Edit organization',
      list: RESOURCE_LABELS.iam.organizations,
      profile: RESOURCE_LABELS.iam.organizationProfile,
      show: 'Organization details',
    },
    placeholders: {
      search: 'Search organization',
    },
    types: ORGANIZATION_TYPE_ENUM,
  },
  parameters: {
    name: RESOURCE_LABELS.iam.parameters,
    messages: {
      empty: 'No parameters found.',
    },
    pages: {
      edit: 'Edit parameter',
      list: RESOURCE_LABELS.iam.parameters,
      organizationSettings: 'Settings',
      userSettings: 'Settings',
    },
  },
  permissions: {
    messages: {
      empty: 'No permissions found.',
    },
    name: RESOURCE_LABELS.iam.permissions,
    pages: {
      list: RESOURCE_LABELS.iam.permissions,
    },
  },
  roles: {
    labels: {
      assignedPermissions: 'Assigned permissions',
      availablePermissions: 'Available permissions',
    },
    messages: {
      deleteConfirm: 'Delete this role?',
      empty: 'No roles found.',
    },
    name: RESOURCE_LABELS.iam.roles,
    pages: {
      create: 'Create role',
      edit: 'Edit role',
      list: RESOURCE_LABELS.iam.roles,
    },
  },
  users: {
    account: {
      communication: 'Communication',
      profile: 'Profile',
      security: 'Security',
      settings: 'Settings',
      title: 'User profile',
    },
    messages: {
      deleteConfirm: 'Delete this user?',
      empty: 'No users found.',
      noPermissions: 'No permissions found.',
      noRoles: 'No roles found.',
      organizationRequired: 'Select an organization.',
      passwordsDoNotMatch: 'Passwords do not match.',
    },
    name: RESOURCE_LABELS.iam.users,
    pages: {
      changePassword: 'Change password',
      create: 'Create user',
      edit: 'Edit user',
      list: RESOURCE_LABELS.iam.users,
      profile: RESOURCE_LABELS.iam.userProfile,
      show: 'User details',
    },
    placeholders: {
      search: 'Search user',
    },
    tabs: {
      permissions: 'Permissions',
      roles: 'Roles',
    },
  },
  userAccess: {
    labels: {
      assignedRoles: 'Assigned roles',
      availableRoles: 'Available roles',
    },
    messages: {
      noUserSelected: 'Select a user to manage access.',
      noRoles: 'No roles found.',
    },
    name: 'User Access',
    pages: {
      list: 'User Access',
    },
  },
} as const
