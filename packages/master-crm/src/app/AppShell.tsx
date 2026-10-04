import { Outlet, useLocation, useNavigate } from 'react-router-dom';
import { AppBar, Button, Icon, Sidebar, useSidebarGesture } from '@blanko/shared';
import { useAuthStore } from '../entities/auth/model/authStore';

const navigation = [
  { id: 'appointments', label: 'Записи', icon: <Icon name="appointments" />, href: '/appointments' },
  { id: 'pending', label: 'Заявки', icon: <Icon name="schedule" />, href: '/appointments/pending' },
  { id: 'clients', label: 'Клиенты', icon: <Icon name="clients" />, href: '/clients' },
  { id: 'services', label: 'Услуги', icon: <Icon name="services" />, href: '/services' },
  { id: 'schedule', label: 'Расписание', icon: <Icon name="schedule" />, href: '/schedule' },
  { id: 'profile', label: 'Профиль', icon: <Icon name="profile" />, href: '/profile' },
  { id: 'settings', label: 'Настройки', icon: <Icon name="settings" />, href: '/profile/reminders' },
];

const pages = [
  { path: '/appointments', title: 'Записи' },
  { path: '/appointments/pending', title: 'Заявки' },
  { path: '/clients', title: 'Клиенты' },
  { path: '/services', title: 'Услуги' },
  { path: '/schedule', title: 'Расписание' },
  { path: '/profile/telegram', title: 'Telegram' },
  { path: '/profile/reminders', title: 'Напоминания' },
  { path: '/profile/timezone', title: 'Часовой пояс' },
  { path: '/profile', title: 'Профиль' },
];

export function AppShell() {
  const { pathname } = useLocation();
  const navigate = useNavigate();
  const sidebar = useSidebarGesture();
  const logout = useAuthStore((state) => state.logout);
  const pageTitle = pages.find((page) => pathname === page.path)?.title ?? 'BlankoCRM';

  return (
    <div className="crm-shell" {...sidebar.handlers}>
      <Sidebar
        items={navigation}
        isOpen={sidebar.isOpen}
        onClose={sidebar.close}
        dragOffset={sidebar.dragOffset}
        isDragging={sidebar.isDragging}
      />
      <main className="crm-main">
        <div className="crm-content">
          <AppBar
            title={pageTitle}
            onMenuClick={sidebar.open}
            actions={(
              <Button
                variant="tertiary"
                onClick={() => {
                  logout();
                  navigate('/login', { replace: true });
                }}
              >
                Выйти
              </Button>
            )}
          />
          <Outlet />
        </div>
      </main>
    </div>
  );
}
