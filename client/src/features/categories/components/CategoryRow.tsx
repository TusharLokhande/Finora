import { ChevronRight, MoreVertical, Pencil, Archive, ArchiveRestore, Plus } from "lucide-react";
import { Button } from "@/ui/button";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
} from "@/ui/dropdown-menu";
import { cn } from "@/lib/utils";
import { getCategoryIcon } from "../constants/categoryIcons";
import type { Category } from "../types/category.types";

interface CategoryRowProps {
  category: Category;
  isChild?: boolean;
  parentColor?: string | null;
  childCount?: number;
  expanded?: boolean;
  onToggleExpand?: () => void;
  onEdit: (category: Category) => void;
  onAddChild?: (category: Category) => void;
  onArchive: (category: Category) => void;
  onRestore: (category: Category) => void;
}

export function CategoryRow({
  category,
  isChild,
  parentColor,
  childCount = 0,
  expanded,
  onToggleExpand,
  onEdit,
  onAddChild,
  onArchive,
  onRestore,
}: CategoryRowProps) {
  const Icon = getCategoryIcon(category.icon);
  const color = category.color ?? parentColor ?? "#64748b";
  const hasChildren = childCount > 0;

  return (
    <div
      className={cn(
        "flex items-center gap-2 rounded-lg px-2 py-2 hover:bg-muted/60",
        isChild && "ml-9",
        !category.active && "opacity-50",
      )}
    >
      {!isChild && hasChildren ? (
        <button
          type="button"
          onClick={onToggleExpand}
          className="flex size-6 shrink-0 items-center justify-center rounded-md text-muted-foreground hover:bg-muted"
        >
          <ChevronRight className={cn("size-4 transition-transform", expanded && "rotate-90")} />
        </button>
      ) : (
        !isChild && <span className="size-6 shrink-0" />
      )}

      <span
        className={cn(
          "flex shrink-0 items-center justify-center rounded-lg",
          isChild ? "size-7" : "size-8",
        )}
        style={{ backgroundColor: `${color}20`, color }}
      >
        <Icon className={cn(isChild ? "size-3.5" : "size-4")} />
      </span>

      <span className="flex-1 truncate text-sm font-medium text-foreground">{category.name}</span>

      {!isChild && hasChildren && !expanded && (
        <span className="shrink-0 rounded-full bg-muted px-1.5 py-0.5 text-[11px] tabular-nums text-muted-foreground">
          {childCount}
        </span>
      )}

      {!category.active && (
        <span className="rounded-full bg-muted px-2 py-0.5 text-xs text-muted-foreground">Archived</span>
      )}

      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" size="icon-sm">
            <MoreVertical className="size-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end">
          <DropdownMenuItem onSelect={() => onEdit(category)}>
            <Pencil /> Edit
          </DropdownMenuItem>
          {onAddChild && (
            <DropdownMenuItem onSelect={() => onAddChild(category)}>
              <Plus /> Add sub-category
            </DropdownMenuItem>
          )}
          <DropdownMenuSeparator />
          {category.active ? (
            <DropdownMenuItem variant="destructive" onSelect={() => onArchive(category)}>
              <Archive /> Archive
            </DropdownMenuItem>
          ) : (
            <DropdownMenuItem onSelect={() => onRestore(category)}>
              <ArchiveRestore /> Restore
            </DropdownMenuItem>
          )}
        </DropdownMenuContent>
      </DropdownMenu>
    </div>
  );
}
