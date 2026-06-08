import { API_PATHS } from '../../../data/apiPaths'
import { postJson, unwrapResult } from '../../../data/httpClient'
import type {
  OrganizationCreateForm,
  OrganizationCreateRequest,
  OrganizationDto,
} from './organizationTypes'
import { ORGANIZATION_REQUEST_FIELDS } from './organizationTypes'

export function toOrganizationCreateRequest(data: OrganizationCreateForm): OrganizationCreateRequest {
  return {
    [ORGANIZATION_REQUEST_FIELDS.type]: data.type,
    [ORGANIZATION_REQUEST_FIELDS.name]: data.name,
    [ORGANIZATION_REQUEST_FIELDS.code]: data.code,
    [ORGANIZATION_REQUEST_FIELDS.description]: data.description,
    [ORGANIZATION_REQUEST_FIELDS.defaultLanguage]: data.defaultLanguage,
    [ORGANIZATION_REQUEST_FIELDS.user]: {
      [ORGANIZATION_REQUEST_FIELDS.userName]: data.userName,
      [ORGANIZATION_REQUEST_FIELDS.userEmail]: data.userEmail,
      [ORGANIZATION_REQUEST_FIELDS.userPassword]: data.userPassword,
    },
  }
}

export async function createOrganization(data: OrganizationCreateForm): Promise<OrganizationDto> {
  const response = await postJson(
    API_PATHS.iam.organizations.list,
    toOrganizationCreateRequest(data),
  )

  return unwrapResult<OrganizationDto>(response)
}
