import { keepPreviousData, useQuery } from "@tanstack/react-query";
import { transactionKeys } from "@/features/transactions";
import { getBudgetMonth, hasAnyBudgets } from "../../api/budgets.api";

/** Spend comes from transactions, so budgets key under that prefix and refresh after any transaction write. */
export const budgetKeys = {
  all: [...transactionKeys.all, "budgets"] as const,
  month: (month: string) => [...budgetKeys.all, "month", month] as const,
  hasAny: (month: string) => [...budgetKeys.all, "has-any", month] as const,
};

export function useBudgetMonth(month: string) {
  return useQuery({
    queryKey: budgetKeys.month(month),
    queryFn: () => getBudgetMonth(month),
    placeholderData: keepPreviousData,
  });
}

export function useHasBudgets(month: string) {
  return useQuery({ queryKey: budgetKeys.hasAny(month), queryFn: () => hasAnyBudgets(month) });
}
