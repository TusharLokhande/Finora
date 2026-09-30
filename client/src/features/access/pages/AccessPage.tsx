import { useState } from "react";
import { Navigate } from "react-router-dom";
import { PageBreadcrumb } from "@/components/layout/PageBreadcrumb";
import { useMe } from "@/features/auth";
import { Label } from "@/ui/label";
import { Skeleton } from "@/ui/skeleton";
import { Switch } from "@/ui/switch";
import { Table, TableBody, TableHead, TableHeader, TableRow } from "@/ui/table";
import { Tabs, TabsList, TabsTrigger } from "@/ui/tabs";
import { AuditLogTab } from "../components/AuditLogTab";
import { MemberRow } from "../components/MemberRow";
import { useUpdateAccessSettings } from "../hooks/mutations/useUpdateAccessSettings";
import { useAccessSettings, useMembers } from "../hooks/queries/useMembers";
import type { MemberCounts } from "../types/access.types";

const TABS: { value: string; label: string; count: keyof MemberCounts }[] = [
  { value: "All", label: "All", count: "all" },
  { value: "Pending", label: "Pending", count: "pending" },
  { value: "Approved", label: "Approved", count: "approved" },
  { value: "Rejected", label: "Rejected", count: "rejected" },
  { value: "Suspended", label: "Suspended", count: "suspended" },
];

const AUDIT_TAB = "audit";

export function AccessPage() {
  const { data: me } = useMe();
  const { data, isLoading, isError } = useMembers();
  const { data: settings } = useAccessSettings();
  const updateSettings = useUpdateAccessSettings();
  const [tab, setTab] = useState("All");

  // Backend enforces this too; this just keeps non-admins off a page that would only show errors.
  if (me && me.role !== "Admin") return <Navigate to="/" replace />;

  const members = (data?.items ?? []).filter((m) => tab === "All" || m.status === tab);

  return (
    <div className="flex w-full flex-col gap-5 p-4 md:p-6">
      <PageBreadcrumb items={["Access"]} />
      <div className="flex flex-wrap items-start justify-between gap-4">
        <div>
          <h1 className="font-heading text-xl font-semibold text-foreground">Access</h1>
          <p className="text-sm text-muted-foreground">Review who can sign in to Finora.</p>
        </div>
        <div className="flex items-center gap-2">
          <Switch
            id="accept-requests"
            checked={settings?.signupsOpen ?? false}
            disabled={!settings}
            onCheckedChange={(open) => updateSettings.mutate(open)}
          />
          <Label htmlFor="accept-requests">Accept new requests</Label>
        </div>
      </div>

      <Tabs value={tab} onValueChange={setTab}>
        <TabsList>
          {TABS.map((t) => (
            <TabsTrigger key={t.value} value={t.value}>
              {t.label}
              {data && <span className="ml-1.5 text-muted-foreground tabular-nums">{data.counts[t.count]}</span>}
            </TabsTrigger>
          ))}
          <TabsTrigger value={AUDIT_TAB}>Audit log</TabsTrigger>
        </TabsList>
      </Tabs>

      {tab === AUDIT_TAB ? (
        <AuditLogTab />
      ) : isLoading ? (
        <Skeleton className="h-40 w-full" />
      ) : isError ? (
        <p className="text-sm text-destructive">Couldn&apos;t load members.</p>
      ) : members.length === 0 ? (
        <p className="py-8 text-center text-sm text-muted-foreground">Nobody here.</p>
      ) : (
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Requested / joined</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {members.map((member) => (
              <MemberRow key={member.id} member={member} />
            ))}
          </TableBody>
        </Table>
      )}

      <p className="text-xs text-muted-foreground/80">
        You can&apos;t see any user&apos;s transactions, balances or categories from here.
      </p>
    </div>
  );
}
