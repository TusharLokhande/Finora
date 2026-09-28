import { useState } from "react";
import { ChevronRight, Plus, Tags } from "lucide-react";
import { Button } from "@/ui/button";
import { cn } from "@/lib/utils";
import { CategoryRow } from "./CategoryRow";
import type { Category, CategoryType } from "../types/category.types";

interface CategoryListProps {
  categories: Category[];
  categoryType: CategoryType;
  showArchived: boolean;
  onEdit: (category: Category) => void;
  onAddChild: (category: Category) => void;
  onArchive: (category: Category) => void;
  onRestore: (category: Category) => void;
  onAddCategory: () => void;
}

export function CategoryList({
  categories,
  categoryType,
  showArchived,
  onEdit,
  onAddChild,
  onArchive,
  onRestore,
  onAddCategory,
}: CategoryListProps) {
  const [collapsed, setCollapsed] = useState<Record<string, boolean>>({});
  const [archivedOpen, setArchivedOpen] = useState(false);

  const activeTop = categories.filter((c) => c.active);
  const archivedTop = showArchived ? categories.filter((c) => !c.active) : [];

  if (activeTop.length === 0 && archivedTop.length === 0) {
    return (
      <div className="flex flex-col items-center gap-3 px-2 py-10 text-center">
        <Tags className="size-8 text-muted-foreground/50" />
        <p className="text-sm text-muted-foreground">No {categoryType.toLowerCase()} categories yet.</p>
        <Button variant="outline" size="sm" onClick={onAddCategory}>
          <Plus /> Add category
        </Button>
      </div>
    );
  }

  function renderTopLevel(category: Category) {
    const children = category.children.filter((c) => showArchived || c.active);
    const isExpanded = !collapsed[category.id];

    return (
      <div key={category.id}>
        <CategoryRow
          category={category}
          childCount={children.length}
          expanded={isExpanded}
          onToggleExpand={() => setCollapsed((prev) => ({ ...prev, [category.id]: isExpanded }))}
          onEdit={onEdit}
          onAddChild={onAddChild}
          onArchive={onArchive}
          onRestore={onRestore}
        />
        {isExpanded &&
          children.map((child) => (
            <CategoryRow
              key={child.id}
              category={child}
              isChild
              parentColor={category.color}
              onEdit={onEdit}
              onArchive={onArchive}
              onRestore={onRestore}
            />
          ))}
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-0.5">
      {activeTop.length === 0 ? (
        <p className="px-2 py-6 text-center text-sm text-muted-foreground">
          All your {categoryType.toLowerCase()} categories are archived.
        </p>
      ) : (
        activeTop.map(renderTopLevel)
      )}

      {archivedTop.length > 0 && (
        <div className="mt-1 border-t border-border pt-1">
          <button
            type="button"
            onClick={() => setArchivedOpen((prev) => !prev)}
            className="flex w-full items-center gap-2 rounded-lg px-2 py-2 text-sm text-muted-foreground hover:bg-muted/60"
          >
            <ChevronRight className={cn("size-4 transition-transform", archivedOpen && "rotate-90")} />
            Archived
            <span className="tabular-nums">({archivedTop.length})</span>
          </button>
          {archivedOpen && archivedTop.map(renderTopLevel)}
        </div>
      )}
    </div>
  );
}
