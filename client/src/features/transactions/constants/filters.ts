export { PERIOD_OPTIONS, resolvePeriodRange, type PeriodKey } from "@/lib/dateRange";
import type { TransactionFilters, TransactionType } from "../types/transaction.types";

export const TRANSACTION_TYPES: TransactionType[] = ["Expense", "Income", "Transfer"];

export const DEFAULT_FILTERS: TransactionFilters = {
  period: "12",
  from: null,
  to: null,
  accountIds: [],
  categoryIds: [],
  type: null,
  minAmount: null,
  maxAmount: null,
  search: "",
};

export const PAGE_SIZE = 50;
