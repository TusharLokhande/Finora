import { useState } from "react";
import { formatDistanceToNow } from "date-fns";
import { EllipsisVertical } from "lucide-react";
import { Button } from "@/ui/button";
import { Input } from "@/ui/input";
import { TableCell, TableRow } from "@/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/ui/dropdown-menu";
import { useApproveMember } from "../hooks/mutations/useApproveMember";
import { useDeleteMember, useRejectMember } from "../hooks/mutations/useRemoveMember";
import { useReactivateMember } from "../hooks/mutations/useReactivateMember";
import { useSuspendMember } from "../hooks/mutations/useSuspendMember";
import type { Member } from "../types/access.types";
import { MemberStatusBadge } from "./MemberStatusBadge";

type Panel = "reject" | "delete" | null;

export function MemberRow({ member }: { member: Member }) {
  const [panel, setPanel] = useState<Panel>(null);
  const [reason, setReason] = useState("");

  const approve = useApproveMember();
  const suspend = useSuspendMember();
  const reactivate = useReactivateMember();
  const reject = useRejectMember();
  const remove = useDeleteMember();

  const { id, status } = member;
  const isAdmin = member.role === "Admin";

  return (
    <>
      <TableRow>
        <TableCell className="font-medium">{member.name}</TableCell>
        <TableCell className="text-muted-foreground">{member.email}</TableCell>
        <TableCell><MemberStatusBadge status={status} /></TableCell>
        <TableCell className="text-muted-foreground">
          {formatDistanceToNow(new Date(member.createdAtUtc), { addSuffix: true })}
        </TableCell>
        <TableCell className="text-right">
          {isAdmin ? null : status === "Pending" ? (
            <div className="flex justify-end gap-2">
              <Button size="sm" onClick={() => approve.mutate(id)}>Approve</Button>
              <Button size="sm" variant="outline" className="border-destructive/40 text-destructive hover:bg-destructive/10" onClick={() => setPanel("reject")}>
                Reject
              </Button>
            </div>
          ) : (
            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <Button variant="ghost" size="icon-sm" aria-label={`Actions for ${member.name}`}>
                  <EllipsisVertical />
                </Button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                {status === "Approved" && <DropdownMenuItem onSelect={() => suspend.mutate(id)}>Suspend</DropdownMenuItem>}
                {status === "Suspended" && <DropdownMenuItem onSelect={() => reactivate.mutate(id)}>Reactivate</DropdownMenuItem>}
                <DropdownMenuItem variant="destructive" onSelect={() => setPanel("delete")}>Delete</DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          )}
        </TableCell>
      </TableRow>

      {panel && (
        <TableRow className="bg-muted/40 hover:bg-muted/40">
          <TableCell colSpan={5}>
            {panel === "reject" ? (
              <form
                className="flex flex-wrap items-center gap-2"
                onSubmit={(e) => {
                  e.preventDefault();
                  reject.mutate({ id, reason: reason.trim() });
                }}
              >
                <Input
                  value={reason}
                  onChange={(e) => setReason(e.target.value)}
                  maxLength={500}
                  placeholder="Reason (optional)"
                  aria-label="Reason for rejecting"
                  className="max-w-sm"
                />
                <Button type="submit" size="sm" variant="destructive" disabled={reject.isPending}>Confirm reject</Button>
                <Button type="button" size="sm" variant="ghost" onClick={() => setPanel(null)}>Cancel</Button>
              </form>
            ) : (
              <div className="flex flex-wrap items-center gap-2">
                <p className="text-sm text-muted-foreground">
                  Delete {member.email} and all of their accounts, transactions, budgets and categories? This can&apos;t be undone.
                </p>
                <Button size="sm" variant="destructive" disabled={remove.isPending} onClick={() => remove.mutate({ id })}>
                  Confirm delete
                </Button>
                <Button size="sm" variant="ghost" onClick={() => setPanel(null)}>Cancel</Button>
              </div>
            )}
          </TableCell>
        </TableRow>
      )}
    </>
  );
}
