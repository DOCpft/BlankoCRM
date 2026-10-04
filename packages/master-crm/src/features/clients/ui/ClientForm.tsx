import { useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button, ContactType, Input, Modal, Select, Textarea } from '@blanko/shared';
import type { ClientDto } from '@blanko/shared';
import { clientSchema, type ClientFormValues } from '../model/clientSchema';

const contactLabels: Record<ContactType, string> = {
  [ContactType.Telegram]: 'Telegram',
  [ContactType.Vk]: 'VK',
  [ContactType.Instagram]: 'Instagram',
  [ContactType.WhatsApp]: 'WhatsApp',
  [ContactType.Viber]: 'Viber',
  [ContactType.MAX]: 'MAX',
};

const contactOptions = Object.values(ContactType)
  .filter((type) => type !== ContactType.MAX)
  .map((type) => ({ value: type, label: contactLabels[type] }));

interface ClientFormProps {
  client?: ClientDto | null;
  loading: boolean;
  onClose: () => void;
  onSubmit: (values: ClientFormValues) => Promise<void>;
  onDelete?: () => void;
}

export function ClientForm({ client, loading, onClose, onSubmit, onDelete }: ClientFormProps) {
  const {
    register,
    control,
    handleSubmit,
    formState: { errors },
  } = useForm<ClientFormValues>({
    resolver: zodResolver(clientSchema),
    defaultValues: {
      name: client?.name ?? '',
      phone: client?.phone ?? '',
      contacts: client?.contacts ?? [],
      birthDate: client?.birthDate?.slice(0, 10) ?? '',
      notes: client?.notes ?? '',
    },
  });
  const contacts = useFieldArray({ control, name: 'contacts' });

  return (
    <Modal open title={client ? 'Карточка клиента' : 'Новый клиент'} onClose={onClose}>
      <form className="crm-modal__form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Input {...register('name')} label="Имя" autoComplete="name" error={errors.name?.message} />
        <Input {...register('phone')} label="Телефон" type="tel" autoComplete="tel" error={errors.phone?.message} />
        <Input {...register('birthDate')} label="Дата рождения" type="date" error={errors.birthDate?.message} />
        <section className="crm-contact-list" aria-label="Контакты клиента">
          <h3>Контакты</h3>
          {contacts.fields.map((contact, index) => (
            <div className="crm-contact-row" key={contact.id}>
              <Select
                {...register(`contacts.${index}.type`)}
                label="Сервис"
                options={contactOptions}
                error={errors.contacts?.[index]?.type?.message}
              />
              <Input
                {...register(`contacts.${index}.value`)}
                label="Контакт"
                error={errors.contacts?.[index]?.value?.message}
              />
              <Button
                type="button"
                variant="tertiary"
                ariaLabel="Удалить контакт"
                onClick={() => contacts.remove(index)}
              >
                ×
              </Button>
            </div>
          ))}
          {errors.contacts?.message && <p className="auth-error" role="alert">{errors.contacts.message}</p>}
          <Button
            type="button"
            variant="secondary"
            onClick={() => contacts.append({ type: ContactType.Telegram, value: '' })}
          >
            Добавить контакт
          </Button>
        </section>
        <Textarea {...register('notes')} label="Заметки" rows={3} maxLength={500} error={errors.notes?.message} />
        <div className="crm-modal__actions">
          {onDelete && (
            <Button type="button" variant="danger" disabled={loading} onClick={onDelete}>
              Удалить
            </Button>
          )}
          <Button type="submit" loading={loading}>
            Сохранить
          </Button>
        </div>
      </form>
    </Modal>
  );
}
