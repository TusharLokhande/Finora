import { useQuery } from "@tanstack/react-query";
import { getAccessSettings, getAuditLog, getMembers } from "../../api/access.api";

export const accessKeys = {
  all: ["access"] as const,
  members: () => [...accessKeys.all, "members"] as const,
  settings: () => [...accessKeys.all, "settings"] as const,
  audit: (page: number) => [...accessKeys.all, "audit", page] as const,
};

export function useMembers() {
  return useQuery({
    queryKey: accessKeys.members(),
    queryFn: getMembers,
    refetchInterval: 60 * 1000, // keeps the nav's pending badge fresh
  });
}

export function useAccessSettings() {
  return useQuery({ queryKey: accessKeys.settings(), queryFn: getAccessSettings });
}

export const AUDIT_PAGE_SIZE = 20;

export function useAuditLog(page: number) {
  return useQuery({
    queryKey: accessKeys.audit(page),
    queryFn: () => getAuditLog(page, AUDIT_PAGE_SIZE),
    placeholderData: (previous) => previous,
  });
}
