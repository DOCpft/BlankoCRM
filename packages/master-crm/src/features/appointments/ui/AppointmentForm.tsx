import { useFieldArray, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button, Input, Modal, Select, Textarea } from '@blanko/shared';
import type { AppointmentDto, ClientDto, ServiceDto } from '@blanko/shared';
import { getDateTimeInputValue, parseDateTimeInputValue } from '../../../utils/dateUtils';
import { appointmentSchema, type AppointmentFormValues } from '../model/appointmentSchema';

interface AppointmentFormProps {
  clients: ClientDto[];
  services: ServiceDto[];
  appointment?: AppointmentDto | null;
  initialStart?: Date;
  loading: boolean;
  onClose: () => void;
  onSubmit: (values: AppointmentFormValues) => Promise<void>;
  onDelete?: () => void;
}

export function AppointmentForm({ clients, services, appointment, initialStart, loading, onClose, onSubmit, onDelete }: AppointmentFormProps) {
  const { register, control, handleSubmit, formState: { errors } } = useForm<AppointmentFormValues>({
    resolver: zodResolver(appointmentSchema),
    defaultValues: {
      clientId: appointment?.clientId ?? '',
      services: appointment?.services.map(({ serviceId, quantity }) => ({ serviceId, quantity })) ?? [{ serviceId: '', quantity: 1 }],
      startTime: getDateTimeInputValue(appointment ? new Date(appointment.startTime) : initialStart ?? new Date()),
      notes: appointment?.notes ?? '',
    },
  });
  const serviceFields = useFieldArray({ control, name: 'services' });

  return (
    <Modal open title={appointment ? 'Запись клиента' : 'Новая запись'} onClose={onClose}>
      <form className="crm-modal__form" onSubmit={handleSubmit(async (values) => onSubmit({ ...values, startTime: parseDateTimeInputValue(values.startTime).toISOString() }))} noValidate>
        <Select {...register('clientId')} label="Клиент" options={clients.map((client) => ({ value: client.id, label: client.name }))} error={errors.clientId?.message} />
        <Input {...register('startTime')} label="Начало" type="datetime-local" error={errors.startTime?.message} />
        {serviceFields.fields.map((field, index) => (
          <div className="crm-contact-row" key={field.id}>
            <Select {...register(`services.${index}.serviceId`)} label="Услуга" options={services.map((service) => ({ value: service.id, label: `${service.name} · ${service.durationMinutes} мин.` }))} error={errors.services?.[index]?.serviceId?.message} />
            <Input {...register(`services.${index}.quantity`, { valueAsNumber: true })} label="Кол-во" type="number" min={1} error={errors.services?.[index]?.quantity?.message} />
            {serviceFields.fields.length > 1 && <Button type="button" variant="tertiary" ariaLabel="Удалить услугу" onClick={() => serviceFields.remove(index)}>×</Button>}
          </div>
        ))}
        {errors.services?.message && <p className="auth-error" role="alert">{errors.services.message}</p>}
        <Button type="button" variant="secondary" onClick={() => serviceFields.append({ serviceId: '', quantity: 1 })}>Добавить услугу</Button>
        <Textarea {...register('notes')} label="Заметки" rows={3} maxLength={500} error={errors.notes?.message} />
        <div className="crm-modal__actions">
          {onDelete && <Button type="button" variant="danger" disabled={loading} onClick={onDelete}>Удалить запись</Button>}
          <Button type="submit" loading={loading}>Сохранить</Button>
        </div>
      </form>
    </Modal>
  );
}
