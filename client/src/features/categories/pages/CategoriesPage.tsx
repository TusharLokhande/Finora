import { useState } from "react";
import { Plus, RefreshCw } from "lucide-react";
import { Button } from "@/ui/button";
import { Tabs, TabsList, TabsTrigger } from "@/ui/tabs";
import { Checkbox } from "@/ui/checkbox";
import { Skeleton } from "@/ui/skeleton";
import { PageBreadcrumb } from "@/components/layout/PageBreadcrumb";
import { useCategories } from "../hooks/queries/useCategories";
import { useArchiveCategory } from "../hooks/mutations/useArchiveCategory";
import { useRestoreCategory } from "../hooks/mutations/useRestoreCategory";
import { CategoryList } from "../components/CategoryList";
import { CategoryFormSheet } from "../components/CategoryFormSheet";
import type { Category, CategoryType } from "../types/category.types";

export function CategoriesPage() {
  const { data: categories, isLoading, isError, refetch } = useCategories();
  const archiveMutation = useArchiveCategory();
  const restoreMutation = useRestoreCategory();

  const [activeTab, setActiveTab] = useState<CategoryType>("Expense");
  const [showArchived, setShowArchived] = useState(false);
  const [sheetOpen, setSheetOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | undefined>();
  const [defaultParent, setDefaultParent] = useState<Category | undefined>();

  const allTopLevel = categories ?? [];
  const filtered = allTopLevel.filter((c) => c.type === activeTab);
  const expenseCount = allTopLevel.filter((c) => c.type === "Expense" && (showArchived || c.active)).length;
  const incomeCount = allTopLevel.filter((c) => c.type === "Income" && (showArchived || c.active)).length;

  function openCreate() {
    setEditingCategory(undefined);
    setDefaultParent(undefined);
    setSheetOpen(true);
  }

  function openAddChild(parent: Category) {
    setEditingCategory(undefined);
    setDefaultParent(parent);
    setSheetOpen(true);
  }

  function openEdit(category: Category) {
    setEditingCategory(category);
    setDefaultParent(undefined);
    setSheetOpen(true);
  }

  return (
    <div className="flex w-full flex-col gap-5 p-4 md:p-6">
      <PageBreadcrumb items={["Categories"]} />
      <div className="flex items-start justify-between gap-3">
        <div className="space-y-1">
          <h1 className="font-heading text-xl font-semibold text-foreground">Categories</h1>
          <p className="text-sm text-muted-foreground">
            Organize your income and expenses. Archiving keeps a category&apos;s name on old transactions.
          </p>
        </div>
        <Button onClick={openCreate}>
          <Plus /> Add category
        </Button>
      </div>

      <div className="flex flex-wrap items-center justify-between gap-3">
        <Tabs value={activeTab} onValueChange={(value) => setActiveTab(value as CategoryType)}>
          <TabsList>
            <TabsTrigger value="Expense">
              Expense <span className="tabular-nums opacity-60">{expenseCount}</span>
            </TabsTrigger>
            <TabsTrigger value="Income">
              Income <span className="tabular-nums opacity-60">{incomeCount}</span>
            </TabsTrigger>
          </TabsList>
        </Tabs>

        <label className="flex items-center gap-2 text-sm text-muted-foreground">
          <Checkbox
            checked={showArchived}
            onCheckedChange={(checked) => setShowArchived(checked === true)}
          />
          Show archived
        </label>
      </div>

      <div className="rounded-xl border border-border bg-card p-2">
        {isLoading && (
          <div className="flex flex-col gap-1 p-1">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className="flex items-center gap-2 px-2 py-2">
                <Skeleton className="size-8 shrink-0 rounded-lg" />
                <Skeleton className="h-4 w-32" />
              </div>
            ))}
          </div>
        )}

        {isError && (
          <div className="flex flex-col items-center gap-3 px-2 py-10 text-center">
            <p className="text-sm text-destructive">Couldn&apos;t load categories.</p>
            <Button variant="outline" size="sm" onClick={() => refetch()}>
              <RefreshCw /> Try again
            </Button>
          </div>
        )}

        {!isLoading && !isError && (
          <CategoryList
            categories={filtered}
            categoryType={activeTab}
            showArchived={showArchived}
            onEdit={openEdit}
            onAddChild={openAddChild}
            onArchive={(category) => archiveMutation.mutate(category.id)}
            onRestore={(category) => restoreMutation.mutate(category.id)}
            onAddCategory={openCreate}
          />
        )}
      </div>

      <CategoryFormSheet
        open={sheetOpen}
        onOpenChange={setSheetOpen}
        topLevelCategories={allTopLevel}
        category={editingCategory}
        defaultParent={defaultParent}
      />
    </div>
  );
}
