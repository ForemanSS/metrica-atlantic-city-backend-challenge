import {
  useMutation,
  useQueryClient,
} from '@tanstack/react-query'
import { uploadLoad }
  from './loadsApi'

interface UploadLoadVariables {
  file: File
  onProgress?: (
    percentage: number,
  ) => void
}

export function useUploadLoad() {
  const queryClient =
    useQueryClient()

  return useMutation({
    mutationFn: ({
      file,
      onProgress,
    }: UploadLoadVariables) =>
      uploadLoad(
        file,
        onProgress,
      ),

    onSuccess: async () => {
      await queryClient.invalidateQueries({
        queryKey: ['loads'],
      })
    },
  })
}