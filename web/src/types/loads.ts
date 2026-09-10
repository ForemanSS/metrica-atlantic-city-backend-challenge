export type LoadStatus =
  | 'Pending'
  | 'Processing'
  | 'Loaded'
  | 'Completed'
  | 'Notified'

export type LoadResult =
  | 'Pending'
  | 'Success'
  | 'Partial'
  | 'Rejected'
  | 'Failed'

export interface LoadSummary {
  id: string
  fileName: string
  period: string | null
  status: LoadStatus
  result: LoadResult
  totalRows: number
  validRows: number
  insertedRows: number
  existingRows: number
  invalidRows: number
  userEmail: string
  correlationId: string
  createdAt: string
  completedAt: string | null
  notifiedAt: string | null
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
}

export interface GetLoadsParams {
  page: number
  pageSize: number
}

export interface LoadDetail
  extends LoadSummary {
  userId: string
  errorMessage: string | null
  processingStartedAt: string | null
  loadedAt: string | null
}

export interface LoadHistoryItem {
  id: string
  status: LoadStatus
  result: LoadResult
  message: string | null
  correlationId: string
  occurredAt: string
}

export interface ProcessedDataItem {
  id: string
  sourceRowNumber: number
  period: string
  productCode: string
  productName: string
  description: string
  category: string
  quantity: number
  price: number
  createdAt: string
}

export interface LoadErrorItem {
  id: string
  rowNumber: number | null
  errorCode: string
  field: string | null
  message: string
  rawData: string | null
  createdAt: string
}

export interface UploadLoadResponse {
  loadId: string
  status: 'Pending'
  correlationId: string
}

export interface ApiErrorResponse {
  code?: string
  message?: string
}