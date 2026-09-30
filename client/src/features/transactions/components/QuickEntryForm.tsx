import { useEffect, useId, useRef } from "react";
import { format } from "date-fns";
import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { CornerDownLeft } from "lucide-react";
import { cn } from "@/lib/utils";
import { Button } from "@/ui/button";
import { Input } from "@/ui/input";
import { Select } from "@/ui/select";
import { DatePicker } from "@/components/DatePicker";
import { useHotkey } from "@/hooks/useHotkey";
import { TRANSACTION_TYPES } from "../constants/filters";
import { QUICK_ENTRY_AMOUNT_ID, focusQuickEntry } from "../constants/quickEntry";
import { createTransactionSchema } from "../schemas/transaction.schema";
import { useCreateTransaction } from "../hooks/mutations/useCreateTransaction";
import { useDescriptionSuggestions } from "../hooks/queries/useTransactions";
import { useLookupOptions, type LookupOption } from "../hooks/queries/useLookupOptions";
import type { CreateTransactionInput, TransactionType } from "../types/transaction.types";

const today = () => format(new Date(), "yyyy-MM-dd");

interface QuickEntryFormProps {
  /** "row" sits pinned above the ledger like a table row; "stacked" fits a sheet or empty state. */
  variant?: "row" | "stacked";
}

/**
 * Amount-first entry. Tab order: date, type, amount, category, account, description, submit.
 * Enter saves; only amount and description reset, so the rest carries over to the next entry.
 */
