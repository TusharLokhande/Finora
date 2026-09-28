export interface AuthUser {
  id: string;
  name: string;
  email: string;
}

export interface AuthResult {
  accessToken: string;
  expiresInSeconds: number;
  user: AuthUser;
}
