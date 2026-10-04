import { useEffect, useMemo, useState } from 'react';
import type { FormEvent } from 'react';
import { QueryClientProvider } from '@tanstack/react-query';
import { createHashRouter, RouterProvider } from 'react-router-dom';
import { AppointmentStatus, Button, Card, Input, Modal, Snackbar, Textarea, getApiErrorMessage, queryClient, vkService } from '@blanko/shared';
import type { AppointmentDto, PublicFreeSlotDto, PublicServiceDto } from '@blanko/shared';
import { useCancelMyPublicAppointment, useCreatePublicAppointment, useMyPublicAppointments, usePublicServices, usePublicSlots } from './entities/public-booking/api/publicBookingQueries';
import './client-app.css';

function organizationSlugFromUrl(): string {
  const querySlug = new URLSearchParams(window.location.search).get('slug');
  if (querySlug?.trim()) return querySlug.trim().toLowerCase();

  for (const candidate of [window.location.pathname, window.location.hash.slice(1)]) {
    const segments = candidate.split('/').filter(Boolean);
    if (segments[0] === 'booking' && segments[1]) return decodeURIComponent(segments[1]).toLowerCase();
    if (segments[0] && segments[0] !== 'index.html') return decodeURIComponent(segments[0]).toLowerCase();
  }
  return '';
}

function toDateInputValue(date: Date): string {
  return new Date(date.getTime() - date.getTimezoneOffset() * 60000).toISOString().slice(0, 10);
}

