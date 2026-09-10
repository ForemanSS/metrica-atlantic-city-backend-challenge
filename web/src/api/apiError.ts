import axios from 'axios'
import type {
  ApiErrorResponse,
} from '../types/loads'

export function getApiErrorMessage(
  error: unknown,
  fallback:
    string = 'Ocurrió un error inesperado.',
): string {
  if (
    axios.isAxiosError<ApiErrorResponse>(
      error,
    )
  ) {
    return (
      error.response?.data?.message ??
      fallback
    )
  }

  return fallback
}