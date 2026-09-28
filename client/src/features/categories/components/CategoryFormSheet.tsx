import { useEffect, useState } from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetDescription,
  SheetFooter,
  SheetClose,
} from "@/ui/sheet";
import { Button } from "@/ui/button";
import { Input } from "@/ui/input";
import { Label } from "@/ui/label";
import { Select } from "@/ui/select";
import { ErrorStatus } from "@/types/errorStatus.enum";
import type { ApiError } from "@/types/apiError.types";
import { createCategorySchema, type CreateCategoryFormValues } from "../schemas/category.schema";
import { CategoryIconPicker } from "./CategoryIconPicker";
import { CategoryColorPicker } from "./CategoryColorPicker";
import { useCreateCategory } from "../hooks/mutations/useCreateCategory";
import { useUpdateCategory } from "../hooks/mutations/useUpdateCategory";
import { DEFAULT_CATEGORY_COLOR } from "../constants/categoryColors";
import type { Category } from "../types/category.types";

type TypeOption = { value: "Expense" | "Income"; label: string };
type ParentOption = { value: string | null; label: string };

const TYPE_OPTIONS: TypeOption[] = [
  { value: "Expense", label: "Expense" },
  { value: "Income", label: "Income" },
];

interface CategoryFormSheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  topLevelCategories: Category[];
  category?: Category;
  defaultParent?: Category;
}

export function CategoryFormSheet({
  open,
  onOpenChange,
  topLevelCategories,
  category,
  defaultParent,
}: CategoryFormSheetProps) {
  const isEdit = !!category;
  const parentLocked = isEdit || !!defaultParent;
  const lockedParent = category
    ? topLevelCategories.find((c) => c.id === category.parentId)
    : defaultParent;

  const [formError, setFormError] = useState<string | null>(null);
  const createMutation = useCreateCategory();
  const updateMutation = useUpdateCategory();
  const isPending = createMutation.isPending || updateMutation.isPending;

  const form = useForm<CreateCategoryFormValues>({
    resolver: zodResolver(createCategorySchema),
    defaultValues: {
      name: "",
      type: "Expense",
      parentId: null,
      color: DEFAULT_CATEGORY_COLOR,
      icon: "tag",
    },
  });

  useEffect(() => {
    if (!open) return;
    setFormError(null);

    if (category) {
      form.reset({
        name: category.name,
        type: category.type,
        parentId: category.parentId,
        color: category.color,
        icon: category.icon,
      });
    } else if (defaultParent) {
      form.reset({
        name: "",
        type: defaultParent.type,
        parentId: defaultParent.id,
        color: defaultParent.color,
        icon: "tag",
      });
    } else {
      form.reset({
        name: "",
        type: "Expense",
        parentId: null,
        color: DEFAULT_CATEGORY_COLOR,
        icon: "tag",
      });
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, category, defaultParent]);

  const type = form.watch("type");
  const parentId = form.watch("parentId");
  const color = form.watch("color");
  const icon = form.watch("icon");
  const typeLocked = isEdit || !!defaultParent || !!parentId;

  const parentOptions: ParentOption[] = [
    { value: null, label: "No parent (top-level)" },
    ...topLevelCategories
      .filter((c) => c.active && c.type === type)
      .map((c) => ({ value: c.id, label: c.name })),
  ];

  function applyServerError(error: unknown): void {
    const apiError = error as ApiError;

    if (apiError.errors) {
      for (const [key, messages] of Object.entries(apiError.errors)) {
        const field = (key.charAt(0).toLowerCase() + key.slice(1)) as keyof CreateCategoryFormValues;
        form.setError(field, { message: messages[0] });
      }
      return;
    }

    if (apiError.status === ErrorStatus.Duplicate) {
      form.setError("name", { message: apiError.message });
      return;
    }

    setFormError(apiError.message ?? "Something went wrong.");
  }

  function onSubmit(values: CreateCategoryFormValues) {
    setFormError(null);

    if (category) {
      updateMutation.mutate(
        { id: category.id, payload: { name: values.name, color: values.color, icon: values.icon } },
        { onSuccess: () => onOpenChange(false), onError: applyServerError },
      );
    } else {
      createMutation.mutate(values, {
        onSuccess: () => onOpenChange(false),
        onError: applyServerError,
      });
    }
  }

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="overflow-y-auto">
        <form onSubmit={form.handleSubmit(onSubmit)} className="flex flex-1 flex-col">
          <SheetHeader>
            <SheetTitle>{isEdit ? "Edit category" : "New category"}</SheetTitle>
            <SheetDescription>
              {lockedParent
                ? `Sub-category of ${lockedParent.name}`
                : "Top-level categories can have sub-categories."}
            </SheetDescription>
          </SheetHeader>

          <div className="flex flex-col gap-4 px-4">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="category-name">Name</Label>
              <Input
                id="category-name"
                autoFocus
                {...form.register("name")}
                aria-invalid={!!form.formState.errors.name}
              />
              {form.formState.errors.name && (
                <p className="text-xs text-destructive">{form.formState.errors.name.message}</p>
              )}
            </div>

            <div className="flex flex-col gap-1.5">
              <Label>Type</Label>
              {typeLocked ? (
                <p className="text-sm text-muted-foreground">{type}</p>
              ) : (
                <Controller
                  control={form.control}
                  name="type"
                  render={({ field }) => (
                    <Select<TypeOption>
                      value={TYPE_OPTIONS.find((o) => o.value === field.value)}
                      onChange={(option) => option && field.onChange(option.value)}
                      options={TYPE_OPTIONS}
                      isSearchable={false}
                    />
                  )}
                />
              )}
            </div>

            {!parentLocked && (
              <div className="flex flex-col gap-1.5">
                <Label>Parent</Label>
                <Controller
                  control={form.control}
                  name="parentId"
                  render={({ field }) => (
                    <Select<ParentOption>
                      value={parentOptions.find((o) => o.value === field.value) ?? parentOptions[0]}
                      onChange={(option) => field.onChange(option?.value ?? null)}
                      options={parentOptions}
                      isSearchable={false}
                    />
                  )}
                />
              </div>
            )}

            <div className="flex flex-col gap-1.5">
              <Label>Icon</Label>
              <CategoryIconPicker value={icon} color={color} onChange={(value) => form.setValue("icon", value)} />
            </div>

            <div className="flex flex-col gap-1.5">
              <Label>Color</Label>
              <CategoryColorPicker value={color} onChange={(value) => form.setValue("color", value)} />
              {form.formState.errors.color && (
                <p className="text-xs text-destructive">{form.formState.errors.color.message}</p>
              )}
            </div>

            {formError && <p className="text-sm text-destructive">{formError}</p>}
          </div>

          <SheetFooter>
            <Button type="submit" disabled={isPending}>
              {isEdit ? "Save changes" : "Create category"}
            </Button>
            <SheetClose asChild>
              <Button type="button" variant="outline">
                Cancel
              </Button>
            </SheetClose>
          </SheetFooter>
        </form>
      </SheetContent>
    </Sheet>
  );
}
