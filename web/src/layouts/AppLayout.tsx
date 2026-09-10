import {
  Avatar,
  Box,
  Button,
  Divider,
  Typography,
} from '@mui/material'
import DashboardOutlinedIcon
  from '@mui/icons-material/DashboardOutlined'
import LogoutOutlinedIcon
  from '@mui/icons-material/LogoutOutlined'
import {
  Outlet,
  useLocation,
  useNavigate,
} from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

const sidebarWidth = 230

export function AppLayout() {
  const {
    session,
    logout,
  } = useAuth()

  const navigate =
    useNavigate()

  const location =
    useLocation()

  const displayName =
    session?.user.displayName ??
    'Usuario'

  return (
    <Box
      sx={{
        minHeight: '100vh',
        bgcolor: 'background.default',
      }}
    >
      <Box
        component="aside"
        sx={{
          position: 'fixed',
          inset: '0 auto 0 0',
          width: sidebarWidth,
          bgcolor: 'primary.main',
          color: 'white',
          display: {
            xs: 'none',
            md: 'flex',
          },
          flexDirection: 'column',
          zIndex: 10,
        }}
      >
        <Box
          sx={{
            px: 3,
            py: 3,
          }}
        >
          <Typography
            variant="h5"
            sx={{
              fontWeight: 800,
            }}
          >
            Atlantic City
          </Typography>

          <Typography
            variant="caption"
            sx={{
              opacity: 0.72,
            }}
          >
            Plataforma de cargas masivas
          </Typography>
        </Box>

        <Divider
          sx={{
            borderColor:
              'rgba(255,255,255,0.12)',
          }}
        />

        <Box
          component="nav"
          sx={{
            p: 2,
          }}
        >
          <Button
            fullWidth
            startIcon={
              <DashboardOutlinedIcon />
            }
            onClick={() =>
              navigate('/')
            }
            sx={{
              justifyContent: 'flex-start',
              px: 2,
              py: 1.25,
              color: 'white',
              bgcolor:
                location.pathname === '/' ||
                    location.pathname.startsWith(
                    '/loads/',
                    )
                  ? 'rgba(255,255,255,0.14)'
                  : 'transparent',

              '&:hover': {
                bgcolor:
                  'rgba(255,255,255,0.10)',
              },
            }}
          >
            Cargas
          </Button>
        </Box>

        <Box
          sx={{
            mt: 'auto',
            p: 2,
          }}
        >
          <Divider
            sx={{
              mb: 2,
              borderColor:
                'rgba(255,255,255,0.12)',
            }}
          />

          <Box
            sx={{
              display: 'flex',
              alignItems: 'center',
              gap: 1.5,
              mb: 2,
            }}
          >
            <Avatar
              sx={{
                width: 36,
                height: 36,
                bgcolor:
                  'secondary.main',
              }}
            >
              {displayName
                .charAt(0)
                .toUpperCase()}
            </Avatar>

            <Box
              sx={{
                minWidth: 0,
              }}
            >
              <Typography
                variant="body2"
                sx={{
                  fontWeight: 700,
                }}
                noWrap
              >
                {displayName}
              </Typography>

              <Typography
                variant="caption"
                sx={{
                  opacity: 0.7,
                }}
                noWrap
              >
                {session?.user.role}
              </Typography>
            </Box>
          </Box>

          <Button
            fullWidth
            startIcon={
              <LogoutOutlinedIcon />
            }
            onClick={() => {
              void logout()
            }}
            sx={{
              color:
                'rgba(255,255,255,0.85)',
              justifyContent:
                'flex-start',
            }}
          >
            Cerrar sesión
          </Button>
        </Box>
      </Box>

      <Box
        sx={{
          ml: {
            xs: 0,
            md: `${sidebarWidth}px`,
          },
        }}
      >
        <Box
          component="header"
          sx={{
            height: 70,
            px: {
              xs: 2,
              md: 4,
            },
            bgcolor: 'white',
            borderBottom:
              '1px solid',
            borderColor: 'divider',
            display: 'flex',
            alignItems: 'center',
            justifyContent:
              'space-between',
          }}
        >
          <Box>
            <Typography
              variant="body2"
              color="text.secondary"
            >
              Gestión operativa
            </Typography>

            <Typography
              sx={{
                fontWeight: 700,
              }}
            >
              Carga masiva de productos
            </Typography>
          </Box>

          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              display: {
                xs: 'none',
                sm: 'block',
              },
            }}
          >
            {session?.user.email}
          </Typography>
        </Box>

        <Box
          component="main"
          sx={{
            p: {
              xs: 2,
              md: 4,
            },
          }}
        >
          <Outlet />
        </Box>
      </Box>
    </Box>
  )
}