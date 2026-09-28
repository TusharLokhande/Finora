import { useMutation, useQueryClient } from "@tanstack/react-query";
import { restoreCategory } from "../../api/categories.api";
import { categoryKeys } from "../queries/useCategories";

export function useRestoreCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: restoreCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.all });
    },
  });
}
