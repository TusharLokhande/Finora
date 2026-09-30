import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { exportData } from "../../api/settings.api";

export function useExportData() {
  return useMutation<void, ApiError>({
    mutationFn: exportData,
    onError: (error) => toast.error(error.message ?? "Couldn't export your data."),
  });
}
