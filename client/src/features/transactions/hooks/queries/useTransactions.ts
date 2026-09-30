import { keepPreviousData, useInfiniteQuery, useQuery, type InfiniteData } from "@tanstack/react-query";
import type { PagedResult } from "@/types/pagination.types";
import { getDescriptionSuggestions, getRecentTransactions, searchTransactions } from "../../api/transactions.api";
import type { Transaction, TransactionFilters } from "../../types/transaction.types";

/**
 * Everything derived from transactions (dashboard widgets included) keys under `all`,
 * so one invalidation after a write refreshes every screen.
 */
export const transactionKeys = {
  all: ["transactions"] as const,
  lists: () => [...transactionKeys.all, "list"] as const,
  list: (filters: TransactionFilters) => [...transactionKeys.lists(), filters] as const,
  recent: (limit: number) => [...transactionKeys.all, "recent", limit] as const,
  suggestions: (q: string) => [...transactionKeys.all, "suggestions", q] as const,
};

export type TransactionPages = InfiniteData<PagedResult<Transaction>, number>;

export function useTransactions(filters: TransactionFilters) {
  return useInfiniteQuery({
    queryKey: transactionKeys.list(filters),
    queryFn: ({ pageParam }) => searchTransactions(filters, pageParam),
    initialPageParam: 1,
    getNextPageParam: (last) => (last.page * last.pageSize < last.totalCount ? last.page + 1 : undefined),
    placeholderData: keepPreviousData,
  });
}

export function useRecentTransactions(limit = 8) {
  return useQuery({
    queryKey: transactionKeys.recent(limit),
    queryFn: () => getRecentTransactions(limit),
  });
}

export function useDescriptionSuggestions(q: string) {
  const trimmed = q.trim();
  return useQuery({
    queryKey: transactionKeys.suggestions(trimmed.toLowerCase()),
    queryFn: () => getDescriptionSuggestions(trimmed),
    enabled: trimmed.length >= 2,
    staleTime: 5 * 60 * 1000,
    placeholderData: keepPreviousData,
  });
}
