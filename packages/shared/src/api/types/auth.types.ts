export interface AuthResponse {
  token: string;
  name: string;
  email: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest extends LoginRequest {
  name: string | null;
  organizationName: string | null;
}

export interface VkMiniAppAuthRequest {
  vkParams: Record<string, string>;
}
