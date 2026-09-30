import { useMemo } from "react";
import { useAccounts } from "@/features/accounts";
import { useCategories } from "@/features/categories";
import type { TransactionType } from "../../types/transaction.types";

export interface LookupOption {
  value: string;
  label: string;
}

export interface CategoryOption extends LookupOption {
  type: "Income" | "Expense";
  active: boolean;
  /** Own name, without the parent prefix. */
  name: string;
  parentId: string | null;
  parentName: string | null;
  /** Own icon/color, falling back to the parent's. */
  icon: string | null;
  color: string | null;
}

/** Account and category select options, flattened from the accounts list and category tree. */
export function useLookupOptions() {
  const { data: accounts } = useAccounts();
  const { data: categories } = useCategories();

  return useMemo(() => {
    const sortedAccounts = [...(accounts ?? [])].sort((a, b) => a.sortOrder - b.sortOrder);
    const allAccounts: LookupOption[] = sortedAccounts.map((a) => ({ value: a.id, label: a.name }));
    const activeAccounts: LookupOption[] = sortedAccounts
      .filter((a) => a.active)
      .map((a) => ({ value: a.id, label: a.name }));

    const allCategories: CategoryOption[] = [];
    for (const root of [...(categories ?? [])].sort((a, b) => a.sortOrder - b.sortOrder)) {
      allCategories.push({
        value: root.id,
        label: root.name,
        name: root.name,
        type: root.type,
        active: root.active,
        parentId: null,
        parentName: null,
        icon: root.icon,
        color: root.color,
      });
      for (const child of [...root.children].sort((a, b) => a.sortOrder - b.sortOrder)) {
        allCategories.push({
          value: child.id,
          label: `${root.name} › ${child.name}`,
          name: child.name,
          type: child.type,
          active: root.active && child.active,
          parentId: root.id,
          parentName: root.name,
          icon: child.icon ?? root.icon,
          color: child.color ?? root.color,
        });
      }
    }

    const categoryById = new Map(allCategories.map((c) => [c.value, c]));
    const accountById = new Map(allAccounts.map((a) => [a.value, a]));

    const categoriesFor = (type: TransactionType) =>
      allCategories.filter((c) => c.active && c.type === type);

    return { allAccounts, activeAccounts, allCategories, categoryById, accountById, categoriesFor };
  }, [accounts, categories]);
}
