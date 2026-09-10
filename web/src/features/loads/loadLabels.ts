import type {
  LoadStatus,
} from '../../types/loads'

const statusLabels:
Record<LoadStatus, string> = {
  Pending: 'Pendiente',
  Processing: 'En proceso',
  Loaded: 'Cargado',
  Completed: 'Finalizado',
  Notified: 'Notificado',
}

export function getLoadStatusLabel(
  status: LoadStatus,
): string {
  return statusLabels[status]
}