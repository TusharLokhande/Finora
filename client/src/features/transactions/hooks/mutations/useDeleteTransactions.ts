import { toast } from "sonner";
import {
  bulkDeleteTransactions,
  bulkRestoreTransactions,
  deleteTransaction,
  restoreTransaction,
} from "../../api/transactions.api";
import type { Transaction } from "../../types/transaction.types";
import { insertItems, removeItems, useOptimisticTransactionsMutation } from "./useOptimisticTransactionsMutation";

const ids = (rows: Transaction[]) => rows.map((t) => t.id);
const plural = (n: number) => (n === 1 ? "Transaction" : `${n} transactions`);

/** Puts soft-deleted rows back (the Undo on the delete toast). */
export function useRestoreTransactions() {
  return useOptimisticTransactionsMutation({
    mutationFn: (rows: Transaction[]): Promise<unknown> =>
      rows.length === 1 ? restoreTransaction(rows[0].id) : bulkRestoreTransactions(ids(rows)),
    updater: (pages, rows) => insertItems(pages, rows),
    errorMessage: "Couldn't restore.",
  });
}

/** Soft delete (one or many) with an Undo toast. */
export function useDeleteTransactions() {
  const restore = useRestoreTransactions();

  return useOptimisticTransactionsMutation({
    mutationFn: (rows: Transaction[]): Promise<unknown> =>
      rows.length === 1 ? deleteTransaction(rows[0].id) : bulkDeleteTransactions(ids(rows)),
    updater: (pages, rows) => removeItems(pages, new Set(ids(rows))),
    errorMessage: "Couldn't delete.",
    onSuccess: (_data, rows) =>
      toast.success(`${plural(rows.length)} deleted`, {
        action: { label: "Undo", onClick: () => restore.mutate(rows) },
      }),
  });
}
