import { useState } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { Button, Input, authApi } from '@blanko/shared';
import { useAuthStore } from '../../../entities/auth/model/authStore';
import { loginSchema, type LoginFormValues } from '../model/authSchemas';
import { getAuthErrorMessage } from '../model/getAuthErrorMessage';
import { VkLoginButton } from './VkLoginButton';

interface LoginFormProps {
  registered?: boolean;
}

export function LoginForm({ registered = false }: LoginFormProps) {
  const navigate = useNavigate();
  const setAuth = useAuthStore((state) => state.setAuth);
  const [apiError, setApiError] = useState('');
  const loginMutation = useMutation({ mutationFn: authApi.login });
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: '', password: '' },
  });

  const onSubmit = async (values: LoginFormValues) => {
    setApiError('');
    try {
      const auth = await loginMutation.mutateAsync(values);
      setAuth(auth);
      navigate('/appointments', { replace: true });
    } catch (error: unknown) {
      setApiError(getAuthErrorMessage(error));
    }
  };

  return (
    <form className="auth-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      {registered && <p className="auth-success" role="status">Регистрация завершена. Теперь войдите в аккаунт.</p>}
      {apiError && <p className="auth-error" role="alert">{apiError}</p>}
      <Input
        {...register('email')}
        label="Email"
        type="email"
        autoComplete="email"
        inputMode="email"
        error={errors.email?.message}
      />
      <Input
        {...register('password')}
        label="Пароль"
        type="password"
        autoComplete="current-password"
        error={errors.password?.message}
      />
      <Button fullWidth type="submit" loading={loginMutation.isPending}>
        Войти
      </Button>
      <VkLoginButton />
      <p className="auth-form__footer">
        Нет аккаунта? <Link to="/register">Зарегистрироваться</Link>
      </p>
    </form>
  );
}
