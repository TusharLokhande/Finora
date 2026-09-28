import { useMutation, useQueryClient } from "@tanstack/react-query";
import { createCategory } from "../../api/categories.api";
import { categoryKeys } from "../queries/useCategories";

export function useCreateCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: createCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.all });
    },
  });
}
