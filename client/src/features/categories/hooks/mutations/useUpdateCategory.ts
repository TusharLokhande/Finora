import { useMutation, useQueryClient } from "@tanstack/react-query";
import { updateCategory } from "../../api/categories.api";
import { categoryKeys } from "../queries/useCategories";
import type { UpdateCategoryInput } from "../../types/category.types";

export function useUpdateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateCategoryInput }) => updateCategory(id, payload),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.all });
    },
  });
}
