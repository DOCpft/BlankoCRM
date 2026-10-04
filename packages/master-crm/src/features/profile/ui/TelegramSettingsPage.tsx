import { useState } from 'react';
import { Button, Card, Snackbar } from '@blanko/shared';
import { useGenerateTelegramLink, useTelegramStatus, useUnlinkTelegram } from '../../../entities/profile/api/profileQueries';

export function TelegramSettingsPage() {
  const status = useTelegramStatus();
  const generateLink = useGenerateTelegramLink();
  const unlink = useUnlinkTelegram();
  const [notice, setNotice] = useState<{ message: string; variant: 'success' | 'error' } | null>(null);
  const generate = async () => {
    setNotice(null);
    try {
      const result = await generateLink.mutateAsync();
      window.open(result.url, '_blank', 'noopener,noreferrer');
      setNotice({ message: 'Ссылка для привязки открыта. Перейдите в Telegram и запустите бота.', variant: 'success' });
    } catch {
      setNotice({ message: 'Не удалось создать ссылку. Попробуйте ещё раз.', variant: 'error' });
    }
  };
  const unlinkAccount = async () => {
    setNotice(null);
    try {
      await unlink.mutateAsync();
      setNotice({ message: 'Telegram-аккаунт отвязан', variant: 'success' });
    } catch {
      setNotice({ message: 'Не удалось отвязать Telegram-аккаунт', variant: 'error' });
    }
  };

  return <section className="crm-page">
    <header className="crm-page__header"><div><h1>Telegram</h1><p>Свяжите Telegram для получения уведомлений о записях</p></div></header>
    {status.isError && <p className="auth-error" role="alert">Не удалось загрузить статус Telegram.</p>}
    <Card className="crm-settings-card">
      <div className="crm-settings-card__body">
        <strong>{status.data?.linked ? 'Аккаунт привязан' : 'Аккаунт не привязан'}</strong>
        <span>{status.data?.linked ? 'Бот сможет отправлять уведомления в привязанный чат.' : 'Для привязки откройте бота и выполните команду запуска по ссылке.'}</span>
      </div>
      {status.data?.linked
        ? <Button variant="danger" loading={unlink.isPending} onClick={() => void unlinkAccount()}>Отвязать</Button>
        : <Button loading={generateLink.isPending || status.isLoading} disabled={status.isLoading} onClick={() => void generate()}>Создать ссылку</Button>}
    </Card>
    {status.data?.linked && <p className="crm-settings-note">Уведомления бота: {status.data.notificationsEnabled ? 'включены' : 'выключены'}.</p>}
    {notice && <Snackbar message={notice.message} variant={notice.variant} />}
  </section>;
}