export function QuickEntryForm({ variant = "row" }: QuickEntryFormProps) {
  const { activeAccounts, categoriesFor } = useLookupOptions();
  const createMutation = useCreateTransaction();
  const listId = useId();
  const amountRef = useRef<HTMLInputElement | null>(null);

  const form = useForm<CreateTransactionInput>({
    resolver: zodResolver(createTransactionSchema),
    defaultValues: {
      type: "Expense",
      amount: NaN,
      accountId: "",
      toAccountId: null,
      categoryId: null,
      txnDate: today(),
      description: null,
      notes: null,
    },
  });

  const type = useWatch({ control: form.control, name: "type" });
  const accountId = useWatch({ control: form.control, name: "accountId" });
  const description = useWatch({ control: form.control, name: "description" });
  const { data: suggestions = [] } = useDescriptionSuggestions(description ?? "");

  const isTransfer = type === "Transfer";
  const categoryOptions = isTransfer ? [] : categoriesFor(type);
  const pick = (options: LookupOption[], id: string | null) => options.find((o) => o.value === id) ?? null;

  useHotkey("n", focusQuickEntry);

  // Default the account once accounts load.
  useEffect(() => {
    if (!accountId && activeAccounts[0]) form.setValue("accountId", activeAccounts[0].value);
  }, [accountId, activeAccounts, form]);

  function setType(next: TransactionType) {
    if (next === type) return;
    form.setValue("type", next);
    form.setValue("categoryId", null);
    form.setValue("toAccountId", null);
    form.clearErrors();
  }

  /** Picking a past description fills in what it was last logged with, unless the user already chose a category. */
  function prefillFrom(value: string) {
    const match = suggestions.find((s) => s.description.toLowerCase() === value.trim().toLowerCase());
    if (!match || match.type !== type || form.getFieldState("categoryId").isDirty) return;
    if (match.categoryId && categoryOptions.some((c) => c.value === match.categoryId)) {
      form.setValue("categoryId", match.categoryId, { shouldValidate: true });
    }
    if (activeAccounts.some((a) => a.value === match.accountId)) form.setValue("accountId", match.accountId);
  }

  function onSubmit(values: CreateTransactionInput) {
    // Clear right away so the next entry can start while this one saves.
    form.reset({ ...values, amount: NaN, description: null });
    // After the reset re-render; RHF's setFocus here loses to the submitting input.
    requestAnimationFrame(() => amountRef.current?.focus());

    createMutation.mutate(values, {
      // The mutation already rolled back and toasted; put the entry back if the user hasn't started a new one.
      onError: () => Number.isNaN(form.getValues("amount")) && form.reset(values),
    });
  }

  const errors = form.formState.errors;
  const firstError = Object.values(errors)[0]?.message;
  const stacked = variant === "stacked";

  const typeToggle = (
    <div role="radiogroup" aria-label="Type" className="flex shrink-0 rounded-lg border border-input p-0.5">
      {TRANSACTION_TYPES.map((t) => (
        <button
          key={t}
          type="button"
          role="radio"
          aria-checked={type === t}
          onClick={() => setType(t)}
          className={cn(
            "rounded-md px-2 py-0.5 text-xs font-medium text-muted-foreground transition-colors hover:text-foreground",
            stacked && "flex-1 py-1",
            type === t && "bg-muted text-foreground",
          )}
        >
          {t}
        </button>
      ))}
    </div>
  );

  // Transfers reuse the two select slots as From / To so tab order stays the same.
  const firstSelect = isTransfer ? (
    <Controller
      control={form.control}
      name="accountId"
      render={({ field }) => (
        <Select<LookupOption>
          aria-label="From account"
          placeholder="From account"
          options={activeAccounts}
          value={pick(activeAccounts, field.value)}
          onChange={(o) => field.onChange(o?.value ?? "")}
          aria-invalid={!!errors.accountId}
          menuPortalTarget={document.body}
        />
      )}
    />
  ) : (
    <Controller
      control={form.control}
      name="categoryId"
      render={({ field }) => (
        <Select<LookupOption>
          aria-label="Category"
          placeholder="Category"
          options={categoryOptions}
          value={pick(categoryOptions, field.value)}
          onChange={(o) => field.onChange(o?.value ?? null)}
          aria-invalid={!!errors.categoryId}
          menuPortalTarget={document.body}
        />
      )}
    />
  );

  const secondSelect = isTransfer ? (
    <Controller
      control={form.control}
      name="toAccountId"
      render={({ field }) => (
        <Select<LookupOption>
          aria-label="To account"
          placeholder="To account"
          options={activeAccounts.filter((a) => a.value !== accountId)}
          value={pick(activeAccounts, field.value)}
          onChange={(o) => field.onChange(o?.value ?? null)}
          aria-invalid={!!errors.toAccountId}
          menuPortalTarget={document.body}
        />
      )}
    />
  ) : (
    <Controller
      control={form.control}
      name="accountId"
      render={({ field }) => (
        <Select<LookupOption>
          aria-label="Account"
          placeholder="Account"
          options={activeAccounts}
          value={pick(activeAccounts, field.value)}
          onChange={(o) => field.onChange(o?.value ?? "")}
          aria-invalid={!!errors.accountId}
          menuPortalTarget={document.body}
        />
      )}
    />
  );

  return (
    <form
      onSubmit={form.handleSubmit(onSubmit)}
      aria-label="Add transaction"
      className={cn("flex flex-col gap-1.5", !stacked && "border-b border-border bg-muted/30 px-3 py-2")}
    >
      <div
        className={cn(
          stacked ? "grid grid-cols-2 gap-3" : "flex flex-wrap items-center gap-2 lg:flex-nowrap",
        )}
      >
        <Controller
          control={form.control}
          name="txnDate"
          render={({ field }) => (
            <DatePicker
              className={cn(!stacked && "w-34 shrink-0")}
              value={field.value}
              onChange={(value) => field.onChange(value ?? "")}
              aria-invalid={!!errors.txnDate}
            />
          )}
        />

        {typeToggle}

        <Controller
          control={form.control}
          name="amount"
          render={({ field }) => (
            <Input
              id={QUICK_ENTRY_AMOUNT_ID}
              ref={(el) => {
                field.ref(el);
                amountRef.current = el;
              }}
              type="number"
              inputMode="decimal"
              step="0.01"
              min={0}
              autoFocus
              aria-label="Amount"
              placeholder="Amount"
              className={cn("text-right font-mono tabular-nums", stacked ? "col-span-2 h-10 text-lg" : "w-28 shrink-0")}
              value={Number.isNaN(field.value) ? "" : field.value}
              onChange={(e) => field.onChange(e.target.value === "" ? NaN : Number(e.target.value))}
              aria-invalid={!!errors.amount}
            />
          )}
        />

        <div className={cn(stacked ? "col-span-2" : "w-48 shrink-0")}>{firstSelect}</div>
        <div className={cn(stacked ? "col-span-2" : "w-40 shrink-0")}>{secondSelect}</div>

        <Controller
          control={form.control}
          name="description"
          render={({ field }) => (
            <Input
              ref={field.ref}
              list={listId}
              aria-label="Description"
              placeholder="Description"
              autoComplete="off"
              maxLength={200}
              className={cn(stacked ? "col-span-2" : "min-w-40 flex-1")}
              value={field.value ?? ""}
              onChange={(e) => {
                field.onChange(e.target.value || null);
                prefillFrom(e.target.value);
              }}
            />
          )}
        />
        <datalist id={listId}>
          {suggestions.map((s) => (
            <option key={s.description} value={s.description} />
          ))}
        </datalist>

        <Button type="submit" className={cn(stacked && "col-span-2")}>
          <CornerDownLeft /> Add
        </Button>
      </div>

      {firstError && <p className="text-xs text-destructive">{firstError}</p>}
    </form>
  );
}
