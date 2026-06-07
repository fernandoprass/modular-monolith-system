import { Stack } from '@mui/material'
import {
  Create,
  PasswordInput,
  required,
  SelectInput,
  SimpleForm,
  TextInput,
} from 'react-admin'
import { useWatch } from 'react-hook-form'

import { languageChoices } from '../../../shared/languages'
import { RESOURCE_NAMES } from '../../../shared/resourceNames'
import { ORGANIZATION_TYPES, organizationTypeChoices } from './organizationTypes'

function OrganizationFields() {
  const organizationType = useWatch({ name: 'type' })
  const isCompany = organizationType === ORGANIZATION_TYPES.company

  return (
    <>
      <SelectInput
        choices={organizationTypeChoices}
        defaultValue={ORGANIZATION_TYPES.company}
        source="type"
        validate={required()}
      />
      {isCompany && (
        <Stack
          alignItems={{ xs: 'stretch', sm: 'flex-start' }}
          direction={{ xs: 'column', sm: 'row' }}
          spacing={1}
        >
          <TextInput source="code" validate={required()} />
          <TextInput source="name" validate={required()} />
        </Stack>
      )}
      <TextInput source="description" validate={required()} />
      <SelectInput
        choices={languageChoices}
        defaultValue="en"
        source="defaultLanguage"
        validate={required()}
      />
      <TextInput source="userName" validate={required()} />
      <TextInput source="userEmail" type="email" validate={required()} />
      <PasswordInput source="userPassword" validate={required()} />
    </>
  )
}

export function OrganizationCreate() {
  return (
    <Create mutationMode="pessimistic" resource={RESOURCE_NAMES.organizations}>
      <SimpleForm>
        <OrganizationFields />
      </SimpleForm>
    </Create>
  )
}
