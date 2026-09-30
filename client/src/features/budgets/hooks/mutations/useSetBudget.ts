import { useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { transactionKeys } from "@/features/transactions";
import { setBudget } from "../../api/budgets.api";
import { budgetKeys } from "../queries/useBudgets";
import { withBudget } from "../../constants/budgetMath";
import type { BudgetLine, BudgetMonth } from "../../types/budget.types";

interface SetBudgetVariables {
  month: string;
  /** The row being edited; for a sub-category's first budget, a stub built from the category. */
  line: BudgetLine;
  /** Minor units. */
  amount: number;
}

/** Inserts or replaces a line; a new sub-category goes after its parent's last row. */
function upsertLine(lines: BudgetLine[], next: BudgetLine): BudgetLine[] {
  const index = lines.findIndex((l) => l.categoryId === next.categoryId);
  if (index >= 0) return lines.map((l, i) => (i === index ? next : l));

  let after = -1;
  lines.forEach((l, i) => {
    if (l.categoryId === next.parentId || l.parentId === next.parentId) after = i;
  });
  return [...lines.slice(0, after + 1), next, ...lines.slice(after + 1)];
}

/** Optimistic inline budget edit: bar, percent and totals move at once; rolls back with a toast. */
export function useSetBudget() {
  const queryClient = useQueryClient();

  return useMutation<BudgetMonth, ApiError, SetBudgetVariables, { previous?: BudgetMonth; previousHasAny?: boolean }>({
    mutationFn: ({ month, line, amount }) => setBudget({ month, categoryId: line.categoryId, amount }),
    onMutate: async ({ month, line, amount }) => {
      await queryClient.cancelQueries({ queryKey: budgetKeys.all });
      const previous = queryClient.getQueryData<BudgetMonth>(budgetKeys.month(month));
      const previousHasAny = queryClient.getQueryData<boolean>(budgetKeys.hasAny(month));

      queryClient.setQueryData<BudgetMonth>(budgetKeys.month(month), (old) => {
        if (!old) return old;
        const totalBudgeted = old.totalBudgeted + amount - (line.budgeted ?? 0);
        return {
          ...old,
          totalBudgeted,
          remaining: totalBudgeted - old.totalSpent,
          lines: upsertLine(old.lines, withBudget(line, amount)),
        };
      });
      queryClient.setQueryData(budgetKeys.hasAny(month), true);
      return { previous, previousHasAny };
    },
    onError: (error, { month }, context) => {
      queryClient.setQueryData(budgetKeys.month(month), context?.previous);
      queryClient.setQueryData(budgetKeys.hasAny(month), context?.previousHasAny);
      toast.error(error.message ?? "Couldn't save this budget.");
    },
    onSuccess: (saved, { month }) => queryClient.setQueryData(budgetKeys.month(month), saved),
    onSettled: () => {
      // Budgets, the dashboard and reports all key under the transactions prefix.
      queryClient.invalidateQueries({ queryKey: transactionKeys.all });
    },
  });
}
