import { reorderAccounts } from "../../api/accounts.api";
import { useOptimisticAccountsMutation } from "./useOptimisticAccountsMutation";

export function useReorderAccounts() {
  return useOptimisticAccountsMutation<string[]>({
    mutationFn: reorderAccounts,
    updater: (accounts, orderedIds) => {
      const order = new Map(orderedIds.map((id, index) => [id, index]));
      return [...accounts]
        .sort((a, b) => (order.get(a.id) ?? 0) - (order.get(b.id) ?? 0))
        .map((a) => ({ ...a, sortOrder: order.get(a.id) ?? a.sortOrder }));
    },
    errorMessage: "Couldn't reorder accounts.",
  });
}
