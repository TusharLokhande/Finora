import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { updateAccessSettings } from "../../api/access.api";
import { accessKeys } from "../queries/useMembers";
import type { AccessSettings } from "../../types/access.types";

export function useUpdateAccessSettings() {
  const queryClient = useQueryClient();

  return useMutation<AccessSettings, ApiError, boolean, { previous?: AccessSettings }>({
    mutationFn: updateAccessSettings,
    onMutate: async (signupsOpen) => {
      await queryClient.cancelQueries({ queryKey: accessKeys.settings() });
      const previous = queryClient.getQueryData<AccessSettings>(accessKeys.settings());
      queryClient.setQueryData<AccessSettings>(accessKeys.settings(), { signupsOpen });
      return { previous };
    },
    onError: (error, _v, context) => {
      if (context?.previous) queryClient.setQueryData(accessKeys.settings(), context.previous);
      toast.error(error.message ?? "Couldn't update this setting.");
    },
    onSettled: () => queryClient.invalidateQueries({ queryKey: accessKeys.all }),
  });
}
