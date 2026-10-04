import { masterClient } from '../http/masterClient';
import type { AuthResponse, LoginRequest, RegisterRequest } from '../types/auth.types';

export const authApi = {
  async register(request: RegisterRequest): Promise<void> {
    await masterClient.post('/Auth/register', request);
  },

  async login(request: LoginRequest): Promise<AuthResponse> {
    const { data } = await masterClient.post<AuthResponse>('/Auth/login', request);
    return data;
  },

  async loginWithVk(vkParams: Record<string, string>): Promise<AuthResponse> {
    const { data } = await masterClient.post<AuthResponse>('/Auth/vk-mini-app', { vkParams });
    return data;
  },
};
