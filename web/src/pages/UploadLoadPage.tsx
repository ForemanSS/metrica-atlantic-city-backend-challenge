import {
  useRef,
  useState,
  type ChangeEvent,
  type DragEvent,
} from 'react'
import {
  Alert,
  Box,
  Button,
  LinearProgress,
  Paper,
  Typography,
} from '@mui/material'
import {
  useNavigate,
} from 'react-router-dom'
import { getApiErrorMessage }
  from '../api/apiError'
import { useUploadLoad }
  from '../features/loads/useUploadLoad'

const maxFileSizeBytes =
  20 * 1024 * 1024

const excelMimeType =
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'

function formatFileSize(
  bytes: number,
): string {
  if (bytes < 1024 * 1024) {
    return `${(
      bytes / 1024
    ).toFixed(1)} KB`
  }

  return `${(
    bytes /
    (1024 * 1024)
  ).toFixed(2)} MB`
}

function validateFile(
  file: File,
): string | null {
  if (
    !file.name
      .toLowerCase()
      .endsWith('.xlsx')
  ) {
    return 'Sólo se permiten archivos con extensión .xlsx.'
  }

  if (file.size <= 0) {
    return 'El archivo seleccionado está vacío.'
  }

  if (
    file.size >
    maxFileSizeBytes
  ) {
    return 'El archivo supera el tamaño máximo permitido de 20 MB.'
  }

  return null
}

