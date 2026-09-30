import { useMemo, useState } from "react";
import { Controller, useForm, useWatch } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { cn } from "@/lib/utils";
import { Sheet, SheetClose, SheetContent, SheetFooter, SheetHeader, SheetTitle } from "@/ui/sheet";
import { Button } from "@/ui/button";
import { Input } from "@/ui/input";
import { Label } from "@/ui/label";
import { Select } from "@/ui/select";
import { Textarea } from "@/ui/textarea";
import { DatePicker } from "@/components/DatePicker";
import type { ApiError } from "@/types/apiError.types";
import { TRANSACTION_TYPES } from "../constants/filters";
import { createTransactionSchema, toFormValues } from "../schemas/transaction.schema";
import { useUpdateTransaction } from "../hooks/mutations/useUpdateTransaction";
import { useLookupOptions, type LookupOption } from "../hooks/queries/useLookupOptions";
import type { CreateTransactionInput, Transaction } from "../types/transaction.types";

interface EditTransactionSheetProps {
  transaction?: Transaction;
  onOpenChange: (open: boolean) => void;
}

export function EditTransactionSheet({ transaction, onOpenChange }: EditTransactionSheetProps) {
  const { allAccounts, activeAccounts, allCategories, categoriesFor } = useLookupOptions();
  const updateMutation = useUpdateTransaction();
  const [formError, setFormError] = useState<string | null>(null);

  // `values` re-fills the form whenever a different transaction is opened.
  const values = useMemo(() => (transaction ? toFormValues(transaction) : undefined), [transaction]);
  const form = useForm<CreateTransactionInput>({ resolver: zodResolver(createTransactionSchema), values });
  const type = useWatch({ control: form.control, name: "type" });
  const accountId = useWatch({ control: form.control, name: "accountId" });
  const errors = form.formState.errors;

  // Lookups use every account/category so archived ones still display; options offer only active ones.
  const pick = (options: LookupOption[], id: string | null) => options.find((o) => o.value === id) ?? null;
  const categoryOptions = type && type !== "Transfer" ? categoriesFor(type) : [];

  function onSubmit(values: CreateTransactionInput) {
    if (!transaction) return;
    setFormError(null);
    updateMutation.mutate(
      { id: transaction.id, payload: values },
      {
        onSuccess: () => onOpenChange(false),
        onError: (error) => setFormError((error as unknown as ApiError).message ?? "Something went wrong."),
      },
    );
  }

  return (
    <Sheet
      open={!!transaction}
      onOpenChange={(open) => {
        if (!open) setFormError(null);
        onOpenChange(open);
      }}
    >
      <SheetContent className="overflow-y-auto">
        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-1 flex-col">
          <SheetHeader>
            <SheetTitle>Edit transaction</SheetTitle>
          </SheetHeader>

          <div className="flex flex-col gap-4 px-4">
            <div role="radiogroup" aria-label="Type" className="flex rounded-lg border border-input p-0.5">
              {TRANSACTION_TYPES.map((t) => (
                <button
                  key={t}
                  type="button"
                  role="radio"
                  aria-checked={type === t}
                  onClick={() => {
                    if (t === type) return;
                    form.setValue("type", t);
                    form.setValue("categoryId", null);
                    form.setValue("toAccountId", null);
                    form.clearErrors();
                  }}
                  className={cn(
                    "flex-1 rounded-md px-2.5 py-1 text-xs font-medium text-muted-foreground transition-colors hover:text-foreground",
                    type === t && "bg-muted text-foreground",
                  )}
                >
                  {t}
                </button>
              ))}
            </div>

            <Field label="Amount" error={errors.amount?.message}>
              <Controller
                control={form.control}
                name="amount"
                render={({ field }) => (
                  <Input
                    ref={field.ref}
                    type="number"
                    inputMode="decimal"
                    step="0.01"
                    min={0}
                    className="tabular-nums"
                    value={Number.isNaN(field.value) || field.value == null ? "" : field.value}
                    onChange={(e) => field.onChange(e.target.value === "" ? NaN : Number(e.target.value))}
                    aria-invalid={!!errors.amount}
                  />
                )}
              />
            </Field>

            {type !== "Transfer" && (
              <Field label="Category" error={errors.categoryId?.message}>
                <Controller
                  control={form.control}
                  name="categoryId"
                  render={({ field }) => (
                    <Select<LookupOption>
                      options={categoryOptions}
                      value={pick(allCategories, field.value)}
                      onChange={(o) => field.onChange(o?.value ?? null)}
                      aria-invalid={!!errors.categoryId}
                    />
                  )}
                />
              </Field>
            )}

            <Field label={type === "Transfer" ? "From account" : "Account"} error={errors.accountId?.message}>
              <Controller
                control={form.control}
                name="accountId"
                render={({ field }) => (
                  <Select<LookupOption>
                    options={activeAccounts}
                    value={pick(allAccounts, field.value)}
                    onChange={(o) => field.onChange(o?.value ?? "")}
                    aria-invalid={!!errors.accountId}
                  />
                )}
              />
            </Field>

            {type === "Transfer" && (
              <Field label="To account" error={errors.toAccountId?.message}>
                <Controller
                  control={form.control}
                  name="toAccountId"
                  render={({ field }) => (
                    <Select<LookupOption>
                      options={activeAccounts.filter((a) => a.value !== accountId)}
                      value={pick(allAccounts, field.value)}
                      onChange={(o) => field.onChange(o?.value ?? null)}
                      aria-invalid={!!errors.toAccountId}
                    />
                  )}
                />
              </Field>
            )}

            <Field label="Date" error={errors.txnDate?.message}>
              <Controller
                control={form.control}
                name="txnDate"
                render={({ field }) => (
                  <DatePicker
                    value={field.value}
                    onChange={(value) => field.onChange(value ?? "")}
                    aria-invalid={!!errors.txnDate}
                  />
                )}
              />
            </Field>

            <Field label="Description" error={errors.description?.message}>
              <Input maxLength={200} {...form.register("description", { setValueAs: (v: string) => v || null })} />
            </Field>

            <Field label="Notes (optional)" error={errors.notes?.message}>
              <Textarea {...form.register("notes", { setValueAs: (v: string) => v || null })} />
            </Field>

            {formError && <p className="text-sm text-destructive">{formError}</p>}
          </div>

          <SheetFooter>
            <Button type="submit" disabled={updateMutation.isPending}>
              Save
            </Button>
            <SheetClose asChild>
              <Button type="button" variant="outline">
                Cancel
              </Button>
            </SheetClose>
          </SheetFooter>
        </form>
      </SheetContent>
    </Sheet>
  );
}

function Field({ label, error, children }: { label: string; error?: string; children: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-1.5">
      <Label>{label}</Label>
      {children}
      {error && <p className="text-xs text-destructive">{error}</p>}
    </div>
  );
}
