import { Link, useLocation } from 'react-router-dom';
import { LoginForm } from '../../features/auth/ui/LoginForm';
import './auth.css';

export function LoginPage() {
  const location = useLocation();
  const navigationState: unknown = location.state;
  const registered =
    typeof navigationState === 'object' && navigationState !== null &&
    'registered' in navigationState && navigationState.registered === true;

  return (
    <main className="auth-page">
      <section className="auth-card" aria-labelledby="login-title">
        <h1 id="login-title">Вход в BlankoCRM</h1>
        <LoginForm registered={registered} />
        <Link className="auth-page__back" to="/register">Создать аккаунт</Link>
      </section>
    </main>
  );
}
