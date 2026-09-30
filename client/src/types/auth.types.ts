export type UserRole = "User" | "Admin";
export type UserStatus = "Pending" | "Approved" | "Rejected" | "Suspended";

export interface AuthUser {
  id: string;
  name: string;
  email: string;
  role: UserRole;
  status: UserStatus;
  /** Why access was withdrawn, when the server has one. */
  reason: string | null;
}

export interface AuthResult {
  accessToken: string;
  expiresInSeconds: number;
  user: AuthUser;
}
