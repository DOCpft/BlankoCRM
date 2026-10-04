import { z } from 'zod';

export const serviceSchema = z.object({
  name: z.string().trim().min(1, 'Название обязательно').max(100, 'Название не должно превышать 100 символов'),
  durationMinutes: z.number().int('Введите целое число минут').min(1, 'Минимум 1 минута').max(1440, 'Максимум 1440 минут'),
  price: z.number().min(0, 'Цена не может быть отрицательной').max(999999.99, 'Максимальная цена 999999,99 ₽'),
  description: z.string().max(500, 'Описание не должно превышать 500 символов'),
});

export type ServiceFormValues = z.infer<typeof serviceSchema>;
