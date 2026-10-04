import { Link } from 'react-router-dom';
import { RegisterForm } from '../../features/auth/ui/RegisterForm';
import '../login/auth.css';

export function RegisterPage() {
  return (
    <main className="auth-page">
      <section className="auth-card" aria-labelledby="register-title">
        <h1 id="register-title">Регистрация мастера</h1>
        <RegisterForm />
        <Link className="auth-page__back" to="/login">Уже есть аккаунт? Войти</Link>
      </section>
    </main>
  );
}
