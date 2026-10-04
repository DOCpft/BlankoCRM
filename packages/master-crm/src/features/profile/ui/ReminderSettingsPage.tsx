import { useEffect, useState } from 'react';
import { Button, Snackbar } from '@blanko/shared';
import type { ReminderSettingDto } from '@blanko/shared';
import { useReminderSettings, useUpdateReminderSettings } from '../../../entities/profile/api/profileQueries';

const allowedOffsets = [1, 3, 6, 12, 24];

export function ReminderSettingsPage() {
  const settingsQuery = useReminderSettings();
  const update = useUpdateReminderSettings();
  const [settings, setSettings] = useState<ReminderSettingDto[]>([]);
  const [notice, setNotice] = useState<{ message: string; variant: 'success' | 'error' } | null>(null);

  useEffect(() => {
    if (settingsQuery.data) setSettings(settingsQuery.data);
  }, [settingsQuery.data]);

  const toggle = (offsetHours: number, enabled: boolean) => {
    setSettings((current) => current.map((setting) => setting.offsetHours === offsetHours ? { ...setting, enabled } : setting));
  };
  const save = async () => {
    setNotice(null);
    try {
      await update.mutateAsync({ settings });
      setNotice({ message: 'Настройки напоминаний сохранены', variant: 'success' });
    } catch {
      setNotice({ message: 'Не удалось сохранить настройки напоминаний', variant: 'error' });
    }
  };

  return <section className="crm-page">
    <header className="crm-page__header"><div><h1>Напоминания</h1><p>Выберите, за сколько часов отправлять напоминания клиенту</p></div></header>
    {settingsQuery.isLoading && <p role="status">Загрузка настроек…</p>}
    {settingsQuery.isError && <p className="auth-error" role="alert">Не удалось загрузить настройки напоминаний.</p>}
    {!settingsQuery.isLoading && !settingsQuery.isError && <>
      <div className="crm-settings-list">
        {allowedOffsets.map((offset) => {
          const setting = settings.find((item) => item.offsetHours === offset);
          return <label className="crm-settings-toggle" key={offset}>
            <span>За {offset} {offset === 1 ? 'час' : offset < 5 ? 'часа' : 'часов'}</span>
            <input type="checkbox" checked={setting?.enabled ?? false} disabled={!setting} onChange={(event) => toggle(offset, event.target.checked)} />
          </label>;
        })}
      </div>
      <Button loading={update.isPending} disabled={settings.length === 0} onClick={() => void save()}>Сохранить</Button>
    </>}
    {notice && <Snackbar message={notice.message} variant={notice.variant} />}
  </section>;
}
