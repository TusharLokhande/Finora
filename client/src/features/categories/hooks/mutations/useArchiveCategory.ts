import { useMutation, useQueryClient } from "@tanstack/react-query";
import { archiveCategory } from "../../api/categories.api";
import { categoryKeys } from "../queries/useCategories";

export function useArchiveCategory() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: archiveCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: categoryKeys.all });
    },
  });
}
