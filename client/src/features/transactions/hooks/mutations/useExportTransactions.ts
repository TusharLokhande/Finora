import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { exportTransactions } from "../../api/transactions.api";
import type { TransactionFilters } from "../../types/transaction.types";

/** Downloads an .xlsx. Pass the current filters, or null for everything. */
export function useExportTransactions() {
  return useMutation<void, ApiError, TransactionFilters | null>({
    mutationFn: exportTransactions,
    onError: (error) => toast.error(error.message ?? "Couldn't export transactions."),
  });
}
