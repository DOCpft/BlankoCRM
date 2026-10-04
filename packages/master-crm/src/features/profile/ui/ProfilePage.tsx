import { Avatar, Card } from '@blanko/shared';
import { useAuthStore } from '../../../entities/auth/model/authStore';

export function ProfilePage() {
  const user = useAuthStore((state) => state.user);

  return (
    <section className="crm-page">
      <header className="crm-page__header">
        <div>
          <h1>Профиль</h1>
          <p>Данные учётной записи</p>
        </div>
      </header>
      {user ? (
        <Card className="crm-profile-card">
          <Avatar name={user.name || user.email} size={64} />
          <div className="crm-profile-card__details">
            <strong>{user.name || 'Имя не указано'}</strong>
            <span>{user.email}</span>
          </div>
        </Card>
      ) : (
        <p className="crm-data-list__state" role="status">Данные профиля недоступны. Выйдите и войдите в аккаунт повторно.</p>
      )}
      <p className="crm-profile-note">Изменение данных профиля пока недоступно.</p>
    </section>
  );
}
