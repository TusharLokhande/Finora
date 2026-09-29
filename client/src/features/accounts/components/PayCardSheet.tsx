import { useEffect, useState } from "react";
import { format } from "date-fns";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
  SheetFooter,
  SheetClose,
} from "@/ui/sheet";
import { Button } from "@/ui/button";
import { Input } from "@/ui/input";
import { Label } from "@/ui/label";
import { Textarea } from "@/ui/textarea";
import { Select } from "@/ui/select";
import { DatePicker } from "@/components/DatePicker";
import type { ApiError } from "@/types/apiError.types";
import { formatCurrency } from "@/lib/currency";
import { payCardSchema } from "../schemas/account.schema";
import { usePayCard } from "../hooks/mutations/usePayCard";
import type { Account, PayCardInput } from "../types/account.types";

type SourceOption = { value: string; label: string };

interface PayCardSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  card?: Account;
  sourceAccounts: Account[];
}

export function PayCardSheet({ open, onOpenChange, card, sourceAccounts }: PayCardSheetProps) {
  const [formError, setFormError] = useState<string | null>(null);
  const payMutation = usePayCard();

  const form = useForm<PayCardInput>({
    resolver: zodResolver(payCardSchema),
    defaultValues: {
      sourceAccountId: "",
      amount: 0,
      date: format(new Date(), "yyyy-MM-dd"),
      note: null,
    },
  });

  useEffect(() => {
    if (!open) return;
    setFormError(null);
    form.reset({
      sourceAccountId: sourceAccounts[0]?.id ?? "",
      amount: 0,
      date: format(new Date(), "yyyy-MM-dd"),
      note: null,
    });
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, card]);

  const options: SourceOption[] = sourceAccounts.map((a) => ({ value: a.id, label: a.name }));

  function applyServerError(error: unknown): void {
    const apiError = error as ApiError;

    if (apiError.errors) {
      for (const [key, messages] of Object.entries(apiError.errors)) {
        const field = (key.charAt(0).toLowerCase() + key.slice(1)) as keyof PayCardInput;
        form.setError(field, { message: messages[0] });
      }
      return;
    }

    setFormError(apiError.message ?? "Something went wrong.");
  }

  function onSubmit(values: PayCardInput) {
    if (!card) return;
    setFormError(null);

    payMutation.mutate(
      { cardAccountId: card.id, payload: values },
      { onSuccess: () => onOpenChange(false), onError: applyServerError },
    );
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="overflow-y-auto">
        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-1 flex-col">
          <SheetHeader>
            <SheetTitle>Pay {card?.name}</SheetTitle>
            <SheetDescription>
              {card ? `Currently owe ${formatCurrency(card.outstanding ?? 0)}.` : ""}
            </SheetDescription>
          </SheetHeader>

          <div className="flex flex-col gap-4 px-4">
            <div className="flex flex-col gap-1.5">
              <Label>From</Label>
              <Controller
                control={form.control}
                name="sourceAccountId"
                render={({ field }) => (
                  <Select<SourceOption>
                    value={options.find((o) => o.value === field.value) ?? null}
                    onChange={(option) => field.onChange(option?.value ?? "")}
                    options={options}
                    isSearchable={false}
                    aria-invalid={!!form.formState.errors.sourceAccountId}
                  />
                )}
              />
              {form.formState.errors.sourceAccountId && (
                <p className="text-xs text-destructive">{form.formState.errors.sourceAccountId.message}</p>
              )}
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="pay-amount">Amount</Label>
              <Controller
                control={form.control}
                name="amount"
                render={({ field }) => (
                  <Input
                    id="pay-amount"
                    type="number"
                    step="0.01"
                    min={0}
                    autoFocus
                    value={Number.isNaN(field.value) ? "" : field.value}
                    onChange={(e) => field.onChange(e.target.value === "" ? 0 : Number(e.target.value))}
                    aria-invalid={!!form.formState.errors.amount}
                  />
                )}
              />
              {form.formState.errors.amount && (
                <p className="text-xs text-destructive">{form.formState.errors.amount.message}</p>
              )}
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="pay-date">Date</Label>
              <Controller
                control={form.control}
                name="date"
                render={({ field }) => (
                  <DatePicker
                    id="pay-date"
                    value={field.value}
                    onChange={(value) => field.onChange(value ?? "")}
                    aria-invalid={!!form.formState.errors.date}
                  />
                )}
              />
              {form.formState.errors.date && (
                <p className="text-xs text-destructive">{form.formState.errors.date.message}</p>
              )}
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="pay-note">Note (optional)</Label>
              <Controller
                control={form.control}
                name="note"
                render={({ field }) => (
                  <Textarea
                    id="pay-note"
                    value={field.value ?? ""}
                    onChange={(e) => field.onChange(e.target.value === "" ? null : e.target.value)}
                  />
                )}
              />
            </div>

            {formError && <p className="text-sm text-destructive">{formError}</p>}
          </div>

          <SheetFooter>
            <Button type="submit" disabled={payMutation.isPending || !card}>
              Pay
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
