import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Paper,
  Pagination,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import RefreshIcon
  from '@mui/icons-material/Refresh'
import {
  useMemo,
  useState,
} from 'react'
import {
  LoadResultChip,
  LoadStatusChip,
} from '../features/loads/LoadChips'
import {
  shouldPollLoad,
  useLoads,
} from '../features/loads/useLoads'

import { useNavigate }
  from 'react-router-dom'

const pageSize = 10

const dateFormatter =
  new Intl.DateTimeFormat(
    'es-PE',
    {
      dateStyle: 'short',
      timeStyle: 'short',
    },
  )

function formatDate(
  value: string | null,
): string {
  if (!value) {
    return '-'
  }

  return dateFormatter.format(
    new Date(value),
  )
}

export function LoadsPage() {
  const navigate =
    useNavigate()

  const [page, setPage] =
    useState(1)

  const {
    data,
    isLoading,
    isError,
    isFetching,
    refetch,
  } = useLoads({
    page,
    pageSize,
  })

  const items =
    useMemo(
      () => data?.items ?? [],
      [data],
    )

  const activeLoads =
    items.filter(
        shouldPollLoad,
    ).length

  const observations =
    items.filter(
      (load) =>
        load.result === 'Partial' ||
        load.result === 'Rejected' ||
        load.result === 'Failed',
    ).length

  return (
    <Box>
      <Box
        sx={{
          mb: 3,
          display: 'flex',
          justifyContent:
            'space-between',
          alignItems: {
            xs: 'flex-start',
            sm: 'center',
          },
          gap: 2,
          flexDirection: {
            xs: 'column',
            sm: 'row',
          },
        }}
      >
        <Box>
          <Typography
            variant="h4"
            sx={{
              mb: 0.5,
            }}
          >
            Historial de cargas
          </Typography>

          <Typography
            color="text.secondary"
          >
            Consulta el estado y resultado
            de los archivos procesados.
          </Typography>
        </Box>

        <Box
            sx={{
                display: 'flex',
                gap: 1.5,
                flexWrap: 'wrap',
            }}
            >
            <Button
                variant="outlined"
                startIcon={
                isFetching
                    ? (
                        <CircularProgress
                        size={16}
                        />
                    )
                    : (
                        <RefreshIcon />
                    )
                }
                onClick={() => {
                void refetch()
                }}
                disabled={isFetching}
            >
                {isFetching
                ? 'Actualizando...'
                : 'Actualizar'}
            </Button>

            <Button
                variant="contained"
                onClick={() =>
                navigate('/loads/new')
                }
            >
                + Nueva carga
            </Button>
        </Box>
      </Box>

      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            sm:
              'repeat(2, minmax(0, 1fr))',
            lg:
              'repeat(4, minmax(0, 1fr))',
          },
          gap: 2,
          mb: 3,
        }}
      >
        <MetricCard
          label="Total de cargas"
          value={
            data?.totalItems ?? 0
          }
        />

        <MetricCard
          label="Mostradas"
          value={items.length}
        />

        <MetricCard
          label="En proceso"
          value={activeLoads}
        />

        <MetricCard
          label="Con observaciones"
          value={observations}
        />
      </Box>

      {isError && (
        <Alert
          severity="error"
          sx={{
            mb: 3,
          }}
        >
          No fue posible consultar
          las cargas.
        </Alert>
      )}

      <Paper
        variant="outlined"
        sx={{
          overflow: 'hidden',
        }}
      >
        <Box
          sx={{
            px: 3,
            py: 2,
            borderBottom:
              '1px solid',
            borderColor: 'divider',
          }}
        >
          <Typography
            sx={{
              fontWeight: 700,
            }}
          >
            Archivos procesados
          </Typography>

          <Typography
            variant="body2"
            color="text.secondary"
          >
            {data?.totalItems ?? 0}
            {' '}
            registros encontrados
          </Typography>
        </Box>

        {isLoading ? (
          <Box
            sx={{
              minHeight: 280,
              display: 'grid',
              placeItems: 'center',
            }}
          >
            <CircularProgress />
          </Box>
        ) : (
          <TableContainer>
            <Table>
              <TableHead>
                <TableRow>
                  <TableCell>
                    Archivo
                  </TableCell>

                  <TableCell>
                    Período
                  </TableCell>

                  <TableCell>
                    Estado
                  </TableCell>

                  <TableCell>
                    Resultado
                  </TableCell>

                  <TableCell
                    align="right"
                  >
                    Registros
                  </TableCell>

                  <TableCell>
                    Fecha
                  </TableCell>

                  <TableCell>
                    Usuario
                  </TableCell>

                  <TableCell
                    align="right"
                    sx={{
                        width: 130,
                    }}
                    >
                    Acciones
                  </TableCell>
                </TableRow>
              </TableHead>

              <TableBody>
                {items.map(
                  (load) => (
                    <TableRow
                        key={load.id}
                        hover
                        tabIndex={0}
                        onClick={() =>
                            navigate(`/loads/${load.id}`)
                        }
                        onKeyDown={(event) => {
                            if (
                            event.key === 'Enter' ||
                            event.key === ' '
                            ) {
                            event.preventDefault()
                            navigate(`/loads/${load.id}`)
                            }
                        }}
                        sx={{
                            cursor: 'pointer',

                            '&:nth-of-type(odd)': {
                            backgroundColor: '#ffffff',
                            },

                            '&:nth-of-type(even)': {
                            backgroundColor: '#f8fafc',
                            },

                            '&:hover': {
                            backgroundColor: '#eef4ff',
                            },
                        }}
                    >
                      <TableCell>
                        <Typography
                          variant="body2"
                          sx={{
                            fontWeight: 600,
                          }}
                        >
                          {load.fileName}
                        </Typography>

                        <Typography
                          variant="caption"
                          color="text.secondary"
                        >
                          {load.id}
                        </Typography>
                      </TableCell>

                      <TableCell>
                        {load.period ?? '-'}
                      </TableCell>

                      <TableCell>
                        <LoadStatusChip
                          status={
                            load.status
                          }
                        />
                      </TableCell>

                      <TableCell>
                        <LoadResultChip
                          result={
                            load.result
                          }
                        />
                      </TableCell>

                      <TableCell
                        align="right"
                      >
                        <Typography
                          variant="body2"
                        >
                          {
                            load.totalRows
                          }
                        </Typography>

                        <Typography
                          variant="caption"
                          color="text.secondary"
                        >
                          {
                            load.insertedRows
                          }
                          {' '}
                          insertados
                        </Typography>
                      </TableCell>

                      <TableCell>
                        {formatDate(
                          load.createdAt,
                        )}
                      </TableCell>

                      <TableCell>
                        <Typography
                            variant="body2"
                        >
                            {load.userEmail}
                        </Typography>
                        </TableCell>

                        <TableCell align="right">
                        <Button
                            size="small"
                            variant="outlined"
                            onClick={(event) => {
                            event.stopPropagation()
                            navigate(`/loads/${load.id}`)
                            }}
                            aria-label={
                            `Ver detalle de ${load.fileName}`
                            }
                        >
                            Ver detalle →
                        </Button>
                        </TableCell>
                    </TableRow>
                  ),
                )}

                {items.length === 0 && (
                  <TableRow>
                    <TableCell
                      colSpan={8}
                      align="center"
                    >
                      <Box
                        sx={{
                          py: 6,
                        }}
                      >
                        <Typography
                          color="text.secondary"
                        >
                          No existen cargas
                          registradas.
                        </Typography>
                      </Box>
                    </TableCell>
                  </TableRow>
                )}
              </TableBody>
            </Table>
          </TableContainer>
        )}

        {(data?.totalPages ?? 0) >
          1 && (
          <Box
            sx={{
              px: 3,
              py: 2,
              display: 'flex',
              justifyContent:
                'flex-end',
              borderTop:
                '1px solid',
              borderColor:
                'divider',
            }}
          >
            <Pagination
              page={page}
              count={
                data?.totalPages ?? 1
              }
              onChange={(
                _,
                nextPage,
              ) =>
                setPage(nextPage)
              }
              color="primary"
            />
          </Box>
        )}
      </Paper>
    </Box>
  )
}

function MetricCard({
  label,
  value,
}: {
  label: string
  value: number
}) {
  return (
    <Paper
      variant="outlined"
      sx={{
        p: 2.5,
      }}
    >
      <Typography
        variant="body2"
        color="text.secondary"
        sx={{
          mb: 1,
        }}
      >
        {label}
      </Typography>

      <Typography
        variant="h4"
      >
        {value}
      </Typography>
    </Paper>
  )
}