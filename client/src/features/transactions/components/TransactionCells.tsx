import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { getCategoryIcon } from "@/features/categories";
import type { CategoryOption } from "../hooks/queries/useLookupOptions";
import type { Transaction, TransactionType } from "../types/transaction.types";

/** Income green with "+", transfers muted italic, expenses default with "−". Never red for ordinary spend. */
export function AmountText({ type, amount, className }: { type: TransactionType; amount: number; className?: string }) {
  return (
    <span
      className={cn(
        "font-mono tabular-nums whitespace-nowrap",
        type === "Income" && "text-positive",
        type === "Transfer" && "text-muted-foreground italic",
        className,
      )}
    >
      {type === "Income" ? "+" : type === "Expense" ? "−" : ""}
      {formatCurrency(amount)}
    </span>
  );
}

/** Signed net for a day header or total: income minus expense, transfers ignored. */
export function NetAmount({ amount }: { amount: number }) {
  return (
    <span className={cn("font-mono tabular-nums whitespace-nowrap", amount > 0 ? "text-positive" : "text-muted-foreground")}>
      {amount > 0 ? "+" : amount < 0 ? "−" : ""}
      {formatCurrency(Math.abs(amount))}
    </span>
  );
}

export function CategoryLabel({ transaction: t, category }: { transaction: Transaction; category?: CategoryOption }) {
  if (t.type === "Transfer") {
    return <span className="truncate text-muted-foreground italic">→ {t.toAccountName}</span>;
  }

  const Icon = getCategoryIcon(category?.icon);
  const color = category?.color ?? "#64748b";

  return (
    <span className="flex min-w-0 items-center gap-2">
      <span
        className="flex size-5 shrink-0 items-center justify-center rounded-md"
        style={{ backgroundColor: `${color}20`, color }}
      >
        <Icon className="size-3" />
      </span>
      <span className="truncate" title={t.parentCategoryName ? `${t.parentCategoryName} › ${t.categoryName}` : undefined}>
        {t.categoryName}
      </span>
    </span>
  );
}