function BookingPage({ slug }: { slug: string }) {
  const vkParams = useMemo(() => Object.fromEntries(new URLSearchParams(window.location.search).entries()), []);
  const [view, setView] = useState<'booking' | 'mine'>('booking');
  const [selectedService, setSelectedService] = useState<PublicServiceDto | null>(null);
  const [selectedSlot, setSelectedSlot] = useState<PublicFreeSlotDto | null>(null);
  const [date, setDate] = useState(() => toDateInputValue(new Date()));
  const [name, setName] = useState('');
  const [phone, setPhone] = useState('');
  const [notes, setNotes] = useState('');
  const [notice, setNotice] = useState<{ message: string; variant: 'success' | 'error' } | null>(null);
  const from = useMemo(() => new Date(`${date}T00:00:00`).toISOString(), [date]);
  const to = useMemo(() => new Date(new Date(`${date}T00:00:00`).getTime() + 86400000).toISOString(), [date]);
  const servicesQuery = usePublicServices(slug);
  const slotsQuery = usePublicSlots(slug, from, to);
  const mineRequest = useMemo(() => ({ vkParams }), [vkParams]);
  const mineQuery = useMyPublicAppointments(slug, mineRequest);
  const createAppointment = useCreatePublicAppointment(slug);
  const cancelAppointment = useCancelMyPublicAppointment(slug, mineRequest);
  const services = servicesQuery.data ?? [];
  const slots = slotsQuery.data ?? [];

  useEffect(() => {
    if (!vkParams.vk_user_id) return;
    try {
      const profile = JSON.parse(localStorage.getItem('blanko-client-profile') ?? 'null') as { name?: string; phone?: string } | null;
      if (profile) {
        setName(profile.name ?? '');
        setPhone(profile.phone ?? '');
      }
    } catch {
      localStorage.removeItem('blanko-client-profile');
    }
  }, [vkParams.vk_user_id]);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedService || !selectedSlot) return;
    try {
      await createAppointment.mutateAsync({
        freeSlotId: selectedSlot.id,
        vkParams,
        name: name.trim(),
        phone: phone.trim() || null,
        services: [{ serviceId: selectedService.id, quantity: 1 }],
        startTime: selectedSlot.startTime,
        notes: notes.trim() || null,
      });
      localStorage.setItem('blanko-client-profile', JSON.stringify({ name: name.trim(), phone: phone.trim() }));
      setSelectedSlot(null);
      setSelectedService(null);
      setNotes('');
      setView('mine');
      setNotice({ message: 'Заявка отправлена мастеру. Дождитесь подтверждения.', variant: 'success' });
    } catch (error: unknown) {
      setNotice({ message: getApiErrorMessage(error), variant: 'error' });
      void slotsQuery.refetch();
    }
  };

  const cancel = async (appointment: AppointmentDto) => {
    try {
      const pending = appointment.status === AppointmentStatus.Pending;
      await cancelAppointment.mutateAsync({ id: appointment.id, pending });
      setNotice({ message: pending ? 'Заявка отозвана' : 'Запись отменена', variant: 'success' });
    } catch (error: unknown) {
      setNotice({ message: getApiErrorMessage(error), variant: 'error' });
    }
  };

  const formatTime = (value: string) => new Date(value).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });

  return (
    <main className="client-app">
      <header className="client-header"><h1>Запись к мастеру</h1></header>
      <nav className="client-tabs" aria-label="Разделы записи">
        <Button variant={view === 'booking' ? 'primary' : 'secondary'} onClick={() => setView('booking')}>Записаться</Button>
        <Button variant={view === 'mine' ? 'primary' : 'secondary'} onClick={() => setView('mine')}>Мои записи</Button>
      </nav>
      {view === 'booking' ? <section className="client-page">
        <p>Выберите услугу и время. Заявку должен подтвердить мастер.</p>
        <Input label="Дата" type="date" min={toDateInputValue(new Date())} value={date} onChange={(event) => { setDate(event.target.value); setSelectedSlot(null); }} />
        {servicesQuery.isLoading && <p role="status">Загружаем услуги…</p>}
        {servicesQuery.isError && <p className="auth-error" role="alert">Не удалось загрузить каталог услуг.</p>}
        <div className="client-service-list">{services.map((service) => <Card key={service.id} className={`client-service${selectedService?.id === service.id ? ' client-service--selected' : ''}`}><button type="button" className="client-service__select" onClick={() => { setSelectedService(service); setSelectedSlot(null); }}><strong>{service.name}</strong><span>{service.durationMinutes} мин · {service.price.toLocaleString('ru-RU')} ₽</span>{service.description && <small>{service.description}</small>}</button></Card>)}</div>
        {selectedService && <section className="client-slots"><h2>Свободное время</h2>{slotsQuery.isLoading && <p role="status">Ищем свободные интервалы…</p>}{slotsQuery.isError && <p className="auth-error" role="alert">Не удалось загрузить свободные интервалы.</p>}{!slotsQuery.isLoading && slots.length === 0 && <p>На эту дату нет доступного времени. Выберите другой день.</p>}<div className="client-slot-list">{slots.map((slot) => <Button key={slot.id} variant={selectedSlot?.id === slot.id ? 'primary' : 'secondary'} onClick={() => setSelectedSlot(slot)}>{formatTime(slot.startTime)}</Button>)}</div></section>}
      </section> : <section className="client-page">
        <h2>Мои записи</h2>
        {!vkParams.vk_user_id || !vkParams.sign ? <p>Откройте приложение по ссылке мастера внутри VK, чтобы увидеть записи.</p> : <>
          {mineQuery.isLoading && <p role="status">Загружаем ваши записи…</p>}
          {mineQuery.isError && <p className="auth-error" role="alert">Не удалось загрузить записи. Повторите попытку.</p>}
          {!mineQuery.isLoading && (mineQuery.data ?? []).length === 0 && <p>Активных записей пока нет.</p>}
          <div className="client-booking-list">{(mineQuery.data ?? []).map((appointment) => <Card key={appointment.id} className="client-booking-card"><div className="client-booking-card__body"><strong>{appointment.services.map((service) => service.serviceName ?? 'Услуга').join(', ')}</strong><span>{new Date(appointment.startTime).toLocaleString('ru-RU', { dateStyle: 'medium', timeStyle: 'short' })}</span><span>{appointment.status === AppointmentStatus.Pending ? 'Ожидает подтверждения' : appointment.status === AppointmentStatus.Scheduled ? 'Подтверждена' : appointment.status}</span></div>{[AppointmentStatus.Pending, AppointmentStatus.Scheduled].includes(appointment.status) && <Button variant="tertiary" loading={cancelAppointment.isPending} onClick={() => void cancel(appointment)}>{appointment.status === AppointmentStatus.Pending ? 'Отозвать заявку' : 'Отменить запись'}</Button>}</Card>)}</div>
        </>}
      </section>}
      {selectedSlot && selectedService && <Modal open title="Оставить заявку" onClose={() => setSelectedSlot(null)}><form className="crm-modal__form" onSubmit={(event) => void submit(event)}><p>{selectedService.name} · {new Date(selectedSlot.startTime).toLocaleString('ru-RU', { dateStyle: 'medium', timeStyle: 'short' })}</p><Input label="Ваше имя" autoComplete="name" maxLength={200} required value={name} onChange={(event) => setName(event.target.value)} /><Input label="Телефон (необязательно)" type="tel" autoComplete="tel" maxLength={40} value={phone} onChange={(event) => setPhone(event.target.value)} /><Textarea label="Комментарий (необязательно)" rows={3} maxLength={500} value={notes} onChange={(event) => setNotes(event.target.value)} /><p className="client-booking-disclaimer">Заявка ждёт подтверждения мастера; слот до подтверждения не резервируется.</p><div className="crm-modal__actions"><Button type="button" variant="tertiary" onClick={() => setSelectedSlot(null)}>Назад</Button><Button type="submit" loading={createAppointment.isPending} disabled={!name.trim()}>Отправить заявку</Button></div></form></Modal>}
      {notice && <div className="client-notice"><Snackbar message={notice.message} variant={notice.variant} /><Button variant="tertiary" ariaLabel="Закрыть сообщение" onClick={() => setNotice(null)}>×</Button></div>}
    </main>
  );
}

function MissingSlugPage() {
  return <main className="client-app"><section className="client-missing-slug"><h1>Запросите ссылку на запись у мастера</h1></section></main>;
}

const slug = organizationSlugFromUrl();
const router = createHashRouter([{ path: '*', element: slug ? <BookingPage slug={slug} /> : <MissingSlugPage /> }]);

export function ClientApp() {
  useEffect(() => {
    if (vkService.isAvailable()) void vkService.init();
  }, []);

  return <QueryClientProvider client={queryClient}><RouterProvider router={router} /></QueryClientProvider>;
}
