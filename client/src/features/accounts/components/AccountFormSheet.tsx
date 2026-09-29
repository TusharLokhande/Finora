import { useEffect, useState } from "react";
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
import { Tabs, TabsList, TabsTrigger } from "@/ui/tabs";
import { ErrorStatus } from "@/types/errorStatus.enum";
import type { ApiError } from "@/types/apiError.types";
import { toMajorUnits } from "@/lib/currency";
import { createAccountSchema, type CreateAccountFormValues } from "../schemas/account.schema";
import { ACCOUNT_TYPE_LABELS, ACCOUNT_TYPE_OPTIONS } from "../constants/accountTypes";
import { useCreateAccount } from "../hooks/mutations/useCreateAccount";
import { useUpdateAccount } from "../hooks/mutations/useUpdateAccount";
import type { Account } from "../types/account.types";

interface AccountFormSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  account?: Account;
}

const EMPTY_VALUES: CreateAccountFormValues = {
  name: "",
  type: "Bank",
  openingBalance: 0,
  creditLimit: null,
  statementDay: null,
  dueDay: null,
};

export function AccountFormSheet({ open, onOpenChange, account }: AccountFormSheetProps) {
  const isEdit = !!account;
  const [formError, setFormError] = useState<string | null>(null);
  const createMutation = useCreateAccount();
  const updateMutation = useUpdateAccount();
  const isPending = createMutation.isPending || updateMutation.isPending;

  const form = useForm<CreateAccountFormValues>({
    resolver: zodResolver(createAccountSchema),
    defaultValues: EMPTY_VALUES,
  });

  useEffect(() => {
    if (!open) return;
    setFormError(null);

    if (account) {
      form.reset({
        name: account.name,
        type: account.type,
        openingBalance: toMajorUnits(
          account.type === "CreditCard" ? (account.outstanding ?? 0) : (account.balance ?? 0),
        ),
        creditLimit: account.creditLimit != null ? toMajorUnits(account.creditLimit) : null,
        statementDay: account.statementDay,
        dueDay: account.dueDay,
      });
    } else {
      form.reset(EMPTY_VALUES);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, account]);

  const type = form.watch("type");
  const isCard = type === "CreditCard";

  function applyServerError(error: unknown): void {
    const apiError = error as ApiError;

    if (apiError.errors) {
      for (const [key, messages] of Object.entries(apiError.errors)) {
        const field = (key.charAt(0).toLowerCase() + key.slice(1)) as keyof CreateAccountFormValues;
        form.setError(field, { message: messages[0] });
      }
      return;
    }

    if (apiError.status === ErrorStatus.Duplicate) {
      form.setError("name", { message: apiError.message });
      return;
    }

    setFormError(apiError.message ?? "Something went wrong.");
  }

  function onSubmit(values: CreateAccountFormValues) {
    setFormError(null);

    if (account) {
      updateMutation.mutate(
        {
          id: account.id,
          payload: {
            name: values.name,
            openingBalance: values.openingBalance,
            creditLimit: values.creditLimit,
            statementDay: values.statementDay,
            dueDay: values.dueDay,
          },
        },
        { onSuccess: () => onOpenChange(false), onError: applyServerError },
      );
    } else {
      createMutation.mutate(values, {
        onSuccess: () => onOpenChange(false),
        onError: applyServerError,
      });
    }
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="overflow-y-auto">
        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-1 flex-col">
          <SheetHeader>
            <SheetTitle>{isEdit ? "Edit account" : "New account"}</SheetTitle>
            <SheetDescription>
              {isEdit ? "Update this account's details." : "Add a bank, cash, wallet or credit card account."}
            </SheetDescription>
          </SheetHeader>

          <div className="flex flex-col gap-4 px-4">
            <div className="flex flex-col gap-1.5">
              <Label>Type</Label>
              {isEdit ? (
                <p className="text-sm text-muted-foreground">{ACCOUNT_TYPE_LABELS[type]}</p>
              ) : (
                <Controller
                  control={form.control}
                  name="type"
                  render={({ field }) => (
                    <Tabs value={field.value} onValueChange={field.onChange}>
                      <TabsList className="w-full">
                        {ACCOUNT_TYPE_OPTIONS.map((option) => (
                          <TabsTrigger key={option} value={option}>
                            {ACCOUNT_TYPE_LABELS[option]}
                          </TabsTrigger>
                        ))}
                      </TabsList>
                    </Tabs>
                  )}
                />
              )}
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="account-name">Name</Label>
              <Input
                id="account-name"
                autoFocus
                {...form.register("name")}
                aria-invalid={!!form.formState.errors.name}
              />
              {form.formState.errors.name && (
                <p className="text-xs text-destructive">{form.formState.errors.name.message}</p>
              )}
            </div>

            <div className="flex flex-col gap-1.5">
              <Label htmlFor="account-opening-balance">
                {isCard ? "Current amount owed" : "Opening balance"}
              </Label>
              <Controller
                control={form.control}
                name="openingBalance"
                render={({ field }) => (
                  <Input
                    id="account-opening-balance"
                    type="number"
                    step="0.01"
                    min={0}
                    value={Number.isNaN(field.value) ? "" : field.value}
                    onChange={(e) => field.onChange(e.target.value === "" ? 0 : Number(e.target.value))}
                    aria-invalid={!!form.formState.errors.openingBalance}
                  />
                )}
              />
              {form.formState.errors.openingBalance && (
                <p className="text-xs text-destructive">{form.formState.errors.openingBalance.message}</p>
              )}
            </div>

            {isCard && (
              <>
                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="account-credit-limit">Credit limit</Label>
                  <Controller
                    control={form.control}
                    name="creditLimit"
                    render={({ field }) => (
                      <Input
                        id="account-credit-limit"
                        type="number"
                        step="0.01"
                        min={0}
                        value={field.value ?? ""}
                        onChange={(e) => field.onChange(e.target.value === "" ? null : Number(e.target.value))}
                        aria-invalid={!!form.formState.errors.creditLimit}
                      />
                    )}
                  />
                  {form.formState.errors.creditLimit && (
                    <p className="text-xs text-destructive">{form.formState.errors.creditLimit.message}</p>
                  )}
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="account-statement-day">Statement day</Label>
                    <Controller
                      control={form.control}
                      name="statementDay"
                      render={({ field }) => (
                        <Input
                          id="account-statement-day"
                          type="number"
                          min={1}
                          max={28}
                          value={field.value ?? ""}
                          onChange={(e) => field.onChange(e.target.value === "" ? null : Number(e.target.value))}
                          aria-invalid={!!form.formState.errors.statementDay}
                        />
                      )}
                    />
                    {form.formState.errors.statementDay && (
                      <p className="text-xs text-destructive">{form.formState.errors.statementDay.message}</p>
                    )}
                  </div>

                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="account-due-day">Due day</Label>
                    <Controller
                      control={form.control}
                      name="dueDay"
                      render={({ field }) => (
                        <Input
                          id="account-due-day"
                          type="number"
                          min={1}
                          max={28}
                          value={field.value ?? ""}
                          onChange={(e) => field.onChange(e.target.value === "" ? null : Number(e.target.value))}
                          aria-invalid={!!form.formState.errors.dueDay}
                        />
                      )}
                    />
                    {form.formState.errors.dueDay && (
                      <p className="text-xs text-destructive">{form.formState.errors.dueDay.message}</p>
                    )}
                  </div>
                </div>
              </>
            )}

            {formError && <p className="text-sm text-destructive">{formError}</p>}
          </div>

          <SheetFooter>
            <Button type="submit" disabled={isPending}>
              {isEdit ? "Save changes" : "Create account"}
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
