import type { UserStatus } from "@/types/auth.types";
import { cn } from "@/lib/utils";

const TONES: Record<UserStatus, string> = {
  Pending: "bg-amber-500/15 text-amber-700 dark:text-amber-400",
  Approved: "bg-emerald-500/15 text-emerald-700 dark:text-emerald-400",
  Suspended: "bg-destructive/10 text-destructive",
  Rejected: "bg-destructive/10 text-destructive",
};

export function MemberStatusBadge({ status }: { status: UserStatus }) {
  return <span className={cn("rounded-full px-2 py-0.5 text-xs font-medium", TONES[status])}>{status}</span>;
}
