import { Landmark, Banknote, Wallet, CreditCard as CreditCardIcon, type LucideIcon } from "lucide-react";
import type { AccountType } from "../types/account.types";

export const ACCOUNT_TYPE_LABELS: Record<AccountType, string> = {
  Bank: "Bank",
  Cash: "Cash",
  Wallet: "Wallet",
  CreditCard: "Credit Card",
};

export const ACCOUNT_TYPE_ICONS: Record<AccountType, LucideIcon> = {
  Bank: Landmark,
  Cash: Banknote,
  Wallet: Wallet,
  CreditCard: CreditCardIcon,
};

/** Default row/chip color per type, used whenever an account has no custom color set. */
export const ACCOUNT_TYPE_COLORS: Record<AccountType, string> = {
  Bank: "#3b82f6",
  Cash: "#22c55e",
  Wallet: "#8b5cf6",
  CreditCard: "#ec4899",
};

export const ACCOUNT_TYPE_OPTIONS: AccountType[] = ["Bank", "Cash", "Wallet", "CreditCard"];
