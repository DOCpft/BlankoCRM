import { Navigate, Outlet } from 'react-router-dom';
import { Placeholder } from '@blanko/shared';
import { useAuthStore } from '../../entities/auth/model/authStore';

export function PublicOnlyRoute() {
  const token = useAuthStore((state) => state.token);
  const isHydrated = useAuthStore((state) => state.isHydrated);

  if (!isHydrated) return <Placeholder>Загрузка профиля…</Placeholder>;
  return token ? <Navigate to="/appointments" replace /> : <Outlet />;
}
