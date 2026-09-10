import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Divider,
  Pagination,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
} from '@mui/material'
import ArrowBackIcon
  from '@mui/icons-material/ArrowBack'

import {
  useEffect,
  useRef,
  useState,
} from 'react'
import {
  useQueryClient,
} from '@tanstack/react-query'
import {
  useNavigate,
  useParams,
} from 'react-router-dom'
import {
  LoadResultChip,
  LoadStatusChip,
} from '../features/loads/LoadChips'
import {
  getLoadStatusLabel,
} from '../features/loads/loadLabels'
import {
  useLoadData,
  useLoadDetail,
  useLoadErrors,
  useLoadHistory,
} from '../features/loads/useLoadDetail'

import { shouldPollLoad }
  from '../features/loads/useLoads'

const pageSize = 10

const dateFormatter =
  new Intl.DateTimeFormat(
    'es-PE',
    {
      dateStyle: 'medium',
      timeStyle: 'short',
    },
  )

function formatDate(
  value: string | null,
): string {
  return value
    ? dateFormatter.format(
        new Date(value),
      )
    : '-'
}

export function LoadDetailPage() {
  const { loadId = '' } =
    useParams()

  const navigate =
    useNavigate()

  const queryClient =
    useQueryClient()

  const terminalRefreshRef =
    useRef<string | null>(null)

  const [dataPage, setDataPage] =
    useState(1)

  const [errorsPage, setErrorsPage] =
    useState(1)

  const detail =
    useLoadDetail(loadId)

  const shouldPoll =
    detail.data
        ? shouldPollLoad(
            detail.data,
        )
        : true

  const history =
    useLoadHistory(
        loadId,
        shouldPoll,
    )

  const data =
    useLoadData(
        loadId,
        dataPage,
        pageSize,
        shouldPoll,
    )

    const errors =
    useLoadErrors(
        loadId,
        errorsPage,
        pageSize,
        shouldPoll,
    )

    const detailStatus =
    detail.data?.status

    const detailResult =
    detail.data?.result

    const detailCompletedAt =
    detail.data?.completedAt

    const detailNotifiedAt =
    detail.data?.notifiedAt

    useEffect(() => {
    if (
        !detailStatus ||
        !detailResult
    ) {
        return
    }

    const isTerminal =
        detailStatus === 'Notified' ||
        detailResult === 'Rejected' ||
        detailResult === 'Failed'

    if (!isTerminal) {
        return
    }

    const terminalKey =
        [
        loadId,
        detailStatus,
        detailResult,
        detailCompletedAt ?? '',
        detailNotifiedAt ?? '',
        ].join(':')

    if (
        terminalRefreshRef.current ===
        terminalKey
    ) {
        return
    }

    terminalRefreshRef.current =
        terminalKey

    void Promise.all([
        queryClient.refetchQueries({
        queryKey: [
            'load',
            loadId,
            'history',
        ],
        type: 'active',
        }),

        queryClient.refetchQueries({
        queryKey: [
            'load',
            loadId,
            'data',
        ],
        type: 'active',
        }),

        queryClient.refetchQueries({
        queryKey: [
            'load',
            loadId,
            'errors',
        ],
        type: 'active',
        }),
    ])
    }, [
    detailStatus,
    detailResult,
    detailCompletedAt,
    detailNotifiedAt,
    loadId,
    queryClient,
    ])

    if (detail.isLoading) {
        return (
        <Box
            sx={{
            minHeight: 400,
            display: 'grid',
            placeItems: 'center',
            }}
        >
            <CircularProgress />
        </Box>
        )
    }

  if (
    detail.isError ||
    !detail.data
  ) {
    return (
      <Alert severity="error">
        No fue posible consultar
        la carga solicitada.
      </Alert>
    )
  }

  const load =
    detail.data

  return (
    <Box>
      <Button
        startIcon={
          <ArrowBackIcon />
        }
        onClick={() =>
          navigate('/')
        }
        sx={{
          mb: 2,
        }}
      >
        Volver al historial
      </Button>

      <Box
        sx={{
          display: 'flex',
          justifyContent:
            'space-between',
          alignItems: {
            xs: 'flex-start',
            md: 'center',
          },
          flexDirection: {
            xs: 'column',
            md: 'row',
          },
          gap: 2,
          mb: 3,
        }}
      >
        <Box>
          <Typography
            variant="h4"
            sx={{
              mb: 0.5,
            }}
          >
            {load.fileName}
          </Typography>

          <Typography
            color="text.secondary"
          >
            {load.id}
          </Typography>
        </Box>

        <Box
          sx={{
            display: 'flex',
            gap: 1,
          }}
        >
          <LoadStatusChip
            status={load.status}
          />

          <LoadResultChip
            result={load.result}
          />
        </Box>
      </Box>

      {load.errorMessage && (
        <Alert
          severity="error"
          sx={{
            mb: 3,
          }}
        >
          {load.errorMessage}
        </Alert>
      )}

      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            sm:
              'repeat(2, minmax(0, 1fr))',
            lg:
              'repeat(5, minmax(0, 1fr))',
          },
          gap: 2,
          mb: 3,
        }}
      >
        <Metric
          label="Total"
          value={load.totalRows}
        />

        <Metric
          label="Válidos"
          value={load.validRows}
        />

        <Metric
          label="Insertados"
          value={load.insertedRows}
        />

        <Metric
          label="Existentes"
          value={load.existingRows}
        />

        <Metric
          label="Inválidos"
          value={load.invalidRows}
        />
      </Box>

      <Box
        sx={{
          display: 'grid',
          gridTemplateColumns: {
            xs: '1fr',
            lg: '360px 1fr',
          },
          gap: 3,
          mb: 3,
        }}
      >
        <Paper
          variant="outlined"
          sx={{
            p: 3,
          }}
        >
          <Typography
            variant="h6"
            sx={{
              mb: 2,
              fontWeight: 700,
            }}
          >
            Información
          </Typography>

          <InfoRow
            label="Período"
            value={
              load.period ?? '-'
            }
          />

          <InfoRow
            label="Usuario"
            value={load.userEmail}
          />

          <InfoRow
            label="Correlation ID"
            value={
              load.correlationId
            }
          />

          <InfoRow
            label="Registrado"
            value={formatDate(
              load.createdAt,
            )}
          />

          <InfoRow
            label="Finalizado"
            value={formatDate(
              load.completedAt,
            )}
          />

          <InfoRow
            label="Notificado"
            value={formatDate(
              load.notifiedAt,
            )}
          />
        </Paper>

        <Paper
          variant="outlined"
          sx={{
            p: 3,
          }}
        >
          <Typography
            variant="h6"
            sx={{
              mb: 2,
              fontWeight: 700,
            }}
          >
            Trazabilidad
          </Typography>

          {history.isLoading && (
            <CircularProgress
              size={24}
            />
          )}

          {history.data?.map(
            (item, index) => (
              <Box
                key={item.id}
                sx={{
                  display: 'grid',
                  gridTemplateColumns:
                    '28px 1fr',
                  gap: 1.5,
                }}
              >
                <Box
                  sx={{
                    display: 'flex',
                    flexDirection:
                      'column',
                    alignItems:
                      'center',
                  }}
                >
                  <Box
                    sx={{
                        width: 21,
                        height: 21,
                        borderRadius: '50%',
                        display: 'grid',
                        placeItems: 'center',
                        bgcolor: 'success.main',
                        color: 'white',
                        fontSize: 13,
                        fontWeight: 800,
                        flexShrink: 0,
                    }}
                    >
                    ✓
                    </Box>

                  {index <
                    history.data.length -
                      1 && (
                    <Box
                      sx={{
                        width: '1px',
                        flex: 1,
                        minHeight: 35,
                        bgcolor:
                          'divider',
                      }}
                    />
                  )}
                </Box>

                <Box
                  sx={{
                    pb: 2.5,
                  }}
                >
                  <Box
                    sx={{
                      display: 'flex',
                      gap: 1,
                      alignItems:
                        'center',
                      flexWrap: 'wrap',
                    }}
                  >
                    <Typography
                      sx={{
                        fontWeight: 700,
                      }}
                    >
                      {getLoadStatusLabel(item.status)}
                    </Typography>

                    <LoadResultChip
                      result={
                        item.result
                      }
                    />
                  </Box>

                  {item.message && (
                    <Typography
                      variant="body2"
                      color=
                        "text.secondary"
                      sx={{
                        mt: 0.5,
                      }}
                    >
                      {item.message}
                    </Typography>
                  )}

                  <Typography
                    variant="caption"
                    color=
                      "text.secondary"
                  >
                    {formatDate(
                      item.occurredAt,
                    )}
                  </Typography>
                </Box>
              </Box>
            ),
          )}
        </Paper>
      </Box>

      <Paper
        variant="outlined"
        sx={{
          mb: 3,
          overflow: 'hidden',
        }}
      >
        <SectionHeader
          title="Datos procesados"
          subtitle={`${data.data?.totalItems ?? 0} registros`}
        />

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>
                  Fila
                </TableCell>
                <TableCell>
                  Código
                </TableCell>
                <TableCell>
                  Producto
                </TableCell>
                <TableCell>
                  Categoría
                </TableCell>
                <TableCell
                  align="right"
                >
                  Cantidad
                </TableCell>
                <TableCell
                  align="right"
                >
                  Precio
                </TableCell>
              </TableRow>
            </TableHead>

            <TableBody>
              {data.data?.items.map(
                (item) => (
                  <TableRow
                    key={item.id}
                  >
                    <TableCell>
                      {
                        item.sourceRowNumber
                      }
                    </TableCell>

                    <TableCell>
                      {item.productCode}
                    </TableCell>

                    <TableCell>
                      {item.productName}
                    </TableCell>

                    <TableCell>
                      {item.category}
                    </TableCell>

                    <TableCell
                      align="right"
                    >
                      {item.quantity}
                    </TableCell>

                    <TableCell
                      align="right"
                    >
                      S/{' '}
                      {item.price.toFixed(
                        2,
                      )}
                    </TableCell>
                  </TableRow>
                ),
              )}

              {data.data?.items.length ===
                0 && (
                <EmptyRow
                  colSpan={6}
                  text=
                    "No existen datos procesados."
                />
              )}
            </TableBody>
          </Table>
        </TableContainer>

        {(data.data?.totalPages ??
          0) > 1 && (
          <PaginationFooter
            page={dataPage}
            pages={
              data.data
                ?.totalPages ?? 1
            }
            onChange={
              setDataPage
            }
          />
        )}
      </Paper>

      <Paper
        variant="outlined"
        sx={{
          overflow: 'hidden',
        }}
      >
        <SectionHeader
          title="Errores y observaciones"
          subtitle={`${errors.data?.totalItems ?? 0} registros`}
        />

        <TableContainer>
          <Table size="small">
            <TableHead>
              <TableRow>
                <TableCell>
                  Fila
                </TableCell>
                <TableCell>
                  Código
                </TableCell>
                <TableCell>
                  Campo
                </TableCell>
                <TableCell>
                  Mensaje
                </TableCell>
              </TableRow>
            </TableHead>

            <TableBody>
              {errors.data?.items.map(
                (item) => (
                  <TableRow
                    key={item.id}
                  >
                    <TableCell>
                      {item.rowNumber ??
                        '-'}
                    </TableCell>

                    <TableCell>
                      {item.errorCode}
                    </TableCell>

                    <TableCell>
                      {item.field ??
                        '-'}
                    </TableCell>

                    <TableCell>
                      {item.message}
                    </TableCell>
                  </TableRow>
                ),
              )}

              {errors.data?.items
                .length === 0 && (
                <EmptyRow
                  colSpan={4}
                  text=
                    "La carga no presenta errores."
                />
              )}
            </TableBody>
          </Table>
        </TableContainer>

        {(errors.data
          ?.totalPages ?? 0) >
          1 && (
          <PaginationFooter
            page={errorsPage}
            pages={
              errors.data
                ?.totalPages ?? 1
            }
            onChange={
              setErrorsPage
            }
          />
        )}
      </Paper>
    </Box>
  )
}

