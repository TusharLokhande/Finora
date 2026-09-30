import { Copy, PiggyBank } from "lucide-react";
import { Button } from "@/ui/button";
import { monthLabel, monthName, shiftMonth } from "../constants/months";

interface NoBudgetsBannerProps {
  month: string;
  /** Whether the previous month has anything to copy; hides the copy action when it doesn't. */
  canCopy: boolean;
  copying: boolean;
  onCopy: () => void;
  onStartFresh: () => void;
}

export function NoBudgetsBanner({ month, canCopy, copying, onCopy, onStartFresh }: NoBudgetsBannerProps) {
  return (
    <div role="status" className="flex flex-wrap items-center gap-3 rounded-xl border border-border bg-accent/40 px-4 py-3">
      <PiggyBank className="size-4 shrink-0 text-accent-foreground" />
      <p className="flex-1 text-sm text-foreground">No budgets set for {monthLabel(month)} yet.</p>
      <div className="flex items-center gap-2">
        {canCopy && (
          <Button size="sm" onClick={onCopy} disabled={copying}>
            <Copy /> Copy {monthName(shiftMonth(month, -1))}&apos;s budgets
          </Button>
        )}
        <Button size="sm" variant="ghost" onClick={onStartFresh}>
          Start fresh
        </Button>
      </div>
    </div>
  );
}
