import {
  useState,
  type FormEvent,
} from 'react'
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  Stack,
  TextField,
  Typography,
} from '@mui/material'
import LockOutlinedIcon
  from '@mui/icons-material/LockOutlined'
import {
  Navigate,
  useNavigate,
} from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

export function LoginPage() {
  const {
    login,
    isAuthenticated,
  } = useAuth()

  const navigate =
    useNavigate()

  const [email, setEmail] =
    useState('admin@atlanticcity.pe')

  const [password, setPassword] =
    useState('')

  const [error, setError] =
    useState<string | null>(null)

  const [loading, setLoading] =
    useState(false)

  if (isAuthenticated) {
    return (
      <Navigate
        to="/"
        replace
      />
    )
  }

  async function handleSubmit(
    event: FormEvent<HTMLFormElement>,
  ) {
    event.preventDefault()

    setLoading(true)
    setError(null)

    try {
      await login(
        email,
        password,
      )

      navigate(
        '/',
        {
          replace: true,
        },
      )
    } catch {
      setError(
        'No fue posible iniciar sesión. Verifica tus credenciales.',
      )
    } finally {
      setLoading(false)
    }
  }

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'grid',
        placeItems: 'center',
        p: 3,
        background:
          'linear-gradient(135deg, #0d1729 0%, #172b4d 55%, #263b5e 100%)',
      }}
    >
      <Paper
        elevation={10}
        sx={{
          width: '100%',
          maxWidth: 430,
          p: {
            xs: 3,
            sm: 5,
          },
        }}
      >
        <Stack
          component="form"
          spacing={3}
          onSubmit={handleSubmit}
        >
          <Box>
            <Box
              sx={{
                width: 48,
                height: 48,
                borderRadius: 2,
                display: 'grid',
                placeItems: 'center',
                bgcolor: 'primary.main',
                color: 'white',
                mb: 2,
              }}
            >
              <LockOutlinedIcon />
            </Box>

            <Typography variant="h4">
              Atlantic City
            </Typography>

            <Typography
              color="text.secondary"
              sx={{
                mt: 0.5,
              }}
            >
              Plataforma de cargas masivas
            </Typography>
          </Box>

          {error && (
            <Alert severity="error">
              {error}
            </Alert>
          )}

          <TextField
            label="Correo electrónico"
            type="email"
            value={email}
            onChange={(event) =>
              setEmail(
                event.target.value,
              )
            }
            autoComplete="username"
            required
            fullWidth
          />

          <TextField
            label="Contraseña"
            type="password"
            value={password}
            onChange={(event) =>
              setPassword(
                event.target.value,
              )
            }
            autoComplete="current-password"
            required
            fullWidth
          />

          <Button
            type="submit"
            variant="contained"
            size="large"
            disabled={loading}
            startIcon={
              loading
                ? (
                    <CircularProgress
                      size={18}
                      color="inherit"
                    />
                  )
                : undefined
            }
          >
            {loading
              ? 'Ingresando...'
              : 'Iniciar sesión'}
          </Button>
        </Stack>
      </Paper>
    </Box>
  )
}