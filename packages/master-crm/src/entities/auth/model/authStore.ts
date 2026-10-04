import { create } from 'zustand';
import { createJSONStorage, persist } from 'zustand/middleware';
import type { AuthResponse } from '@blanko/shared';

export interface AuthUser {
  name: string;
  email: string;
}

interface AuthState {
  token: string | null;
  user: AuthUser | null;
  isHydrated: boolean;
  setAuth: (auth: AuthResponse) => void;
  logout: () => void;
  setHydrated: (isHydrated: boolean) => void;
}

interface PersistedAuthState {
  token: string | null;
  user: AuthUser | null;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      token: null,
      user: null,
      isHydrated: false,
      setAuth: ({ token, name, email }) => set({ token, user: { name, email } }),
      logout: () => set({ token: null, user: null }),
      setHydrated: (isHydrated) => set({ isHydrated }),
    }),
    {
      name: 'blanko-crm-auth',
      storage: createJSONStorage(() => localStorage),
      partialize: ({ token, user }): PersistedAuthState => ({ token, user }),
      onRehydrateStorage: () => (state) => state?.setHydrated(true),
    },
  ),
);
