import { axiosClient } from "@/api/axiosClient";
import type { AccessSettings, AuditLogPage, Member, MemberList } from "../types/access.types";

// The page always loads every status and filters by tab in the client; the endpoint also takes ?status=.
export async function getMembers(): Promise<MemberList> {
  const res = await axiosClient.get<MemberList>("/access/members", { params: { status: "All" } });
  return res.data;
}

export async function approveMember(id: string): Promise<Member> {
  const res = await axiosClient.post<Member>(`/access/members/${id}/approve`);
  return res.data;
}

export async function suspendMember(id: string): Promise<Member> {
  const res = await axiosClient.post<Member>(`/access/members/${id}/suspend`);
  return res.data;
}

export async function reactivateMember(id: string): Promise<Member> {
  const res = await axiosClient.post<Member>(`/access/members/${id}/reactivate`);
  return res.data;
}

export async function rejectMember(id: string, reason: string): Promise<void> {
  await axiosClient.post(`/access/members/${id}/reject`, { reason: reason || null });
}

export async function deleteMember(id: string): Promise<void> {
  await axiosClient.delete(`/access/members/${id}`);
}

export async function getAccessSettings(): Promise<AccessSettings> {
  const res = await axiosClient.get<AccessSettings>("/access/settings");
  return res.data;
}

export async function updateAccessSettings(signupsOpen: boolean): Promise<AccessSettings> {
  const res = await axiosClient.put<AccessSettings>("/access/settings", { signupsOpen });
  return res.data;
}

export async function getAuditLog(page: number, pageSize: number): Promise<AuditLogPage> {
  const res = await axiosClient.get<AuditLogPage>("/access/audit-log", { params: { page, pageSize } });
  return res.data;
}
