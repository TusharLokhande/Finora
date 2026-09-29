import { differenceInCalendarDays, format, parseISO } from "date-fns";
import { MoreVertical, Pencil, Archive, ArchiveRestore, ArrowRightLeft, CreditCard as CreditCardIcon } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/ui/button";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
} from "@/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import { formatCurrency } from "@/lib/currency";
import { ACCOUNT_TYPE_COLORS } from "../constants/accountTypes";
import type { Account } from "../types/account.types";

type Urgency = "neutral" | "amber" | "red";

function getDueUrgency(nextDueDate: string | null): { urgency: Urgency; label: string } | null {
  if (!nextDueDate) return null;

  const today = new Date();
  today.setHours(0, 0, 0, 0);
  const daysUntil = differenceInCalendarDays(parseISO(nextDueDate), today);

  if (daysUntil < 0) return { urgency: "red", label: `Overdue ${format(parseISO(nextDueDate), "d MMM")}` };
  if (daysUntil === 0) return { urgency: "red", label: "Due today" };
  if (daysUntil <= 2) return { urgency: "red", label: `Due in ${daysUntil}d` };
  if (daysUntil <= 7) return { urgency: "amber", label: `Due in ${daysUntil}d` };
  return { urgency: "neutral", label: `Due ${format(parseISO(nextDueDate), "d MMM")}` };
}

const CHIP_STYLES: Record<Urgency, string> = {
  neutral: "bg-muted text-muted-foreground",
  amber: "bg-amber-500/15 text-amber-600 dark:text-amber-400",
  red: "bg-destructive/10 text-destructive",
};

const BAR_STYLES: Record<Urgency, string> = {
  neutral: "bg-primary",
  amber: "bg-amber-500",
  red: "bg-destructive",
};

interface CreditCardRowProps {
  account: Account;
  onEdit: (account: Account) => void;
  onArchive: (account: Account) => void;
  onRestore: (account: Account) => void;
  onPay: (account: Account) => void;
}

export function CreditCardRow({ account, onEdit, onArchive, onRestore, onPay }: CreditCardRowProps) {
  const navigate = useNavigate();
  const outstanding = account.outstanding ?? 0;
  const limit = account.creditLimit ?? 0;
  const utilization = limit > 0 ? Math.min(100, Math.max(0, (outstanding / limit) * 100)) : 0;
  const due = getDueUrgency(account.nextDueDate);
  const color = account.color ?? ACCOUNT_TYPE_COLORS.CreditCard;

  return (
    <div
      className={cn(
        "flex cursor-pointer flex-col gap-2 px-3 py-3 hover:bg-muted/60",
        !account.active && "opacity-50",
      )}
      onClick={() => navigate(`/transactions?accountId=${account.id}`)}
    >
      <div className="flex items-center gap-3">
        <span
          className="flex size-8 shrink-0 items-center justify-center rounded-lg"
          style={{ backgroundColor: `${color}20`, color }}
        >
          <CreditCardIcon className="size-4" />
        </span>

        <div className="min-w-0 flex-1">
          <p className="truncate text-sm font-medium text-foreground">{account.name}</p>
          <p className="mt-1 font-mono text-xs tabular-nums text-muted-foreground">
            {formatCurrency(outstanding)} of {formatCurrency(limit)}
          </p>
        </div>

        {!account.active && (
          <span className="shrink-0 rounded-full bg-muted px-2 py-0.5 text-xs text-muted-foreground">Archived</span>
        )}

        {due && (
          <span className={cn("shrink-0 rounded-full px-2 py-0.5 text-xs font-medium", CHIP_STYLES[due.urgency])}>
            {due.label}
          </span>
        )}

        {account.active && (
          <Button size="sm" variant="secondary" onClick={(e) => { e.stopPropagation(); onPay(account); }}>
            Pay
          </Button>
        )}

        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon-sm" onClick={(e) => e.stopPropagation()}>
              <MoreVertical className="size-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" onClick={(e) => e.stopPropagation()}>
            <DropdownMenuItem onSelect={() => onEdit(account)}>
              <Pencil /> Edit
            </DropdownMenuItem>
            <DropdownMenuItem onSelect={() => navigate(`/transactions?accountId=${account.id}`)}>
              <ArrowRightLeft /> View transactions
            </DropdownMenuItem>
            <DropdownMenuSeparator />
            {account.active ? (
              <DropdownMenuItem variant="destructive" onSelect={() => onArchive(account)}>
                <Archive /> Archive
              </DropdownMenuItem>
            ) : (
              <DropdownMenuItem onSelect={() => onRestore(account)}>
                <ArchiveRestore /> Restore
              </DropdownMenuItem>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      <div className="flex items-center gap-2 pl-11">
        <div className="h-1.5 flex-1 overflow-hidden rounded-full bg-muted">
          <div
            className={cn("h-full rounded-full transition-all", BAR_STYLES[due?.urgency ?? "neutral"])}
            style={{ width: `${utilization}%` }}
          />
        </div>
        <span className="w-12 shrink-0 text-right text-xs tabular-nums text-muted-foreground">
          {Math.round(utilization)}% used
        </span>
      </div>
    </div>
  );
}
