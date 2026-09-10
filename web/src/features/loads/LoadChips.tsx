import {
  Chip,
} from '@mui/material'
import type {
  ChipProps,
} from '@mui/material/Chip'
import type {
  LoadResult,
  LoadStatus,
} from '../../types/loads'
import {
  getLoadStatusLabel,
} from './loadLabels'

const statusColors:
Record<LoadStatus, ChipProps['color']> = {
  Pending: 'default',
  Processing: 'info',
  Loaded: 'primary',
  Completed: 'secondary',
  Notified: 'success',
}

const resultLabels:
Record<LoadResult, string> = {
  Pending: 'Pendiente',
  Success: 'Exitoso',
  Partial: 'Parcial',
  Rejected: 'Rechazado',
  Failed: 'Fallido',
}

const resultColors:
Record<LoadResult, ChipProps['color']> = {
  Pending: 'default',
  Success: 'success',
  Partial: 'warning',
  Rejected: 'error',
  Failed: 'error',
}

export function LoadStatusChip({
  status,
}: {
  status: LoadStatus
}) {
  return (
    <Chip
      size="small"
      label={getLoadStatusLabel(status)}
      color={statusColors[status]}
      variant={
        status === 'Pending'
          ? 'outlined'
          : 'filled'
      }
    />
  )
}

export function LoadResultChip({
  result,
}: {
  result: LoadResult
}) {
  return (
    <Chip
      size="small"
      label={resultLabels[result]}
      color={resultColors[result]}
      variant={
        result === 'Pending'
          ? 'outlined'
          : 'filled'
      }
    />
  )
}