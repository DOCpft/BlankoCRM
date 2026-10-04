import { useState } from 'react';
import { Button, Input, Snackbar } from '@blanko/shared';
import { useUpdateTimeZone } from '../../../entities/profile/api/profileQueries';

export function TimezoneSettingsPage() {
  const [timeZone, setTimeZone] = useState('');
  const [notice, setNotice] = useState<{ message: string; variant: 'success' | 'error' } | null>(null);
  const update = useUpdateTimeZone();
  const save = async () => {
    setNotice(null);
    try {
      await update.mutateAsync({ timeZone: timeZone.trim() });
      setNotice({ message: 'Часовой пояс обновлён', variant: 'success' });
      setTimeZone('');
    } catch {
      setNotice({ message: 'Не удалось обновить часовой пояс. Проверьте IANA-идентификатор.', variant: 'error' });
    }
  };

  return <section className="crm-page">
    <header className="crm-page__header"><div><h1>Часовой пояс</h1><p>Укажите часовой пояс для расписания и напоминаний</p></div></header>
    <p className="crm-settings-note">Текущий часовой пояс не возвращается API. Чтобы изменить его, введите IANA-идентификатор, например Europe/Moscow или Asia/Almaty.</p>
    <form className="crm-settings-form" onSubmit={(event) => { event.preventDefault(); void save(); }}>
      <Input label="IANA-идентификатор" value={timeZone} onChange={(event) => setTimeZone(event.target.value)} placeholder="Europe/Moscow" autoComplete="off" />
      <Button type="submit" loading={update.isPending} disabled={!timeZone.trim()}>Сохранить часовой пояс</Button>
    </form>
    {notice && <Snackbar message={notice.message} variant={notice.variant} />}
  </section>;
}
