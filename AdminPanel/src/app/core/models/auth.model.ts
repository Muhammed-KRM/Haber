export interface UserDto {
  id: string;
  email: string;
  fullName: string;
  profileImageUrl?: string;
  bio?: string;
  role: string;
  createdAt: string;
}

export interface AuthResultDto {
  success: boolean;
  token: string;
  refreshToken: string;
  user?: UserDto;
  errorMessage?: string;
}

export interface AdminUserDto {
  id: string;
  fullName: string;
  email: string;
  role: number;
  isActive: boolean;
  isEmailVerified: boolean;
  createdAt: string;
  newsCount: number;
}
