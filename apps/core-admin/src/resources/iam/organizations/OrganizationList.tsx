import {
  BooleanField,
  Datagrid,
  DeleteButton,
  EditButton,
  List,
  ShowButton,
  SelectField,
  TextField,
  TextInput,
  usePermissions,
} from 'react-admin'

import { IAM_PERMISSIONS } from '../../../shared/iamConstants'
import { languageChoices } from '../../../shared/languages'
import { hasPermissionCode, type PermissionDto } from '../../../shared/permissions'
import { organizationTypeChoices } from './organizationTypes'

const organizationFilters = [
  <TextInput key="Code" source="Code" />,
  <TextInput key="Name" source="Name" />,
]

function OrganizationRowActions() {
  const { permissions } = usePermissions<PermissionDto[]>()
  const userPermissions = permissions ?? []
  const canView = hasPermissionCode(userPermissions, IAM_PERMISSIONS.organizations.view)
  const canUpdate = hasPermissionCode(userPermissions, IAM_PERMISSIONS.organizations.update)
  const canDelete = hasPermissionCode(userPermissions, IAM_PERMISSIONS.organizations.delete)

  return (
    <>
      {canView && <ShowButton />}
      {canUpdate && <EditButton />}
      {canDelete && <DeleteButton mutationMode="pessimistic" />}
    </>
  )
}

export function OrganizationList() {
  return (
    <List
      filters={organizationFilters}
      perPage={25}
      sort={{ field: 'name', order: 'ASC' }}
    >
      <Datagrid bulkActionButtons={false}>
        <SelectField choices={organizationTypeChoices} source="type" />
        <TextField source="code" />
        <TextField source="name" />
        <SelectField choices={languageChoices} source="defaultLanguage" />
        <BooleanField source="isActive" />
        <OrganizationRowActions />
      </Datagrid>
    </List>
  )
}
