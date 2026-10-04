import { useMemo, useState } from 'react';
import { AppointmentStatus, Button, Modal, Snackbar } from '@blanko/shared';
import type { AppointmentDto, ClientDto, ScheduleDto, ServiceDto } from '@blanko/shared';
import { useCancelAppointment, useCompleteAppointment, useConfirmAppointment, useDeleteAppointment, useMarkAppointmentNoShow, useRejectAppointment, useUpdateAppointment } from '../../../entities/appointments/api/appointmentQueries';
import { useClients } from '../../../entities/clients/api/clientQueries';
import { useCreateFreeSlots, useCreateOccupiedSlot, useDeleteSlot, useSchedulePeriod } from '../../../entities/schedule/api/scheduleQueries';
import { useServices } from '../../../entities/services/api/serviceQueries';
import { getWeekRange, getWeekStart } from '../../../utils/dateUtils';
import { AppointmentForm } from '../../appointments/ui/AppointmentForm';
import type { AppointmentFormValues } from '../../appointments/model/appointmentSchema';

const dateOf = (value: string) => new Date(value);
const time = (value: string) => dateOf(value).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
const emptySchedule: ScheduleDto[] = [];

export function SchedulePage() {
  const [weekStart, setWeekStart] = useState(() => getWeekStart(new Date()));
  const [selectedStart, setSelectedStart] = useState<Date | null>(null);
  const [selectedAppointment, setSelectedAppointment] = useState<AppointmentDto | null>(null);
  const [notice, setNotice] = useState('');
  const [bulkOpen, setBulkOpen] = useState(false);
  const range = useMemo(() => getWeekRange(weekStart), [weekStart]);
  const scheduleQuery = useSchedulePeriod(range.start.toISOString(), range.end.toISOString());
  const clientsQuery = useClients();
  const servicesQuery = useServices();
  const createAppointment = useCreateOccupiedSlot();
  const updateAppointment = useUpdateAppointment();
  const deleteAppointment = useDeleteAppointment();
  const createFree = useCreateFreeSlots();
  const deleteSlot = useDeleteSlot();
  const confirmAppointment = useConfirmAppointment();
  const rejectAppointment = useRejectAppointment();
  const cancelAppointment = useCancelAppointment();
  const completeAppointment = useCompleteAppointment();
  const noShowAppointment = useMarkAppointmentNoShow();
  const clients = clientsQuery.data ?? [];
  const services = servicesQuery.data ?? [];
  const schedule = scheduleQuery.data ?? emptySchedule;
  const loading = createAppointment.isPending || updateAppointment.isPending || deleteAppointment.isPending;
  const days = useMemo(() => Array.from({ length: 7 }, (_, index) => { const day = new Date(weekStart); day.setDate(day.getDate() + index); return day; }), [weekStart]);
  const daySlots = useMemo(() => days.map((day) => schedule.filter((slot) => dateOf(slot.startTime).toDateString() === day.toDateString())), [days, schedule]);

  const submitAppointment = async (values: AppointmentFormValues) => {
    const clientServices = values.services.map(({ serviceId, quantity }) => ({ serviceId, quantity }));
    try {
      if (selectedAppointment) await updateAppointment.mutateAsync({ id: selectedAppointment.id, request: { clientId: values.clientId, services: clientServices, startTime: values.startTime, notes: values.notes || null } });
      else await createAppointment.mutateAsync({ startTime: values.startTime, appointmentData: { clientId: values.clientId, services: clientServices, notes: values.notes || null } });
      setSelectedStart(null);
      setSelectedAppointment(null);
      setNotice('Запись сохранена');
    } catch { setNotice('Не удалось сохранить запись. Обновите расписание и попробуйте снова.'); }
  };
  const runAction = async (action: () => Promise<unknown>, success: string) => {
    try { await action(); setNotice(success); } catch { setNotice('Действие не выполнено. Данные могли измениться — обновите список.'); }
  };

  return (
    <section className="crm-page">
      <header className="crm-page__header"><div><h1>Расписание</h1><p>Записи и доступные интервалы на неделю</p></div><Button variant="secondary" onClick={() => setBulkOpen(true)}>Добавить свободное время</Button></header>
      <div className="crm-schedule-toolbar">
        <Button variant="secondary" onClick={() => setWeekStart((value) => { const next = new Date(value); next.setDate(next.getDate() - 7); return next; })}>← Неделя</Button>
        <strong>{range.start.toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' })} — {new Date(range.end.getTime() - 1).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long', year: 'numeric' })}</strong>
        <Button variant="secondary" onClick={() => setWeekStart((value) => { const next = new Date(value); next.setDate(next.getDate() + 7); return next; })}>Неделя →</Button>
      </div>
      {scheduleQuery.isLoading && <p>Загрузка расписания…</p>}
      {(scheduleQuery.isError || clientsQuery.isError || servicesQuery.isError) && <p className="auth-error" role="alert">Не удалось загрузить расписание или справочники.</p>}
      {!scheduleQuery.isLoading && <div className="crm-schedule-week">{days.map((day, dayIndex) => (
        <section className="crm-schedule-day" key={day.toISOString()}>
          <h2>{day.toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long' })}</h2>
          {daySlots[dayIndex].length === 0 && <p>Нет интервалов</p>}
          <div className="crm-schedule-list">
            {daySlots[dayIndex].map((slot: ScheduleDto) => {
              const appointment = slot.appointment;
              const client = appointment && clients.find((item: ClientDto) => item.id === appointment.clientId);
              const serviceNames = appointment?.services.map((item) => services.find((service: ServiceDto) => service.id === item.serviceId)?.name ?? 'Услуга').join(', ');
              return <article className={`crm-schedule-item${appointment ? ' crm-schedule-item--booked' : ' crm-schedule-item--free'}`} key={slot.id}>
                <div className="crm-schedule-item__time"><strong>{time(slot.startTime)}–{time(slot.endTime)}</strong><span>{appointment ? 'Запись' : 'Свободно'}</span></div>
                <div className="crm-schedule-item__details"><strong>{appointment ? client?.name ?? 'Клиент' : 'Свободное время'}</strong>{appointment && <span>{serviceNames}</span>}{appointment?.notes && <small>{appointment.notes}</small>}</div>
                <div className="crm-schedule-item__actions">{appointment ? <>
                  <Button variant="secondary" onClick={() => { setSelectedAppointment(appointment); setSelectedStart(null); }}>Открыть</Button>
                  {appointment.status === AppointmentStatus.Pending && <><Button variant="secondary" onClick={() => void runAction(() => confirmAppointment.mutateAsync(appointment.id), 'Запись подтверждена')}>Подтвердить</Button><Button variant="tertiary" onClick={() => void runAction(() => rejectAppointment.mutateAsync(appointment.id), 'Заявка отклонена')}>Отклонить</Button></>}
                  {appointment.status === AppointmentStatus.Scheduled && <><Button variant="secondary" onClick={() => void runAction(() => completeAppointment.mutateAsync(appointment.id), 'Визит отмечен завершённым')}>Завершить</Button><Button variant="tertiary" onClick={() => void runAction(() => noShowAppointment.mutateAsync(appointment.id), 'Отмечена неявка')}>Не пришёл</Button></>}
                  {[AppointmentStatus.Pending, AppointmentStatus.Scheduled].includes(appointment.status) && <Button variant="tertiary" onClick={() => void runAction(() => cancelAppointment.mutateAsync(appointment.id), 'Запись отменена')}>Отменить</Button>}
                </> : <><Button variant="secondary" onClick={() => { setSelectedStart(dateOf(slot.startTime)); setSelectedAppointment(null); }}>Записать клиента</Button><Button variant="tertiary" onClick={() => void runAction(() => deleteSlot.mutateAsync(slot.id), 'Свободный интервал удалён')}>Удалить</Button></>}</div>
              </article>;
            })}
          </div>
        </section>
      ))}</div>}
      {(selectedStart || selectedAppointment) && <AppointmentForm clients={clients} services={services} appointment={selectedAppointment} initialStart={selectedStart ?? undefined} loading={loading} onClose={() => { setSelectedStart(null); setSelectedAppointment(null); }} onSubmit={submitAppointment} onDelete={selectedAppointment ? () => void runAction(async () => { await deleteAppointment.mutateAsync(selectedAppointment.id); setSelectedAppointment(null); }, 'Запись удалена') : undefined} />}
      {bulkOpen && <BulkFreeTime onClose={() => setBulkOpen(false)} onSave={async (requests) => { try { await createFree.mutateAsync(requests); setNotice('Свободные интервалы добавлены'); setBulkOpen(false); } catch { setNotice('Не удалось создать интервалы. Проверьте пересечения.'); } }} loading={createFree.isPending} />}
      {notice && <Snackbar message={notice} />}
    </section>
  );
}

