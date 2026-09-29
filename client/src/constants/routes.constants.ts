import {
  ArrowRightLeft,
  BarChart3,
  Home,
  Landmark,
  PiggyBank,
  Tags,
  type LucideIcon,
} from "lucide-react";

export interface NavItem {
  title: string;
  url: string;
  icon: LucideIcon;
}

export const navItems: NavItem[] = [
  { title: "Home", url: "/", icon: Home },
  { title: "Transactions", url: "/transactions", icon: ArrowRightLeft },
  { title: "Accounts", url: "/accounts", icon: Landmark },
  { title: "Categories", url: "/categories", icon: Tags },
  { title: "Reports", url: "/reports", icon: BarChart3 },
  { title: "Budgets", url: "/budgets", icon: PiggyBank },
];
