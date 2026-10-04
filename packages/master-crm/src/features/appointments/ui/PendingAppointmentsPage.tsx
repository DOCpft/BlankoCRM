import { useState } from 'react';
import { Avatar, Button, DataList, Snackbar } from '@blanko/shared';
import type { AppointmentDto, ClientDto } from '@blanko/shared';
import { useConfirmAppointment, usePendingAppointments, useRejectAppointment } from '../../../entities/appointments/api/appointmentQueries';
import { useClients } from '../../../entities/clients/api/clientQueries';

const dateTime = (value: string) => new Date(value).toLocaleString('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });

export function PendingAppointmentsPage() {
  const pending = usePendingAppointments();
  const clientsQuery = useClients();
  const confirm = useConfirmAppointment();
  const reject = useRejectAppointment();
  const [notice, setNotice] = useState('');
  const clients = clientsQuery.data ?? [];
  const run = async (callback: () => Promise<unknown>, message: string) => {
    try { await callback(); setNotice(message); }
    catch { setNotice('Не удалось изменить заявку. Возможно, она уже обработана. Список обновлён.'); }
  };
  const actions = (item: AppointmentDto) => <div className="crm-pending-actions">
    <Button variant="secondary" onClick={(event) => { event.stopPropagation(); void run(() => confirm.mutateAsync(item.id), 'Заявка подтверждена'); }}>Подтвердить</Button>
    <Button variant="tertiary" onClick={(event) => { event.stopPropagation(); void run(() => reject.mutateAsync(item.id), 'Заявка отклонена'); }}>Отклонить</Button>
  </div>;
  return <section className="crm-page">
    <header className="crm-page__header"><div><h1>Заявки</h1><p>Ожидают подтверждения</p></div></header>
    {pending.isError && <p className="auth-error" role="alert">Не удалось загрузить заявки. Обновите страницу и попробуйте снова.</p>}
    {clientsQuery.isError && <p className="auth-error" role="alert">Не удалось загрузить данные клиентов.</p>}
    <DataList<AppointmentDto>
      items={pending.data ?? []}
      getId={(item) => item.id}
      loading={pending.isLoading || clientsQuery.isLoading}
      emptyState="Нет заявок на подтверждение"
      variant="action-cards"
      renderCardLeading={(item) => <Avatar name={item.clientName || clients.find((client: ClientDto) => client.id === item.clientId)?.name || 'Клиент'} />}
      renderCardTitle={(item) => item.clientName || clients.find((client: ClientDto) => client.id === item.clientId)?.name || 'Клиент'}
      renderCardSubtitle={(item) => dateTime(item.startTime)}
      renderCardDescription={(item) => item.services.map((service) => `${service.serviceName ?? 'Услуга'} × ${service.quantity}`).join(', ')}
      cardActions={actions}
      columns={[
        { key: 'client', label: 'Клиент', render: (item) => item.clientName || '—' },
        { key: 'date', label: 'Дата и время', render: (item) => dateTime(item.startTime) },
        { key: 'services', label: 'Услуги', render: (item) => item.services.map((service) => service.serviceName ?? 'Услуга').join(', ') },
        { key: 'actions', label: 'Действия', render: actions },
      ]}
    />
    {notice && <Snackbar message={notice} />}
  </section>;
}