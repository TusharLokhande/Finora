import { useRef } from "react";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import { formatCurrency, toMajorUnits, toMinorUnits } from "@/lib/currency";
import { USAGE_TONE_SOFT, USAGE_TONE_TEXT, type UsageTone } from "@/lib/usageTone";
import { UsageBar } from "@/components/UsageBar";
import { Input } from "@/ui/input";
import { getCategoryIcon } from "@/features/categories";
import { budgetAmountSchema } from "../schemas/budget.schema";
import { BUDGET_ROW, SUB_INDENT } from "../constants/layout";
import type { BudgetLine } from "../types/budget.types";

interface BudgetRowProps {
  line: BudgetLine;
  editing: boolean;
  onStartEdit: () => void;
  onCancel: () => void;
  /** Minor units. */
  onSave: (amount: number) => void;
}

/**
 * One category's budget. Clicking "{spent} of {budgeted}" (or "Set budget") swaps in an amount
 * input: Enter or blur saves, Escape cancels — same interaction as the Transactions table.
 */
export function BudgetRow({ line, editing, onStartEdit, onCancel, onSave }: BudgetRowProps) {
  // Armed when the editor gains focus; cleared on the first save/cancel so Enter-then-blur can't save twice.
  const open = useRef(false);

  const Icon = getCategoryIcon(line.icon);
  const hasBudget = line.budgeted !== null;
  const tone = line.status.toLowerCase() as UsageTone;

  function commit(raw: string) {
    if (!open.current) return;
    open.current = false;
    if (raw.trim() === "") return onCancel();

    const parsed = budgetAmountSchema.safeParse(Number(raw));
    if (!parsed.success) {
      toast.error(parsed.error.issues[0]?.message ?? "That amount isn't valid.");
      return onCancel();
    }
    const amount = toMinorUnits(parsed.data);
    if (amount === line.budgeted) return onCancel();
    onSave(amount);
  }

  function cancel() {
    open.current = false;
    onCancel();
  }

  const editor = (
    <span className="flex items-center gap-1.5 font-mono text-sm tabular-nums">
      {hasBudget && <span className="text-muted-foreground">{formatCurrency(line.spent)} of</span>}
      <Input
        type="number"
        inputMode="decimal"
        step="0.01"
        min={0}
        autoFocus
        onFocus={() => (open.current = true)}
        aria-label={`Budget for ${line.categoryName}`}
        placeholder="Amount"
        defaultValue={hasBudget ? toMajorUnits(line.budgeted!) : ""}
        className="h-7 w-28 text-right font-mono tabular-nums"
        onKeyDown={(e) => {
          if (e.key === "Escape") cancel();
          if (e.key === "Enter") commit(e.currentTarget.value);
        }}
        onBlur={(e) => commit(e.currentTarget.value)}
      />
    </span>
  );

  return (
    <div className={cn(BUDGET_ROW, line.parentId && SUB_INDENT, !line.active && "opacity-60")}>
      <div className="flex min-h-7 items-center gap-3">
        <span
          className={cn(
            "flex size-7 shrink-0 items-center justify-center rounded-lg",
            hasBudget ? USAGE_TONE_SOFT[tone] : "bg-muted text-muted-foreground",
          )}
        >
          <Icon className="size-3.5" />
        </span>

        <span className="min-w-0 flex-1 truncate text-sm text-foreground">
          {line.categoryName}
          {!line.active && <span className="ml-2 text-xs text-muted-foreground">Archived</span>}
        </span>

        {editing ? (
          editor
        ) : hasBudget ? (
          <>
            <button
              type="button"
              onClick={onStartEdit}
              title="Edit budget"
              className="-mx-1 cursor-text rounded-md px-1 py-0.5 font-mono text-sm tabular-nums text-foreground hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring/50 focus-visible:outline-none"
            >
              {formatCurrency(line.spent)} <span className="text-muted-foreground">of</span> {formatCurrency(line.budgeted!)}
            </button>
            <span className={cn("w-12 shrink-0 text-right font-mono text-xs tabular-nums", USAGE_TONE_TEXT[tone])}>
              {line.percentUsed !== null ? `${Math.round(line.percentUsed)}%` : "—"}
            </span>
          </>
        ) : (
          <span className="flex items-center gap-2 text-xs">
            {line.spent > 0 && (
              <span className="font-mono tabular-nums text-muted-foreground">{formatCurrency(line.spent)} spent ·</span>
            )}
            <button type="button" onClick={onStartEdit} className="text-primary hover:underline" disabled={!line.active}>
              Set budget
            </button>
          </span>
        )}
      </div>

      {hasBudget && (
        <UsageBar percent={line.percentUsed ?? (line.spent > 0 ? 100 : 0)} tone={tone} />
      )}
    </div>
  );
}
