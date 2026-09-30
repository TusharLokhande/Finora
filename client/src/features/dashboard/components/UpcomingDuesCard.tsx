import { useState } from "react";
import { format, parseISO } from "date-fns";
import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { Button } from "@/ui/button";
import { PayCardSheet, useAccounts, type Account } from "@/features/accounts";
import { useUpcomingDues } from "../hooks/queries/useDashboard";
import type { UpcomingDue } from "../types/dashboard.types";
import { EmptyNote, ListSkeleton, SectionCard, SectionError } from "@/components/SectionCard";

/** Same thresholds as the Accounts page's card chips: red within 2 days or overdue, amber within a week. */
function dueLabel({ daysUntilDue: d, dueDate }: UpcomingDue): { text: string; className: string } {
  if (d < 0) return { text: `Overdue by ${-d} day${d === -1 ? "" : "s"}`, className: "text-negative" };
  if (d === 0) return { text: "Due today", className: "text-negative" };
  if (d === 1) return { text: "Due tomorrow", className: "text-negative" };
  if (d <= 2) return { text: `Due in ${d} days`, className: "text-negative" };
  if (d <= 7) return { text: `Due in ${d} days`, className: "text-amber-600 dark:text-amber-400" };
  return { text: `Due ${format(parseISO(dueDate), "d MMM")}`, className: "text-muted-foreground" };
}

/** Credit cards by nearest due date, each with the Accounts page's Pay flow. */
export function UpcomingDuesCard() {
  const { data, isLoading, isError, refetch } = useUpcomingDues();
  const { data: accounts = [] } = useAccounts();
  const [paying, setPaying] = useState<Account | undefined>();
  const sourceAccounts = accounts.filter((a) => a.type !== "CreditCard" && a.active);

  return (
    <SectionCard title="Upcoming dues">
      {isError ? (
        <SectionError onRetry={refetch} />
      ) : isLoading || !data ? (
        <ListSkeleton rows={2} />
      ) : data.length === 0 ? (
        <EmptyNote>No credit cards.</EmptyNote>
      ) : (
        <ul className="-my-1 divide-y divide-border/60">
          {data.map((due) => {
            const label = dueLabel(due);
            const card = accounts.find((a) => a.id === due.accountId);
            return (
              <li key={due.accountId} className="flex items-center justify-between gap-3 py-2">
                <div className="min-w-0">
                  <p className="truncate text-sm">{due.name}</p>
                  <p className="text-xs">
                    <span className={cn(due.outstanding > 0 ? label.className : "text-muted-foreground")}>
                      {due.outstanding > 0 ? label.text : "Nothing owed"}
                    </span>
                    {due.outstanding > 0 && (
                      <span className="font-mono tabular-nums text-muted-foreground"> · {formatCurrency(due.outstanding)}</span>
                    )}
                  </p>
                </div>
                <Button size="sm" variant="outline" disabled={!card || due.outstanding <= 0} onClick={() => setPaying(card)}>
                  Pay
                </Button>
              </li>
            );
          })}
        </ul>
      )}

      <PayCardSheet
        open={!!paying}
        onOpenChange={(open) => !open && setPaying(undefined)}
        card={paying}
        sourceAccounts={sourceAccounts}
      />
    </SectionCard>
  );
}
