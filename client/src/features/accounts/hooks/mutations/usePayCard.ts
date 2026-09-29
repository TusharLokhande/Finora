import { useMutation, useQueryClient } from "@tanstack/react-query";
import { payCard } from "../../api/accounts.api";
import { accountKeys } from "../queries/useAccounts";
import type { PayCardInput } from "../../types/account.types";

export function usePayCard() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ cardAccountId, payload }: { cardAccountId: string; payload: PayCardInput }) =>
      payCard(cardAccountId, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: accountKeys.all });
    },
  });
}
