import { useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AppointmentStatus, Avatar, Button, DataList, Snackbar } from '@blanko/shared';
import type { AppointmentDto, ClientDto, ServiceDto } from '@blanko/shared';
import { useAppointments, useCancelAppointment, useCompleteAppointment, useDeleteAppointment, useMarkAppointmentNoShow, useUpdateAppointment } from '../../../entities/appointments/api/appointmentQueries';
import { useClients } from '../../../entities/clients/api/clientQueries';
import { useServices } from '../../../entities/services/api/serviceQueries';
import { AppointmentForm } from './AppointmentForm';
import type { AppointmentFormValues } from '../model/appointmentSchema';

const statusLabels: Record<AppointmentStatus, string> = {
  [AppointmentStatus.Scheduled]: 'Подтверждена',
  [AppointmentStatus.Pending]: 'Ожидает подтверждения',
  [AppointmentStatus.Completed]: 'Завершена',
  [AppointmentStatus.Cancelled]: 'Отменена',
  [AppointmentStatus.NoShow]: 'Неявка',
  [AppointmentStatus.Archived]: 'Архив',
  [AppointmentStatus.Rejected]: 'Отклонена',
  [AppointmentStatus.Withdrawn]: 'Отозвана',
};

const dateTime = (value: string) => new Date(value).toLocaleString('ru-RU', { day: 'numeric', month: 'short', hour: '2-digit', minute: '2-digit' });

export function AppointmentsPage() {
  const appointmentsQuery = useAppointments();
  const navigate = useNavigate();
  const clientsQuery = useClients();
  const servicesQuery = useServices();
  const [selected, setSelected] = useState<AppointmentDto | null>(null);
  const [notice, setNotice] = useState('');
  const update = useUpdateAppointment();
  const cancel = useCancelAppointment();
  const complete = useCompleteAppointment();
  const noShow = useMarkAppointmentNoShow();
  const remove = useDeleteAppointment();
  const clients = clientsQuery.data ?? [];
  const services = servicesQuery.data ?? [];
  const items = useMemo(() => [...(appointmentsQuery.data ?? [])].sort((a, b) => a.startTime.localeCompare(b.startTime)), [appointmentsQuery.data]);
  const submit = async (values: AppointmentFormValues) => {
    if (!selected) return;
    try {
      await update.mutateAsync({ id: selected.id, request: { clientId: values.clientId, services: values.services.map(({ serviceId, quantity }) => ({ serviceId, quantity })), startTime: values.startTime, notes: values.notes || null } });
      setSelected(null);
      setNotice('Запись обновлена');
    } catch { setNotice('Не удалось обновить запись'); }
  };
  const action = async (callback: () => Promise<unknown>, message: string) => {
    try { await callback(); setNotice(message); } catch { setNotice('Действие не выполнено. Возможно, статус уже изменился.'); }
  };
  const cardActions = (item: AppointmentDto) => <>
    {[AppointmentStatus.Pending, AppointmentStatus.Scheduled].includes(item.status) && <Button variant="tertiary" onClick={(event) => { event.stopPropagation(); void action(() => cancel.mutateAsync(item.id), 'Запись отменена'); }}>Отменить</Button>}
    {item.status === AppointmentStatus.Scheduled && <><Button variant="secondary" onClick={(event) => { event.stopPropagation(); void action(() => complete.mutateAsync(item.id), 'Визит завершён'); }}>Завершить</Button><Button variant="tertiary" onClick={(event) => { event.stopPropagation(); void action(() => noShow.mutateAsync(item.id), 'Отмечена неявка'); }}>Не пришёл</Button></>}
  </>;
  return <section className="crm-page">
    <header className="crm-page__header"><div><h1>Записи</h1><p>Все записи клиентов и их статусы</p></div><Button onClick={() => navigate('/schedule')}>Новая запись</Button></header>
    {(clientsQuery.isError || servicesQuery.isError || appointmentsQuery.isError) && <p className="auth-error" role="alert">Не удалось загрузить записи или справочники.</p>}
    <DataList<AppointmentDto>
      items={items}
      getId={(item) => item.id}
      loading={appointmentsQuery.isLoading || clientsQuery.isLoading || servicesQuery.isLoading}
      emptyState="Записей пока нет"
      onItemClick={setSelected}
      renderCardLeading={(item) => <Avatar name={item.clientName || clients.find((client: ClientDto) => client.id === item.clientId)?.name || 'Клиент'} />}
      renderCardTitle={(item) => item.clientName || clients.find((client: ClientDto) => client.id === item.clientId)?.name || 'Клиент'}
      renderCardSubtitle={(item) => dateTime(item.startTime)}
      renderCardDescription={(item) => item.services.map((entry) => `${entry.serviceName ?? services.find((service: ServiceDto) => service.id === entry.serviceId)?.name ?? 'Услуга'} × ${entry.quantity}`).join(', ')}
      renderCardBadge={(item) => <span className={`crm-status-badge crm-status-badge--${item.status.toLowerCase()}`}>{statusLabels[item.status]}</span>}
      cardActions={cardActions}
      columns={[
        { key: 'client', label: 'Клиент', render: (item) => item.clientName || '—' },
        { key: 'date', label: 'Дата и время', render: (item) => dateTime(item.startTime) },
        { key: 'status', label: 'Статус', render: (item) => statusLabels[item.status] },
        { key: 'price', label: 'Сумма', render: (item) => `${item.totalPrice.toLocaleString('ru-RU')} ₽` },
      ]}
    />
    {selected && <AppointmentForm key={selected.id} appointment={selected} clients={clients} services={services} loading={update.isPending || remove.isPending} onClose={() => setSelected(null)} onSubmit={submit} onDelete={() => void action(async () => { await remove.mutateAsync(selected.id); setSelected(null); }, 'Запись удалена')} />}
    {notice && <Snackbar message={notice} />}
  </section>;
}