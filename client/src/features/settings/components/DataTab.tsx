import { useState } from "react";
import { Download, Trash2 } from "lucide-react";
import { Button } from "@/ui/button";
import { useExportData } from "../hooks/mutations/useExportData";
import { DeleteAccountDialog } from "./DeleteAccountDialog";

export function DataTab() {
  const exportData = useExportData();
  const [deleteOpen, setDeleteOpen] = useState(false);

  return (
    <div className="flex max-w-xl flex-col gap-6">
      <section className="flex flex-col gap-3 rounded-xl border border-border p-4">
        <div>
          <h2 className="text-sm font-medium text-foreground">Export your data</h2>
          <p className="mt-0.5 text-sm text-muted-foreground">
            Download every account, category, transaction and budget as a single JSON file.
          </p>
        </div>
        <Button variant="outline" className="w-fit" disabled={exportData.isPending} onClick={() => exportData.mutate()}>
          <Download /> {exportData.isPending ? "Preparing…" : "Export data"}
        </Button>
      </section>

      <section className="flex flex-col gap-3 rounded-xl border border-destructive/40 bg-destructive/5 p-4">
        <div>
          <h2 className="text-sm font-medium text-destructive">Delete account</h2>
          <p className="mt-0.5 text-sm text-muted-foreground">
            Permanently remove your account and everything in it. This can&apos;t be undone.
          </p>
        </div>
        <Button variant="destructive" className="w-fit" onClick={() => setDeleteOpen(true)}>
          <Trash2 /> Delete account
        </Button>
      </section>

      <DeleteAccountDialog open={deleteOpen} onOpenChange={setDeleteOpen} />
    </div>
  );
}
