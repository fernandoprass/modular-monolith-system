import { RESOURCE_NAMES } from '../../../shared/resourceNames'
import { OrganizationCreate } from './OrganizationCreate'
import { OrganizationEdit } from './OrganizationEdit'
import { OrganizationList } from './OrganizationList'
import { OrganizationShow } from './OrganizationShow'

export const organizationResource = {
  create: OrganizationCreate,
  edit: OrganizationEdit,
  list: OrganizationList,
  name: RESOURCE_NAMES.organizations,
  show: OrganizationShow,
} as const
