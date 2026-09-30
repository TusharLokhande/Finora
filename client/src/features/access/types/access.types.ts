import type { UserRole, UserStatus } from "@/types/auth.types";

export interface Member {
  id: string;
  name: string;
  email: string;
  status: UserStatus;
  role: UserRole;
  createdAtUtc: string;
}

export interface MemberCounts {
  all: number;
  pending: number;
  approved: number;
  rejected: number;
  suspended: number;
}

export interface MemberList {
  items: Member[];
  counts: MemberCounts;
}

export interface AccessSettings {
  signupsOpen: boolean;
}

export type AuditAction = "Approve" | "Reject" | "Suspend" | "Reactivate" | "DeleteUser" | "ToggleSignups";

export interface AuditLogEntry {
  id: string;
  action: AuditAction;
  adminEmail: string;
  targetEmail: string | null;
  reason: string | null;
  createdAtUtc: string;
}

export interface AuditLogPage {
  items: AuditLogEntry[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