export function UploadLoadPage() {
  const navigate =
    useNavigate()

  const inputRef =
    useRef<HTMLInputElement>(
      null,
    )

  const uploadMutation =
    useUploadLoad()

  const [file, setFile] =
    useState<File | null>(null)

  const [error, setError] =
    useState<string | null>(null)

  const [progress, setProgress] =
    useState(0)

  const [isDragging, setIsDragging] =
    useState(false)

  function selectFile(
    selectedFile: File,
  ) {
    const validationError =
      validateFile(
        selectedFile,
      )

    if (validationError) {
      setFile(null)
      setError(
        validationError,
      )
      setProgress(0)

      return
    }

    uploadMutation.reset()

    setFile(
      selectedFile,
    )

    setError(null)
    setProgress(0)
  }

  function handleInputChange(
    event:
      ChangeEvent<HTMLInputElement>,
  ) {
    const selectedFile =
      event.target.files?.[0]

    if (selectedFile) {
      selectFile(
        selectedFile,
      )
    }

    event.target.value = ''
  }

  function handleDrop(
    event:
      DragEvent<HTMLDivElement>,
  ) {
    event.preventDefault()

    setIsDragging(false)

    const selectedFile =
      event.dataTransfer.files?.[0]

    if (selectedFile) {
      selectFile(
        selectedFile,
      )
    }
  }

  async function handleUpload() {
    if (!file) {
      setError(
        'Debe seleccionar un archivo Excel.',
      )

      return
    }

    setError(null)
    setProgress(0)

    try {
      const response =
        await uploadMutation.mutateAsync({
          file,

          onProgress:
            setProgress,
        })

      navigate(
        `/loads/${response.loadId}`,
        {
          replace: true,
        },
      )
    } catch (uploadError) {
      setError(
        getApiErrorMessage(
          uploadError,
          'No fue posible enviar el archivo.',
        ),
      )
    }
  }

  return (
    <Box
      sx={{
        maxWidth: 900,
        mx: 'auto',
      }}
    >
      <Button
        onClick={() =>
          navigate('/')
        }
        sx={{
          mb: 2,
        }}
      >
        ← Volver al historial
      </Button>

      <Typography
        variant="h4"
        sx={{
          mb: 0.5,
        }}
      >
        Nueva carga
      </Typography>

      <Typography
        color="text.secondary"
        sx={{
          mb: 3,
        }}
      >
        Selecciona el archivo Excel que
        será procesado de manera
        asíncrona.
      </Typography>

      {error && (
        <Alert
          severity="error"
          sx={{
            mb: 3,
          }}
        >
          {error}
        </Alert>
      )}

      <Paper
        variant="outlined"
        sx={{
          p: {
            xs: 2,
            sm: 4,
          },
        }}
      >
        <Box
          onDragEnter={(
            event,
          ) => {
            event.preventDefault()
            setIsDragging(true)
          }}
          onDragOver={(
            event,
          ) => {
            event.preventDefault()
            setIsDragging(true)
          }}
          onDragLeave={() =>
            setIsDragging(false)
          }
          onDrop={
            handleDrop
          }
          onClick={() =>
            inputRef.current?.click()
          }
          sx={{
            minHeight: 260,
            border: '2px dashed',
            borderColor:
              isDragging
                ? 'primary.main'
                : 'divider',

            bgcolor:
              isDragging
                ? 'rgba(19,35,63,0.05)'
                : 'background.default',

            borderRadius: 2,
            display: 'grid',
            placeItems: 'center',
            cursor: 'pointer',
            transition:
              'all 150ms ease',

            '&:hover': {
              borderColor:
                'primary.main',

              bgcolor:
                'rgba(19,35,63,0.03)',
            },
          }}
        >
          <Box
            sx={{
              textAlign: 'center',
              px: 2,
            }}
          >
            <Box
              sx={{
                width: 64,
                height: 64,
                mx: 'auto',
                mb: 2,
                borderRadius: 2,
                display: 'grid',
                placeItems: 'center',
                bgcolor:
                  'primary.main',
                color: 'white',
                fontWeight: 800,
                fontSize: 16,
              }}
            >
              XLSX
            </Box>

            <Typography
              variant="h6"
              sx={{
                mb: 0.5,
                fontWeight: 700,
              }}
            >
              Arrastra tu archivo aquí
            </Typography>

            <Typography
              color="text.secondary"
            >
              o haz clic para
              seleccionarlo
            </Typography>

            <Typography
              variant="caption"
              color="text.secondary"
              sx={{
                display: 'block',
                mt: 1,
              }}
            >
              Formato permitido: .xlsx
              · Máximo 20 MB
            </Typography>
          </Box>
        </Box>

        <input
          ref={inputRef}
          hidden
          type="file"
          accept={
            `.xlsx,${excelMimeType}`
          }
          onChange={
            handleInputChange
          }
        />

        {file && (
          <Paper
            variant="outlined"
            sx={{
              mt: 3,
              p: 2.5,
              bgcolor:
                '#f8fafc',
            }}
          >
            <Box
              sx={{
                display: 'flex',
                justifyContent:
                  'space-between',
                alignItems:
                  'center',
                gap: 2,
                flexWrap:
                  'wrap',
              }}
            >
              <Box>
                <Typography
                  sx={{
                    fontWeight: 700,
                  }}
                >
                  {file.name}
                </Typography>

                <Typography
                  variant="body2"
                  color="text.secondary"
                >
                  {formatFileSize(
                    file.size,
                  )}
                </Typography>
              </Box>

              <Button
                disabled={
                  uploadMutation.isPending
                }
                onClick={(
                  event,
                ) => {
                  event.stopPropagation()

                  setFile(null)
                  setProgress(0)
                  setError(null)
                }}
              >
                Quitar
              </Button>
            </Box>
          </Paper>
        )}

        {uploadMutation.isPending && (
          <Box
            sx={{
              mt: 3,
            }}
          >
            <Box
              sx={{
                display: 'flex',
                justifyContent:
                  'space-between',
                mb: 1,
              }}
            >
              <Typography
                variant="body2"
              >
                Enviando archivo...
              </Typography>

              <Typography
                variant="body2"
                sx={{
                  fontWeight: 700,
                }}
              >
                {progress}%
              </Typography>
            </Box>

            <LinearProgress
              variant="determinate"
              value={progress}
            />
          </Box>
        )}

        <Box
          sx={{
            mt: 4,
            display: 'flex',
            justifyContent:
              'flex-end',
            gap: 1.5,
          }}
        >
          <Button
            variant="outlined"
            disabled={
              uploadMutation.isPending
            }
            onClick={() =>
              navigate('/')
            }
          >
            Cancelar
          </Button>

          <Button
            variant="contained"
            disabled={
              !file ||
              uploadMutation.isPending
            }
            onClick={() => {
              void handleUpload()
            }}
          >
            {uploadMutation.isPending
              ? 'Enviando...'
              : 'Procesar archivo'}
          </Button>
        </Box>
      </Paper>
    </Box>
  )
}