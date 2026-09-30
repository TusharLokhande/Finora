import { useMutation } from "@tanstack/react-query";
import { toast } from "sonner";
import type { ApiError } from "@/types/apiError.types";
import { exportReport } from "../../api/reports.api";
import type { ReportRangeParams } from "../../types/report.types";

export function useExportReport() {
  return useMutation<void, ApiError, ReportRangeParams>({
    mutationFn: exportReport,
    onError: (error) => toast.error(error.message ?? "Couldn't export the report."),
  });
}
