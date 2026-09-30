import { useMemo } from "react";
import { useCategories } from "@/features/categories";
import type { BudgetLine } from "../../types/budget.types";

export interface UnbudgetedOption {
  value: string;
  label: string;
  /** What the row looks like before the first save; spend is filled in by the server afterwards. */
  stub: BudgetLine;
}

/** Active expense categories (and sub-categories) with no budget this month, in category order. */
export function useUnbudgetedCategories(lines: BudgetLine[]) {
  const { data: categories } = useCategories();

  return useMemo(() => {
    const byId = new Map(lines.map((l) => [l.categoryId, l]));
    const options: UnbudgetedOption[] = [];

    const roots = (categories ?? [])
      .filter((c) => c.type === "Expense" && c.active)
      .sort((a, b) => a.sortOrder - b.sortOrder);

    for (const root of roots) {
      const rootLine = byId.get(root.id);
      if (!rootLine || rootLine.budgeted === null) {
        options.push({ value: root.id, label: root.name, stub: rootLine ?? stubFor(root, null) });
      }
      for (const child of [...root.children].filter((c) => c.active).sort((a, b) => a.sortOrder - b.sortOrder)) {
        const childLine = byId.get(child.id);
        if (childLine?.budgeted != null) continue;
        options.push({ value: child.id, label: `${root.name} › ${child.name}`, stub: childLine ?? stubFor(child, root) });
      }
    }

    return options;
  }, [categories, lines]);
}

interface CategoryLike {
  id: string;
  name: string;
  icon: string | null;
  color: string | null;
  sortOrder: number;
}

function stubFor(c: CategoryLike, parent: CategoryLike | null): BudgetLine {
  return {
    categoryId: c.id,
    parentId: parent?.id ?? null,
    categoryName: c.name,
    parentCategoryName: parent?.name ?? null,
    icon: c.icon ?? parent?.icon ?? null,
    color: c.color ?? parent?.color ?? null,
    sortOrder: c.sortOrder,
    active: true,
    budgeted: null,
    spent: 0,
    percentUsed: null,
    status: "Normal",
  };
}
