import { createTheme } from '@mantine/core'

export const appTheme = createTheme({
  fontFamily: 'Inter, Roboto, Arial, sans-serif',
  primaryColor: 'blue',
  defaultRadius: 'sm',
  components: {
    Button: {
      defaultProps: {
        size: 'xs',
      },
    },
    ActionIcon: {
      defaultProps: {
        size: 'sm',
        variant: 'subtle',
      },
    },
    TextInput: {
      defaultProps: {
        size: 'xs',
      },
    },
    PasswordInput: {
      defaultProps: {
        size: 'xs',
      },
    },
    Select: {
      defaultProps: {
        size: 'xs',
      },
    },
  },
})
