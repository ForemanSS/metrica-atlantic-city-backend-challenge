import { httpClient } from '../../api/httpClient'
import type {
  GetLoadsParams,
  LoadSummary,
  PagedResult,
  LoadDetail,
  LoadErrorItem,
  LoadHistoryItem,
  ProcessedDataItem,
  UploadLoadResponse,
} from '../../types/loads'

export async function getLoads(
  params: GetLoadsParams,
): Promise<PagedResult<LoadSummary>> {
  const response =
    await httpClient.get<PagedResult<LoadSummary>>(
      '/api/loads',
      {
        params,
      },
    )

  return response.data
}

export async function getLoadDetail(
  loadId: string,
): Promise<LoadDetail> {
  const response =
    await httpClient.get<LoadDetail>(
      `/api/loads/${loadId}`,
    )

  return response.data
}

export async function getLoadHistory(
  loadId: string,
): Promise<LoadHistoryItem[]> {
  const response =
    await httpClient.get<
      LoadHistoryItem[]
    >(
      `/api/loads/${loadId}/history`,
    )

  return response.data
}

export async function getLoadData(
  loadId: string,
  page: number,
  pageSize: number,
): Promise<
  PagedResult<ProcessedDataItem>
> {
  const response =
    await httpClient.get<
      PagedResult<ProcessedDataItem>
    >(
      `/api/loads/${loadId}/data`,
      {
        params: {
          page,
          pageSize,
        },
      },
    )

  return response.data
}

export async function getLoadErrors(
  loadId: string,
  page: number,
  pageSize: number,
): Promise<
  PagedResult<LoadErrorItem>
> {
  const response =
    await httpClient.get<
      PagedResult<LoadErrorItem>
    >(
      `/api/loads/${loadId}/errors`,
      {
        params: {
          page,
          pageSize,
        },
      },
    )

  return response.data
}

export async function uploadLoad(
  file: File,
  onProgress?: (
    percentage: number,
  ) => void,
): Promise<UploadLoadResponse> {
  const formData =
    new FormData()

  formData.append(
    'File',
    file,
  )

  const response =
    await httpClient.post<UploadLoadResponse>(
      '/api/loads',
      formData,
      {
        onUploadProgress: (
          progressEvent,
        ) => {
          if (
            !progressEvent.total ||
            !onProgress
          ) {
            return
          }

          const percentage =
            Math.round(
              (
                progressEvent.loaded /
                progressEvent.total
              ) * 100,
            )

          onProgress(
            percentage,
          )
        },
      },
    )

  return response.data
}