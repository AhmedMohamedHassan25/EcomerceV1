export interface User {
  userId?: number;
  userName: string;
  email: string;
  password?: string;
  createdAt?: Date;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  user: User;
  expiresAt: Date;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}
