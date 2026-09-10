import {
  createTheme,
} from '@mui/material/styles'

export const theme =
  createTheme({
    palette: {
      mode: 'light',

      primary: {
        main: '#13233f',
      },

      secondary: {
        main: '#b58b3a',
      },

      background: {
        default: '#f4f6f8',
      },
    },

    shape: {
      borderRadius: 10,
    },

    typography: {
      fontFamily:
        '"Inter", "Segoe UI", sans-serif',

      h4: {
        fontWeight: 700,
      },

      h5: {
        fontWeight: 700,
      },

      button: {
        textTransform: 'none',
        fontWeight: 600,
      },
    },
  })