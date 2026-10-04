import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Button, authApi, vkService } from '@blanko/shared';
import { useAuthStore } from '../../../entities/auth/model/authStore';
import { getAuthErrorMessage } from '../model/getAuthErrorMessage';

export function VkLoginButton() {
  const navigate = useNavigate();
  const setAuth = useAuthStore((state) => state.setAuth);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  if (!vkService.isAvailable()) return null;

  const handleLogin = async () => {
    setError('');
    setLoading(true);
    try {
      const params = await vkService.getLaunchParams();
      console.info('VK Mini Apps auth request', {
        vkParams: { ...params, ...(params.sign ? { sign: '[REDACTED]' } : {}) },
      });
      const auth = await authApi.loginWithVk(params);
      setAuth(auth);
      navigate('/appointments', { replace: true });
    } catch (requestError: unknown) {
      setError(getAuthErrorMessage(requestError));
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="auth-vk-login">
      <Button fullWidth variant="secondary" loading={loading} onClick={handleLogin}>
        Войти через VK
      </Button>
      {error && <p className="auth-error" role="alert">{error}</p>}
    </div>
  );
}
