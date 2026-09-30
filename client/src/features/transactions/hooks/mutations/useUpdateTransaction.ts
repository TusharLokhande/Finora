import { updateTransaction } from "../../api/transactions.api";
import type { CreateTransactionInput } from "../../types/transaction.types";
import { mapItems, useOptimisticTransactionsMutation } from "./useOptimisticTransactionsMutation";
import { useToTransaction } from "./useCreateTransaction";

export function useUpdateTransaction() {
  const toTransaction = useToTransaction();

  return useOptimisticTransactionsMutation({
    mutationFn: ({ id, payload }: { id: string; payload: CreateTransactionInput }) => updateTransaction(id, payload),
    updater: (pages, { id, payload }) => mapItems(pages, (t) => (t.id === id ? toTransaction(id, payload) : t)),
    errorMessage: "Couldn't update this transaction.",
  });
}
