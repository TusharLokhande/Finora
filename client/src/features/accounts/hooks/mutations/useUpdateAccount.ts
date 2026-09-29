import { toMinorUnits } from "@/lib/currency";
import { updateAccount } from "../../api/accounts.api";
import { useOptimisticAccountsMutation } from "./useOptimisticAccountsMutation";
import type { UpdateAccountInput } from "../../types/account.types";

export function useUpdateAccount() {
  return useOptimisticAccountsMutation<{ id: string; payload: UpdateAccountInput }>({
    mutationFn: ({ id, payload }) => updateAccount(id, payload),
    updater: (accounts, { id, payload }) =>
      accounts.map((account) => {
        if (account.id !== id) return account;

        const isCard = account.type === "CreditCard";
        const openingMinor = toMinorUnits(payload.openingBalance);
        const creditLimitMinor = isCard && payload.creditLimit != null ? toMinorUnits(payload.creditLimit) : null;

        return {
          ...account,
          name: payload.name,
          balance: isCard ? null : openingMinor,
          outstanding: isCard ? openingMinor : null,
          creditLimit: creditLimitMinor,
          availableCredit: creditLimitMinor != null ? creditLimitMinor - openingMinor : null,
          statementDay: isCard ? payload.statementDay : null,
          dueDay: isCard ? payload.dueDay : null,
        };
      }),
    errorMessage: "Couldn't update this account.",
  });
}
