import { z } from 'zod';

export const appointmentSchema = z.object({
  clientId: z.string().uuid('Выберите клиента'),
  services: z.array(z.object({
    serviceId: z.string().uuid('Выберите услугу'),
    quantity: z.number().int().min(1, 'Количество должно быть не меньше 1'),
  })).min(1, 'Добавьте хотя бы одну услугу'),
  startTime: z.string().min(1, 'Укажите время начала'),
  notes: z.string().max(500, 'Заметки не должны превышать 500 символов'),
});

export type AppointmentFormValues = z.infer<typeof appointmentSchema>;
