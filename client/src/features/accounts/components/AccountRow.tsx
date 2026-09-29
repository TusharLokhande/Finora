import { MoreVertical, Pencil, Archive, ArchiveRestore, ArrowRightLeft, ArrowUp, ArrowDown } from "lucide-react";
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
import { ACCOUNT_TYPE_COLORS, ACCOUNT_TYPE_ICONS, ACCOUNT_TYPE_LABELS } from "../constants/accountTypes";
import type { Account } from "../types/account.types";

interface AccountRowProps {
  account: Account;
  onEdit: (account: Account) => void;
  onArchive: (account: Account) => void;
  onRestore: (account: Account) => void;
  onMoveUp?: (account: Account) => void;
  onMoveDown?: (account: Account) => void;
  canMoveUp?: boolean;
  canMoveDown?: boolean;
}

export function AccountRow({
  account,
  onEdit,
  onArchive,
  onRestore,
  onMoveUp,
  onMoveDown,
  canMoveUp,
  canMoveDown,
}: AccountRowProps) {
  const navigate = useNavigate();
  const Icon = ACCOUNT_TYPE_ICONS[account.type];
  const color = account.color ?? ACCOUNT_TYPE_COLORS[account.type];

  return (
    <div
      className={cn(
        "flex cursor-pointer items-center gap-3 px-3 py-2.5 hover:bg-muted/60",
        !account.active && "opacity-50",
      )}
      onClick={() => navigate(`/transactions?accountId=${account.id}`)}
    >
      <span
        className="flex size-8 shrink-0 items-center justify-center rounded-lg"
        style={{ backgroundColor: `${color}20`, color }}
      >
        <Icon className="size-4" />
      </span>

      <div className="min-w-0 flex-1">
        <p className="truncate text-sm font-medium text-foreground">{account.name}</p>
        <p className="mt-1 text-xs text-muted-foreground">{ACCOUNT_TYPE_LABELS[account.type]}</p>
      </div>

      {!account.active && (
        <span className="shrink-0 rounded-full bg-muted px-2 py-0.5 text-xs text-muted-foreground">Archived</span>
      )}

      <span className="shrink-0 font-mono text-sm tabular-nums text-foreground">
        {formatCurrency(account.balance ?? 0)}
      </span>

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
          {(onMoveUp || onMoveDown) && (
            <>
              <DropdownMenuItem disabled={!canMoveUp} onSelect={() => onMoveUp?.(account)}>
                <ArrowUp /> Move up
              </DropdownMenuItem>
              <DropdownMenuItem disabled={!canMoveDown} onSelect={() => onMoveDown?.(account)}>
                <ArrowDown /> Move down
              </DropdownMenuItem>
            </>
          )}
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
  );
}
