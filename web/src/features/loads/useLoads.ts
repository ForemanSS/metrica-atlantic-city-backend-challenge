import {
  keepPreviousData,
  useQuery,
} from '@tanstack/react-query'
import type {
  GetLoadsParams,
  LoadSummary,
  PagedResult,
} from '../../types/loads'
import { getLoads } from './loadsApi'

const notificationGracePeriodMs =
  15 * 1000

export function shouldPollLoad(
  load: LoadSummary,
): boolean {
  if (
    load.status === 'Pending' ||
    load.status === 'Processing' ||
    load.status === 'Loaded'
  ) {
    return true
  }

  if (
    load.status === 'Completed' &&
    load.completedAt
  ) {
    const completedAt =
      new Date(
        load.completedAt,
      ).getTime()

    return (
      Date.now() - completedAt <
      notificationGracePeriodMs
    )
  }

  return false
}

function hasActiveLoads(
  data:
    | PagedResult<LoadSummary>
    | undefined,
): boolean {
  return (
    data?.items.some(
      shouldPollLoad,
    ) ?? false
  )
}

export function useLoads(
  params: GetLoadsParams,
) {
  return useQuery({
    queryKey: [
      'loads',
      params.page,
      params.pageSize,
    ],

    queryFn: () =>
      getLoads(params),

    placeholderData:
      keepPreviousData,

    refetchInterval: (query) => {
      const data =
        query.state.data as
          | PagedResult<LoadSummary>
          | undefined

      return hasActiveLoads(data)
        ? 3000
        : false
    },

    refetchIntervalInBackground: true,
  })
}