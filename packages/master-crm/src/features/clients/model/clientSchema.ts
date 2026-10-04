import { z } from 'zod';
import { ContactType } from '@blanko/shared';

const contactSchema = z.object({
  type: z.nativeEnum(ContactType).refine((type) => type !== ContactType.MAX, 'Выберите тип контакта'),
  value: z.string().trim().min(1, 'Укажите контакт').max(200, 'Контакт не должен превышать 200 символов'),
});

export const clientSchema = z.object({
  name: z.string().trim().min(1, 'Имя обязательно').max(200, 'Имя не должно превышать 200 символов'),
  phone: z
    .string()
    .trim()
    .max(20, 'Телефон не должен превышать 20 символов')
    .refine((value) => !value || /^\+?[\d\s()-]+$/.test(value), 'Некорректный формат телефона'),
  contacts: z.array(contactSchema).refine(
    (contacts) => new Set(contacts.map(({ type }) => type)).size === contacts.length,
    'Типы контактов не должны повторяться',
  ),
  birthDate: z.string(),
  notes: z.string().max(500, 'Заметки не должны превышать 500 символов'),
});

export type ClientFormValues = z.infer<typeof clientSchema>;
