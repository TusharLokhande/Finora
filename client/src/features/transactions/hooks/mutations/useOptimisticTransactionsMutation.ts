import { useMutation, useQueryClient, type QueryKey } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { accountKeys } from "@/features/accounts";
import { transactionKeys, type TransactionPages } from "../queries/useTransactions";
import type { Transaction } from "../../types/transaction.types";

/** Rows created optimistically carry this id prefix until the refetch swaps in the real row. */
export const TEMP_ID_PREFIX = "temp-";
export const isTempId = (id: string) => id.startsWith(TEMP_ID_PREFIX);

interface OptimisticTransactionsMutationOptions<TVariables, TData> {
  mutationFn: (variables: TVariables) => Promise<TData>;
  /** Patches each cached transaction list. */
  updater: (pages: TransactionPages, variables: TVariables) => TransactionPages;
  errorMessage: string;
  onSuccess?: (data: TData, variables: TVariables) => void;
}

/**
 * Optimistic update + rollback + toast for every cached transaction list, then a refetch of
 * everything derived from transactions (lists, recent, dashboard) and account balances.
 */
export function useOptimisticTransactionsMutation<TVariables, TData = unknown>({
  mutationFn,
  updater,
  errorMessage,
  onSuccess,
}: OptimisticTransactionsMutationOptions<TVariables, TData>) {
  const queryClient = useQueryClient();

  return useMutation<TData, ApiError, TVariables, { snapshots: [QueryKey, TransactionPages | undefined][] }>({
    mutationFn,
    onMutate: async (variables) => {
      await queryClient.cancelQueries({ queryKey: transactionKeys.lists() });
      const snapshots = queryClient.getQueriesData<TransactionPages>({ queryKey: transactionKeys.lists() });
      queryClient.setQueriesData<TransactionPages>({ queryKey: transactionKeys.lists() }, (old) =>
        old ? updater(old, variables) : old,
      );
      return { snapshots };
    },
    onError: (error, _variables, context) => {
      context?.snapshots.forEach(([key, data]) => queryClient.setQueryData(key, data));
      toast.error(error.message ?? errorMessage);
    },
    onSuccess,
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: transactionKeys.all });
      queryClient.invalidateQueries({ queryKey: accountKeys.all });
    },
  });
}

// ---- cache patch helpers ----------------------------------------------------------------

export function mapItems(data: TransactionPages, fn: (t: Transaction) => Transaction): TransactionPages {
  return { ...data, pages: data.pages.map((p) => ({ ...p, items: p.items.map(fn) })) };
}

export function removeItems(data: TransactionPages, ids: Set<string>): TransactionPages {
  let removed = 0;
  const pages = data.pages.map((p) => {
    const items = p.items.filter((t) => !ids.has(t.id));
    removed += p.items.length - items.length;
    return { ...p, items };
  });
  return { ...data, pages: pages.map((p) => ({ ...p, totalCount: p.totalCount - removed })) };
}

/**
 * Inserts rows where the server's date-desc order would put them: before the first loaded row
 * on the same or an earlier date. Rows that belong to not-yet-loaded pages are left for the refetch.
 */
export function insertItems(data: TransactionPages, rows: Transaction[]): TransactionPages {
  const pages = data.pages.map((p) => ({ ...p, items: [...p.items] }));
  let inserted = 0;

  for (const row of rows) {
    const page = pages.find((p) => p.items.some((t) => t.txnDate <= row.txnDate));
    if (page) {
      page.items.splice(page.items.findIndex((t) => t.txnDate <= row.txnDate), 0, row);
      inserted++;
      continue;
    }
    const last = pages.at(-1);
    const allLoaded = last && last.page * last.pageSize >= last.totalCount;
    if (last && allLoaded) {
      last.items.push(row);
      inserted++;
    }
  }

  return { ...data, pages: pages.map((p) => ({ ...p, totalCount: p.totalCount + inserted })) };
}
