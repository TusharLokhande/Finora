import { SidebarMenuBadge } from "@/ui/sidebar";
import { useMembers } from "../hooks/queries/useMembers";

/** Admin-only: rendered inside the Access nav item, so the members query never fires for other users. */
export function AccessNavBadge() {
  const { data } = useMembers();
  const pending = data?.counts.pending ?? 0;

  return pending > 0 ? <SidebarMenuBadge className="bg-amber-500/20 text-amber-700 dark:text-amber-400">{pending}</SidebarMenuBadge> : null;
}
