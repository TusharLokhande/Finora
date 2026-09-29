import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { accountKeys } from "../queries/useAccounts";
import type { Account } from "../../types/account.types";

interface OptimisticAccountsMutationOptions<TVariables> {
  mutationFn: (variables: TVariables) => Promise<Account | Account[]>;
  updater: (accounts: Account[], variables: TVariables) => Account[];
  errorMessage: string;
}

/** Shared optimistic-update + rollback + toast plumbing for the accounts list query. */
export function useOptimisticAccountsMutation<TVariables>({
  mutationFn,
  updater,
  errorMessage,
}: OptimisticAccountsMutationOptions<TVariables>) {
  const queryClient = useQueryClient();

  return useMutation<Account | Account[], ApiError, TVariables, { previous: Account[] | undefined }>({
    mutationFn,
    onMutate: async (variables: TVariables) => {
      await queryClient.cancelQueries({ queryKey: accountKeys.list() });
      const previous = queryClient.getQueryData<Account[]>(accountKeys.list());
      queryClient.setQueryData<Account[]>(accountKeys.list(), (old) => updater(old ?? [], variables));
      return { previous };
    },
    onError: (error, _variables, context) => {
      if (context?.previous) {
        queryClient.setQueryData(accountKeys.list(), context.previous);
      }
      toast.error(error.message ?? errorMessage);
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: accountKeys.all });
    },
  });
}
