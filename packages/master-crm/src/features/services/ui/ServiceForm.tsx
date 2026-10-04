import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button, Input, Modal, Textarea } from '@blanko/shared';
import type { ServiceDto } from '@blanko/shared';
import { serviceSchema, type ServiceFormValues } from '../model/serviceSchema';

interface ServiceFormProps {
  service?: ServiceDto | null;
  loading: boolean;
  onClose: () => void;
  onSubmit: (values: ServiceFormValues) => Promise<void>;
  onDelete?: () => void;
}

export function ServiceForm({ service, loading, onClose, onSubmit, onDelete }: ServiceFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ServiceFormValues>({
    resolver: zodResolver(serviceSchema),
    defaultValues: {
      name: service?.name ?? '',
      durationMinutes: service?.durationMinutes ?? 60,
      price: service?.price ?? 0,
      description: service?.description ?? '',
    },
  });

  return (
    <Modal open title={service ? 'Карточка услуги' : 'Новая услуга'} onClose={onClose}>
      <form className="crm-modal__form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <Input {...register('name')} label="Название услуги" error={errors.name?.message} />
        <Input
          {...register('durationMinutes', { valueAsNumber: true })}
          label="Длительность, минут"
          type="number"
          min={1}
          max={1440}
          step={1}
          error={errors.durationMinutes?.message}
        />
        <Input
          {...register('price', { valueAsNumber: true })}
          label="Цена, ₽"
          type="number"
          min={0}
          max={999999.99}
          step="0.01"
          error={errors.price?.message}
        />
        <Textarea {...register('description')} label="Описание" rows={3} maxLength={500} error={errors.description?.message} />
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
