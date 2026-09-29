import { useQuery } from "@tanstack/react-query";
import { getAccounts } from "../../api/accounts.api";

export const accountKeys = {
  all: ["accounts"] as const,
  list: () => [...accountKeys.all, "list"] as const,
};

export function useAccounts() {
  return useQuery({
    queryKey: accountKeys.list(),
    queryFn: getAccounts,
  });
}
