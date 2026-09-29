import { useState } from "react";
import { Plus, RefreshCw, Landmark } from "lucide-react";
import { Button } from "@/ui/button";
import { Switch } from "@/ui/switch";
import { Label } from "@/ui/label";
import { Skeleton } from "@/ui/skeleton";
import { PageBreadcrumb } from "@/components/layout/PageBreadcrumb";
import { useAccounts } from "../hooks/queries/useAccounts";
import { useArchiveAccount } from "../hooks/mutations/useArchiveAccount";
import { useRestoreAccount } from "../hooks/mutations/useRestoreAccount";
import { useReorderAccounts } from "../hooks/mutations/useReorderAccounts";
import { AccountSummaryStrip } from "../components/AccountSummaryStrip";
import { AccountSection } from "../components/AccountSection";
import { AccountRow } from "../components/AccountRow";
import { CreditCardRow } from "../components/CreditCardRow";
import { AccountFormSheet } from "../components/AccountFormSheet";
import { PayCardSheet } from "../components/PayCardSheet";
import type { Account } from "../types/account.types";

export function AccountsPage() {
  const { data: accounts, isLoading, isError, refetch } = useAccounts();
  const archiveMutation = useArchiveAccount();
  const restoreMutation = useRestoreAccount();
  const reorderMutation = useReorderAccounts();

  const [showArchived, setShowArchived] = useState(false);
  const [formOpen, setFormOpen] = useState(false);
  const [editingAccount, setEditingAccount] = useState<Account | undefined>();
  const [payOpen, setPayOpen] = useState(false);
  const [payingCard, setPayingCard] = useState<Account | undefined>();

  const all = accounts ?? [];

  const bankCashWallet = all
    .filter((a) => a.type !== "CreditCard" && (showArchived || a.active))
    .sort((a, b) => a.sortOrder - b.sortOrder);

  const creditCards = all
    .filter((a) => a.type === "CreditCard" && (showArchived || a.active))
    .sort((a, b) => (a.nextDueDate ?? "9999").localeCompare(b.nextDueDate ?? "9999"));

  const activeAccounts = all.filter((a) => a.active);
  const totals = {
    totalBalance: activeAccounts
      .filter((a) => a.type !== "CreditCard")
      .reduce((sum, a) => sum + (a.balance ?? 0), 0),
    totalOwed: activeAccounts
      .filter((a) => a.type === "CreditCard")
      .reduce((sum, a) => sum + (a.outstanding ?? 0), 0),
    availableCredit: activeAccounts
      .filter((a) => a.type === "CreditCard")
      .reduce((sum, a) => sum + (a.availableCredit ?? 0), 0),
  };

  const sourceAccounts = all.filter((a) => a.type !== "CreditCard" && a.active);

  function openCreate() {
    setEditingAccount(undefined);
    setFormOpen(true);
  }

  function openEdit(account: Account) {
    setEditingAccount(account);
    setFormOpen(true);
  }

  function openPay(account: Account) {
    setPayingCard(account);
    setPayOpen(true);
  }

  function moveAccount(account: Account, direction: -1 | 1) {
    const ids = bankCashWallet.map((a) => a.id);
    const index = ids.indexOf(account.id);
    const swapWith = index + direction;
    if (swapWith < 0 || swapWith >= ids.length) return;

    [ids[index], ids[swapWith]] = [ids[swapWith], ids[index]];
    reorderMutation.mutate(ids);
  }

  const isEmpty = !isLoading && !isError && all.length === 0;

  return (
    <div className="flex w-full flex-col gap-5 p-4 md:p-6">
      <PageBreadcrumb items={["Accounts"]} />
      <div className="flex items-start justify-between gap-3">
        <div className="space-y-1">
          <h1 className="font-heading text-xl font-semibold text-foreground">Accounts</h1>
          <p className="text-sm text-muted-foreground">
            Track balances across your banks, cash, wallets and credit cards.
          </p>
        </div>
        <Button onClick={openCreate}>
          <Plus /> Add account
        </Button>
      </div>

      {!isEmpty && <AccountSummaryStrip {...totals} />}

      {!isEmpty && (
        <div className="flex justify-end">
          <Label className="flex items-center gap-2 text-sm text-muted-foreground">
            <Switch checked={showArchived} onCheckedChange={setShowArchived} />
            Show archived
          </Label>
        </div>
      )}

      {isLoading && (
        <div className="flex flex-col gap-2">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="flex items-center gap-3 rounded-xl border border-border px-3 py-2.5">
              <Skeleton className="size-8 shrink-0 rounded-lg" />
              <Skeleton className="h-4 w-32" />
            </div>
          ))}
        </div>
      )}

      {isError && (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-border px-3 py-10 text-center">
          <p className="text-sm text-destructive">Couldn&apos;t load accounts.</p>
          <Button variant="outline" size="sm" onClick={() => refetch()}>
            <RefreshCw /> Try again
          </Button>
        </div>
      )}

      {isEmpty && (
        <div className="flex flex-col items-center gap-3 rounded-xl border border-border px-3 py-16 text-center">
          <Landmark className="size-8 text-muted-foreground/50" />
          <p className="text-sm text-muted-foreground">No accounts yet. Add your first account to get started.</p>
          <Button onClick={openCreate}>
            <Plus /> Create account
          </Button>
        </div>
      )}

      {!isLoading && !isError && !isEmpty && (
        <div className="grid gap-5 lg:grid-cols-2 lg:items-start">
          <AccountSection title="Bank and cash" emptyMessage="No bank or cash accounts yet." isEmpty={bankCashWallet.length === 0}>
            {bankCashWallet.map((account, index) => (
              <AccountRow
                key={account.id}
                account={account}
                onEdit={openEdit}
                onArchive={(a) => archiveMutation.mutate(a.id)}
                onRestore={(a) => restoreMutation.mutate(a.id)}
                onMoveUp={(a) => moveAccount(a, -1)}
                onMoveDown={(a) => moveAccount(a, 1)}
                canMoveUp={index > 0}
                canMoveDown={index < bankCashWallet.length - 1}
              />
            ))}
          </AccountSection>

          <AccountSection title="Credit cards" emptyMessage="No credit cards yet." isEmpty={creditCards.length === 0}>
            {creditCards.map((account) => (
              <CreditCardRow
                key={account.id}
                account={account}
                onEdit={openEdit}
                onArchive={(a) => archiveMutation.mutate(a.id)}
                onRestore={(a) => restoreMutation.mutate(a.id)}
                onPay={openPay}
              />
            ))}
          </AccountSection>
        </div>
      )}

      <AccountFormSheet open={formOpen} onOpenChange={setFormOpen} account={editingAccount} />
      <PayCardSheet open={payOpen} onOpenChange={setPayOpen} card={payingCard} sourceAccounts={sourceAccounts} />
    </div>
  );
}
