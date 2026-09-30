import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { transactionKeys } from "@/features/transactions";
import { copyBudgetsFromPrevious } from "../../api/budgets.api";

/** Copies last month's budgets into an empty month, then refetches the page. */
export function useCopyBudgets() {
  const queryClient = useQueryClient();

  return useMutation<number, ApiError, string>({
    mutationFn: copyBudgetsFromPrevious,
    onSuccess: (count) => toast.success(count ? `Copied ${count} budget${count === 1 ? "" : "s"}` : "Nothing to copy"),
    onError: (error) => toast.error(error.message ?? "Couldn't copy budgets."),
    onSettled: () => {
      // Budgets, the dashboard and reports all key under the transactions prefix.
      queryClient.invalidateQueries({ queryKey: transactionKeys.all });
    },
  });
}
