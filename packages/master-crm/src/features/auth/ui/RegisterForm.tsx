import { useState } from 'react';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useForm } from 'react-hook-form';
import { Link, useNavigate } from 'react-router-dom';
import { Button, Input, authApi } from '@blanko/shared';
import { registerSchema, type RegisterFormValues } from '../model/authSchemas';
import { getAuthErrorMessage } from '../model/getAuthErrorMessage';

export function RegisterForm() {
  const navigate = useNavigate();
  const [apiError, setApiError] = useState('');
  const registerMutation = useMutation({ mutationFn: authApi.register });
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: { name: '', organizationName: '', email: '', password: '' },
  });

  const onSubmit = async (values: RegisterFormValues) => {
    setApiError('');
    try {
      await registerMutation.mutateAsync({
        name: values.name || null,
        organizationName: values.organizationName || null,
        email: values.email,
        password: values.password,
      });
      navigate('/login', { replace: true, state: { registered: true } });
    } catch (error: unknown) {
      setApiError(getAuthErrorMessage(error));
    }
  };

  return (
    <form className="auth-form" onSubmit={handleSubmit(onSubmit)} noValidate>
      {apiError && <p className="auth-error" role="alert">{apiError}</p>}
      <Input
        {...register('name')}
        label="Ваше имя"
        autoComplete="name"
        error={errors.name?.message}
      />
      <Input
        {...register('organizationName')}
        label="Название организации"
        autoComplete="organization"
        error={errors.organizationName?.message}
      />
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
        autoComplete="new-password"
        error={errors.password?.message}
      />
      <Button fullWidth type="submit" loading={registerMutation.isPending}>
        Создать аккаунт
      </Button>
      <p className="auth-form__footer">
        Уже есть аккаунт? <Link to="/login">Войти</Link>
      </p>
    </form>
  );
}
