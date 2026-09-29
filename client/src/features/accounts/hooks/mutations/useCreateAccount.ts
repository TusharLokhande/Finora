import { toMinorUnits } from "@/lib/currency";
import { createAccount } from "../../api/accounts.api";
import { useOptimisticAccountsMutation } from "./useOptimisticAccountsMutation";
import type { Account, CreateAccountInput } from "../../types/account.types";

function toOptimisticAccount(payload: CreateAccountInput, sortOrder: number): Account {
  const isCard = payload.type === "CreditCard";
  const openingMinor = toMinorUnits(payload.openingBalance);
  const creditLimitMinor = payload.creditLimit != null ? toMinorUnits(payload.creditLimit) : null;

  return {
    id: crypto.randomUUID(),
    name: payload.name,
    type: payload.type,
    color: null,
    sortOrder,
    active: true,
    balance: isCard ? null : openingMinor,
    outstanding: isCard ? openingMinor : null,
    creditLimit: isCard ? creditLimitMinor : null,
    availableCredit: isCard && creditLimitMinor != null ? creditLimitMinor - openingMinor : null,
    statementDay: isCard ? payload.statementDay : null,
    dueDay: isCard ? payload.dueDay : null,
    nextDueDate: null,
  };
}

export function useCreateAccount() {
  return useOptimisticAccountsMutation<CreateAccountInput>({
    mutationFn: createAccount,
    updater: (accounts, payload) => [...accounts, toOptimisticAccount(payload, accounts.length)],
    errorMessage: "Couldn't create this account.",
  });
}