function Metric({
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
      >
        {label}
      </Typography>

      <Typography
        variant="h4"
        sx={{
          mt: 0.5,
        }}
      >
        {value}
      </Typography>
    </Paper>
  )
}

function InfoRow({
  label,
  value,
}: {
  label: string
  value: string
}) {
  return (
    <>
      <Box
        sx={{
          py: 1.25,
        }}
      >
        <Typography
          variant="caption"
          color="text.secondary"
        >
          {label}
        </Typography>

        <Typography
          variant="body2"
          sx={{
            mt: 0.25,
            wordBreak:
              'break-word',
          }}
        >
          {value}
        </Typography>
      </Box>

      <Divider />
    </>
  )
}

function SectionHeader({
  title,
  subtitle,
}: {
  title: string
  subtitle: string
}) {
  return (
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
        {title}
      </Typography>

      <Typography
        variant="body2"
        color="text.secondary"
      >
        {subtitle}
      </Typography>
    </Box>
  )
}

function EmptyRow({
  colSpan,
  text,
}: {
  colSpan: number
  text: string
}) {
  return (
    <TableRow>
      <TableCell
        colSpan={colSpan}
        align="center"
        sx={{
          py: 4,
        }}
      >
        <Typography
          color="text.secondary"
        >
          {text}
        </Typography>
      </TableCell>
    </TableRow>
  )
}

function PaginationFooter({
  page,
  pages,
  onChange,
}: {
  page: number
  pages: number
  onChange: (
    page: number,
  ) => void
}) {
  return (
    <Box
      sx={{
        display: 'flex',
        justifyContent:
          'flex-end',
        px: 3,
        py: 2,
        borderTop:
          '1px solid',
        borderColor: 'divider',
      }}
    >
      <Pagination
        page={page}
        count={pages}
        onChange={(
          _,
          nextPage,
        ) =>
          onChange(nextPage)
        }
      />
    </Box>
  )
}