function BulkFreeTime({ onClose, onSave, loading }: { onClose: () => void; onSave: (requests: Array<{ startTime: string; endTime: string }>) => Promise<void>; loading: boolean }) {
  const [startDate, setStartDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [endDate, setEndDate] = useState(() => new Date().toISOString().slice(0, 10));
  const [from, setFrom] = useState('09:00');
  const [to, setTo] = useState('17:00');
  const [duration, setDuration] = useState(60);
  const [breakMinutes, setBreakMinutes] = useState(0);
  const [weekdays, setWeekdays] = useState([1, 2, 3, 4, 5]);
  const toggleDay = (day: number) => setWeekdays((current) => current.includes(day) ? current.filter((item) => item !== day) : [...current, day]);
  const save = () => {
    const requests: Array<{ startTime: string; endTime: string }> = [];
    const first = new Date(`${startDate}T00:00:00`);
    const last = new Date(`${endDate}T00:00:00`);
    const [fromHour, fromMinute] = from.split(':').map(Number);
    const [toHour, toMinute] = to.split(':').map(Number);
    if (last < first || duration < 1 || breakMinutes < 0) return;
    for (const day = new Date(first); day <= last; day.setDate(day.getDate() + 1)) {
      if (!weekdays.includes(day.getDay())) continue;
      const cursor = new Date(day); cursor.setHours(fromHour, fromMinute, 0, 0);
      const end = new Date(day); end.setHours(toHour, toMinute, 0, 0);
      while (cursor.getTime() + duration * 60000 <= end.getTime()) {
        const slotEnd = new Date(cursor.getTime() + duration * 60000);
        requests.push({ startTime: cursor.toISOString(), endTime: slotEnd.toISOString() });
        cursor.setTime(slotEnd.getTime() + breakMinutes * 60000);
      }
    }
    if (requests.length) void onSave(requests);
  };
  return <Modal open title="Добавить свободное время" onClose={onClose}><div className="crm-modal__form"><div className="crm-form-grid"><label>С даты<input type="date" value={startDate} onChange={(event) => setStartDate(event.target.value)} /></label><label>По дату<input type="date" value={endDate} onChange={(event) => setEndDate(event.target.value)} /></label><label>С<input type="time" value={from} onChange={(event) => setFrom(event.target.value)} /></label><label>До<input type="time" value={to} onChange={(event) => setTo(event.target.value)} /></label><label>Длительность, мин<input type="number" min={1} value={duration} onChange={(event) => setDuration(Number(event.target.value))} /></label><label>Перерыв, мин<input type="number" min={0} value={breakMinutes} onChange={(event) => setBreakMinutes(Number(event.target.value))} /></label></div><fieldset className="crm-weekdays"><legend>Дни недели</legend>{['Вс', 'Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб'].map((label, day) => <label key={day}><input type="checkbox" checked={weekdays.includes(day)} onChange={() => toggleDay(day)} />{label}</label>)}</fieldset><div className="crm-modal__actions"><Button type="button" variant="tertiary" onClick={onClose}>Отмена</Button><Button type="button" loading={loading} onClick={save}>Создать интервалы</Button></div></div></Modal>;
}
