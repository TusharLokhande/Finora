import { useState } from "react";
import { formatDistanceToNow } from "date-fns";
import { Button } from "@/ui/button";
import { Skeleton } from "@/ui/skeleton";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/ui/table";
import { useAuditLog } from "../hooks/queries/useMembers";
import type { AuditAction } from "../types/access.types";

const ACTION_LABELS: Record<AuditAction, string> = {
  Approve: "Approved",
  Reject: "Rejected",
  Suspend: "Suspended",
  Reactivate: "Reactivated",
  DeleteUser: "Deleted",
  ToggleSignups: "Changed new requests",
};

export function AuditLogTab() {
  const [page, setPage] = useState(1);
  const { data, isLoading, isError } = useAuditLog(page);

  if (isLoading) return <Skeleton className="h-40 w-full" />;
  if (isError || !data) return <p className="text-sm text-destructive">Couldn&apos;t load the audit log.</p>;
  if (data.items.length === 0) return <p className="py-8 text-center text-sm text-muted-foreground">No actions recorded yet.</p>;

  return (
    <div className="flex flex-col gap-3">
      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Action</TableHead>
            <TableHead>Member</TableHead>
            <TableHead>By</TableHead>
            <TableHead>Note</TableHead>
            <TableHead>When</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {data.items.map((entry) => (
            <TableRow key={entry.id}>
              <TableCell className="font-medium">{ACTION_LABELS[entry.action]}</TableCell>
              <TableCell>{entry.targetEmail ?? "—"}</TableCell>
              <TableCell className="text-muted-foreground">{entry.adminEmail}</TableCell>
              <TableCell className="text-muted-foreground">{entry.reason ?? "—"}</TableCell>
              <TableCell className="text-muted-foreground">
                {formatDistanceToNow(new Date(entry.createdAtUtc), { addSuffix: true })}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
      <div className="flex items-center justify-end gap-2 text-sm text-muted-foreground">
        <span>Page {data.page} of {Math.max(data.totalPages, 1)}</span>
        <Button size="sm" variant="outline" disabled={page <= 1} onClick={() => setPage(page - 1)}>Previous</Button>
        <Button size="sm" variant="outline" disabled={page >= data.totalPages} onClick={() => setPage(page + 1)}>Next</Button>
      </div>
    </div>
  );
}
