import { Navigate, Outlet } from 'react-router-dom';
import { Placeholder } from '@blanko/shared';
import { useAuthStore } from '../../entities/auth/model/authStore';

export function ProtectedRoute() {
  const token = useAuthStore((state) => state.token);
  const isHydrated = useAuthStore((state) => state.isHydrated);

  if (!isHydrated) return <Placeholder>Загрузка профиля…</Placeholder>;
  return token ? <Outlet /> : <Navigate to="/login" replace />;
}
