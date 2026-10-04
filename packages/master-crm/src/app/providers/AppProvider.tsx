import { useEffect, type ReactNode } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { BrowserRouter, useNavigate } from 'react-router-dom';
import { queryClient } from '@blanko/shared';
import { useAuthStore } from '../../entities/auth/model/authStore';

function SessionExpiryHandler() {
  const navigate = useNavigate();

  useEffect(() => {
    const handleUnauthorized = () => {
      useAuthStore.getState().logout();
      navigate('/login', { replace: true });
    };

    window.addEventListener('blanko:unauthorized', handleUnauthorized);
    return () => window.removeEventListener('blanko:unauthorized', handleUnauthorized);
  }, [navigate]);

  return null;
}

export function AppProvider({ children }: { children: ReactNode }) {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <SessionExpiryHandler />
        {children}
      </BrowserRouter>
    </QueryClientProvider>
  );
}
