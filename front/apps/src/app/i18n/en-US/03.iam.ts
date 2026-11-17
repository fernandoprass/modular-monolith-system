import { ORGANIZATION_TYPE_ENUM } from './enums'

export const iamMessages = {
  actions: {
    backToSignIn: 'Back to sign in',
    changePassword: 'Change password',
    logout: 'Logout',
    signIn: 'Sign in',
  },
  enums: {
    organizationType: ORGANIZATION_TYPE_ENUM,
  },
  labels: {
    adminEmail: 'Admin email',
    adminName: 'Admin name',
    adminPassword: 'Admin password',
    assignedAt: 'Assigned at',
    assignedBy: 'Assigned by',
    assignedPermissions: 'Assigned permissions',
    assignedRoles: 'Assigned roles',
    availablePermissions: 'Available permissions',
    availableRoles: 'Available roles',
    confirmPassword: 'Confirm password',
    currentPassword: 'Current password',
    externalListEndpoint: 'External list endpoint',
    isOrganizationAdmin: 'Organization admin',
    isSystemAdmin: 'System admin',
    listItems: 'List items',
    newPassword: 'New password',
    organization: 'Organization',
    organizationId: 'Organization ID',
    organizationProfile: 'Organization profile',
    organizations: 'Organizations',
    overrideType: 'Override type',
    parameter: 'Parameter',
    parameters: 'Parameters',
    password: 'Password',
    permission: 'Permission',
    permissions: 'Permissions',
    role: 'Role',
    roles: 'Roles',
    userAccess: 'User Access',
    userId: 'User ID',
    userProfile: 'User profile',
  },
  messages: {
    login: {
      signUp: 'New here? Sign up',
    },
    userAccess: {
      noUserSelected: 'Select a user to manage access.',
    },
    users: {
      organizationRequired: 'Select an organization.',
      passwordsDoNotMatch: 'Passwords do not match.',
    },
  },
} as const
