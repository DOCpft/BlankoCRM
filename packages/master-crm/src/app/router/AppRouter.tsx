import { Navigate, Route, Routes } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { PublicOnlyRoute } from './PublicOnlyRoute';
import { AppShell } from '../AppShell';
import { LoginPage } from '../../pages/login/LoginPage';
import { RegisterPage } from '../../pages/register/RegisterPage';
import { PlaceholderPage } from '../../pages/placeholder/PlaceholderPage';
import { ClientsPage } from '../../features/clients/ui/ClientsPage';
import { ServicesPage } from '../../features/services/ui/ServicesPage';
import { SchedulePage } from '../../features/schedule/ui/SchedulePage';
import { AppointmentsPage } from '../../features/appointments/ui/AppointmentsPage';
import { PendingAppointmentsPage } from '../../features/appointments/ui/PendingAppointmentsPage';
import { ProfilePage } from '../../features/profile/ui/ProfilePage';
import { TelegramSettingsPage } from '../../features/profile/ui/TelegramSettingsPage';
import { ReminderSettingsPage } from '../../features/profile/ui/ReminderSettingsPage';
import { TimezoneSettingsPage } from '../../features/profile/ui/TimezoneSettingsPage';

export function AppRouter() {
  return (
    <Routes>
      <Route element={<PublicOnlyRoute />}>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
      </Route>
      <Route element={<ProtectedRoute />}>
        <Route element={<AppShell />}>
          <Route path="/" element={<Navigate to="/appointments" replace />} />
          <Route path="/appointments" element={<AppointmentsPage />} />
          <Route path="/appointments/pending" element={<PendingAppointmentsPage />} />
          <Route path="/appointments/:id" element={<PlaceholderPage title="Карточка записи" />} />
          <Route path="/clients" element={<ClientsPage />} />
          <Route path="/clients/:id" element={<PlaceholderPage title="Карточка клиента" />} />
          <Route path="/services" element={<ServicesPage />} />
          <Route path="/services/:id" element={<PlaceholderPage title="Карточка услуги" />} />
          <Route path="/schedule" element={<SchedulePage />} />
          <Route path="/profile" element={<ProfilePage />} />
          <Route path="/profile/telegram" element={<TelegramSettingsPage />} />
          <Route path="/profile/reminders" element={<ReminderSettingsPage />} />
          <Route path="/profile/timezone" element={<TimezoneSettingsPage />} />
          <Route path="*" element={<Navigate to="/appointments" replace />} />
        </Route>
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  );
}
