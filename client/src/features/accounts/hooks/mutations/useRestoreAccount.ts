import { restoreAccount } from "../../api/accounts.api";
import { useOptimisticAccountsMutation } from "./useOptimisticAccountsMutation";

export function useRestoreAccount() {
  return useOptimisticAccountsMutation<string>({
    mutationFn: restoreAccount,
    updater: (accounts, id) => accounts.map((a) => (a.id === id ? { ...a, active: true } : a)),
    errorMessage: "Couldn't restore this account.",
  });
}
