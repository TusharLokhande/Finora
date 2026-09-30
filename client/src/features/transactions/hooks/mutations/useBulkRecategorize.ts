import { toast } from "sonner";
import { bulkRecategorizeTransactions } from "../../api/transactions.api";
import { useLookupOptions } from "../queries/useLookupOptions";
import { mapItems, useOptimisticTransactionsMutation } from "./useOptimisticTransactionsMutation";

export function useBulkRecategorize() {
  const { categoryById } = useLookupOptions();

  return useOptimisticTransactionsMutation({
    mutationFn: ({ ids, categoryId }: { ids: string[]; categoryId: string }) =>
      bulkRecategorizeTransactions(ids, categoryId),
    updater: (pages, { ids, categoryId }) => {
      const selected = new Set(ids);
      const category = categoryById.get(categoryId);
      return mapItems(pages, (t) =>
        selected.has(t.id)
          ? { ...t, categoryId, categoryName: category?.name ?? null, parentCategoryName: category?.parentName ?? null }
          : t,
      );
    },
    errorMessage: "Couldn't re-categorize.",
    onSuccess: (count) => toast.success(`${count} transaction${count === 1 ? "" : "s"} re-categorized`),
  });
}
