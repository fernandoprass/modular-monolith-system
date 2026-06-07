import { Stack } from '@mui/material'
import { BooleanInput, Edit, SelectInput, SimpleForm, TextInput } from 'react-admin'

import { languageChoices } from '../../../shared/languages'
import { OrganizationCodeEditButton } from './OrganizationCodeEditButton'

export function OrganizationEdit() {
  return (
    <Edit mutationMode="pessimistic">
      <SimpleForm>
        <Stack
          alignItems={{ xs: 'stretch', sm: 'flex-start' }}
          direction={{ xs: 'column', sm: 'row' }}
          spacing={1}
        >
          <TextInput disabled source="code" sx={{ flex: 1 }} />
          <OrganizationCodeEditButton />
        </Stack>
        <TextInput source="name" />
        <TextInput source="description" />
        <SelectInput choices={languageChoices} source="defaultLanguage" />
        <BooleanInput source="isActive" />
      </SimpleForm>
    </Edit>
  )
}
