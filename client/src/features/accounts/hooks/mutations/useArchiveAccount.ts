import { archiveAccount } from "../../api/accounts.api";
import { useOptimisticAccountsMutation } from "./useOptimisticAccountsMutation";

export function useArchiveAccount() {
  return useOptimisticAccountsMutation<string>({
    mutationFn: archiveAccount,
    updater: (accounts, id) => accounts.map((a) => (a.id === id ? { ...a, active: false } : a)),
    errorMessage: "Couldn't archive this account.",
  });
}
