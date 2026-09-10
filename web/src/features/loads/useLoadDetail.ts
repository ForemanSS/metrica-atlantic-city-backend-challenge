import {
  useQuery,
} from '@tanstack/react-query'

import {
  getLoadData,
  getLoadDetail,
  getLoadErrors,
  getLoadHistory,
} from './loadsApi'

import {
  shouldPollLoad,
} from './useLoads'

export function useLoadDetail(
  loadId: string,
) {
  return useQuery({
    queryKey: [
      'load',
      loadId,
    ],

    queryFn: () =>
      getLoadDetail(loadId),

    refetchInterval: (query) => {
      const load =
        query.state.data

      return load &&
        shouldPollLoad(load)
        ? 3000
        : false
    },

    refetchIntervalInBackground: true,
  })
}

export function useLoadHistory(
  loadId: string,
  shouldPoll = false,
) {
  return useQuery({
    queryKey: [
      'load',
      loadId,
      'history',
    ],

    queryFn: () =>
      getLoadHistory(loadId),

    refetchInterval:
      shouldPoll
        ? 3000
        : false,

    refetchIntervalInBackground: true,
  })
}

export function useLoadData(
  loadId: string,
  page: number,
  pageSize: number,
  shouldPoll = false,
) {
  return useQuery({
    queryKey: [
      'load',
      loadId,
      'data',
      page,
      pageSize,
    ],

    queryFn: () =>
      getLoadData(
        loadId,
        page,
        pageSize,
      ),

    refetchInterval:
      shouldPoll
        ? 3000
        : false,

    refetchIntervalInBackground: true,
  })
}

export function useLoadErrors(
  loadId: string,
  page: number,
  pageSize: number,
  shouldPoll = false,
) {
  return useQuery({
    queryKey: [
      'load',
      loadId,
      'errors',
      page,
      pageSize,
    ],

    queryFn: () =>
      getLoadErrors(
        loadId,
        page,
        pageSize,
      ),

    refetchInterval:
      shouldPoll
        ? 3000
        : false,

    refetchIntervalInBackground: true,
  })
}