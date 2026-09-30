import { memo, useRef, useState, type KeyboardEvent, type ReactNode } from "react";
import { MoreHorizontal, Pencil, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { cn } from "@/lib/utils";
import { toMajorUnits } from "@/lib/currency";
import { Button } from "@/ui/button";
import { Checkbox } from "@/ui/checkbox";
import { Input } from "@/ui/input";
import { Select } from "@/ui/select";
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/ui/dropdown-menu";
import { createTransactionSchema, toFormValues } from "../schemas/transaction.schema";
import type { LookupOption, useLookupOptions } from "../hooks/queries/useLookupOptions";
import { isTempId } from "../hooks/mutations/useOptimisticTransactionsMutation";
import type { CreateTransactionInput, Transaction } from "../types/transaction.types";
import { ROW_GRID, ROW_HEIGHT, formatShortDate } from "../constants/ledger";
import { AmountText, CategoryLabel } from "./TransactionCells";

type EditableField = "txnDate" | "description" | "category" | "accountId" | "amount";

interface TransactionRowProps {
  transaction: Transaction;
  selected: boolean;
  /** Any row is selected, so checkboxes stay visible. */
  selecting: boolean;
  onToggleSelect: (id: string) => void;
  onEdit: (transaction: Transaction) => void;
  onDelete: (transaction: Transaction) => void;
  onUpdate: (id: string, payload: CreateTransactionInput) => void;
  /** Passed down once from the table rather than rebuilt per row. */
  lookups: ReturnType<typeof useLookupOptions>;
}

/** A ledger row. Clicking a cell swaps it for its editor; Enter/blur/pick saves, Escape cancels. */
export const TransactionRow = memo(function TransactionRow({
  transaction: t,
  selected,
  selecting,
  onToggleSelect,
  onEdit,
  onDelete,
  onUpdate,
  lookups: { activeAccounts, allAccounts, categoryById, categoriesFor },
}: TransactionRowProps) {
  const [editing, setEditing] = useState<EditableField | null>(null);
  // Mirrors `editing` synchronously so Enter-then-blur or Escape-then-unmount can't save twice.
  const open = useRef(false);
  const pending = isTempId(t.id);
  const isTransfer = t.type === "Transfer";

  function start(field: EditableField) {
    open.current = true;
    setEditing(field);
  }

  function cancel() {
    open.current = false;
    setEditing(null);
  }

  function commit(patch: Partial<CreateTransactionInput>) {
    if (!open.current) return;
    cancel();

    const original = toFormValues(t);
    const parsed = createTransactionSchema.safeParse({ ...original, ...patch });
    if (!parsed.success) {
      toast.error(parsed.error.issues[0]?.message ?? "That value isn't valid.");
      return;
    }
    if (JSON.stringify(parsed.data) === JSON.stringify(createTransactionSchema.parse(original))) return;
    onUpdate(t.id, parsed.data);
  }

  const onInputKey = (e: KeyboardEvent<HTMLInputElement>, toPatch: (value: string) => Partial<CreateTransactionInput>) => {
    if (e.key === "Escape") cancel();
    if (e.key === "Enter") commit(toPatch(e.currentTarget.value));
  };

  /** A cell that becomes its editor on click. A plain function, not a component, so editors never remount. */
  function cell(field: EditableField, className: string | undefined, display: ReactNode, editor: ReactNode) {
    if (editing === field) return <div className={cn("min-w-0", className)}>{editor}</div>;
    return (
      <button
        type="button"
        disabled={pending}
        onClick={() => start(field)}
        className={cn(
          "-mx-1 min-w-0 cursor-text truncate rounded-md px-1 py-0.5 text-left hover:bg-muted focus-visible:ring-2 focus-visible:ring-ring/50 focus-visible:outline-none",
          className,
        )}
      >
        {display}
      </button>
    );
  }

  const pick = (options: LookupOption[], id: string | null) => options.find((o) => o.value === id) ?? null;
  const selectProps = {
    autoFocus: true,
    defaultMenuIsOpen: true,
    menuPortalTarget: document.body,
    onBlur: cancel,
    onKeyDown: (e: KeyboardEvent) => e.key === "Escape" && cancel(),
    className: "w-full",
  };

  const dateEditor = (
    <Input
      type="date"
      autoFocus
      defaultValue={t.txnDate}
      className="h-7 px-1.5 text-xs"
      onKeyDown={(e) => onInputKey(e, (txnDate) => ({ txnDate }))}
      onBlur={(e) => commit({ txnDate: e.currentTarget.value })}
    />
  );

  const descriptionEditor = (
    <Input
      autoFocus
      maxLength={200}
      defaultValue={t.description ?? ""}
      className="h-7"
      onKeyDown={(e) => onInputKey(e, (v) => ({ description: v.trim() || null }))}
      onBlur={(e) => commit({ description: e.currentTarget.value.trim() || null })}
    />
  );

  // For a transfer the category column holds the destination account.
  const categoryEditor = isTransfer ? (
    <Select<LookupOption>
      {...selectProps}
      options={activeAccounts.filter((a) => a.value !== t.accountId)}
      value={pick(allAccounts, t.toAccountId)}
      onChange={(o) => o && commit({ toAccountId: o.value })}
    />
  ) : (
    <Select<LookupOption>
      {...selectProps}
      options={categoriesFor(t.type)}
      value={t.categoryId ? (categoryById.get(t.categoryId) ?? null) : null}
      onChange={(o) => o && commit({ categoryId: o.value })}
    />
  );

  const accountEditor = (
    <Select<LookupOption>
      {...selectProps}
      options={activeAccounts.filter((a) => a.value !== t.toAccountId)}
      value={pick(allAccounts, t.accountId)}
      onChange={(o) => o && commit({ accountId: o.value })}
    />
  );

  const amountEditor = (
    <Input
      type="number"
      inputMode="decimal"
      step="0.01"
      min={0}
      autoFocus
      defaultValue={toMajorUnits(t.amount)}
      className="h-7 text-right font-mono tabular-nums"
      onKeyDown={(e) => onInputKey(e, (v) => ({ amount: Number(v) }))}
      onBlur={(e) => commit({ amount: Number(e.currentTarget.value) })}
    />
  );

  return (
    <div
      role="row"
      aria-selected={selected}
      className={cn(
        ROW_GRID,
        ROW_HEIGHT,
        "group border-b border-border/60 text-sm last:border-b-0 hover:bg-muted/40",
        selected && "bg-accent/40 hover:bg-accent/50",
        pending && "opacity-60",
      )}
    >
      <Checkbox
        aria-label="Select transaction"
        checked={selected}
        disabled={pending}
        onCheckedChange={() => onToggleSelect(t.id)}
        className={cn(
          "opacity-0 group-hover:opacity-100 focus-visible:opacity-100",
          (selected || selecting) && "opacity-100",
        )}
      />

      {cell("txnDate", "hidden text-muted-foreground tabular-nums md:block", formatShortDate(t.txnDate), dateEditor)}

      {cell(
        "description",
        undefined,
        <>
          {t.description ?? <span className="text-muted-foreground">—</span>}
          {t.notes && <span className="ml-2 text-xs text-muted-foreground">{t.notes}</span>}
        </>,
        descriptionEditor,
      )}

      {cell(
        "category",
        "hidden md:block",
        <CategoryLabel transaction={t} category={t.categoryId ? categoryById.get(t.categoryId) : undefined} />,
        categoryEditor,
      )}

      {cell("accountId", "hidden text-muted-foreground md:block", t.accountName, accountEditor)}

      {cell("amount", "justify-self-end text-right", <AmountText type={t.type} amount={t.amount} />, amountEditor)}

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button
            variant="ghost"
            size="icon-sm"
            disabled={pending}
            aria-label="Transaction actions"
            className="opacity-0 group-hover:opacity-100 focus-visible:opacity-100 aria-expanded:opacity-100"
          >
            <MoreHorizontal />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onSelect={() => onEdit(t)}>
            <Pencil /> Edit
          </DropdownMenuItem>
          <DropdownMenuItem variant="destructive" onSelect={() => onDelete(t)}>
            <Trash2 /> Delete
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
});
