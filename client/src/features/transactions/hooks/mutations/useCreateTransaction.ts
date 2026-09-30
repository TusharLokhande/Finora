import { useCallback } from "react";
import { toMinorUnits } from "@/lib/currency";
import { createTransaction } from "../../api/transactions.api";
import { useLookupOptions } from "../queries/useLookupOptions";
import type { CreateTransactionInput, Transaction } from "../../types/transaction.types";
import { TEMP_ID_PREFIX, insertItems, useOptimisticTransactionsMutation } from "./useOptimisticTransactionsMutation";

/** Builds the row a form submission will display, with names from the cached lookups. */
export function useToTransaction() {
  const { accountById, categoryById } = useLookupOptions();

  return useCallback(
    (id: string, input: CreateTransactionInput): Transaction => {
      const isTransfer = input.type === "Transfer";
      const category = !isTransfer && input.categoryId ? categoryById.get(input.categoryId) : undefined;
      return {
        id,
        type: input.type,
        txnDate: input.txnDate,
        amount: toMinorUnits(input.amount),
        accountId: input.accountId,
        toAccountId: isTransfer ? input.toAccountId : null,
        categoryId: isTransfer ? null : input.categoryId,
        description: input.description || null,
        notes: input.notes || null,
        accountName: accountById.get(input.accountId)?.label ?? null,
        toAccountName: isTransfer && input.toAccountId ? (accountById.get(input.toAccountId)?.label ?? null) : null,
        categoryName: category?.name ?? null,
        parentCategoryName: category?.parentName ?? null,
      };
    },
    [accountById, categoryById],
  );
}

export function useCreateTransaction() {
  const toTransaction = useToTransaction();

  return useOptimisticTransactionsMutation({
    mutationFn: createTransaction,
    updater: (pages, input: CreateTransactionInput) =>
      insertItems(pages, [toTransaction(`${TEMP_ID_PREFIX}${crypto.randomUUID()}`, input)]),
    errorMessage: "Couldn't save this transaction.",
  });
}
