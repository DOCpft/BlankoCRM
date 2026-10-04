import { z } from 'zod';

const emailSchema = z
  .string()
  .trim()
  .min(1, 'Email обязателен')
  .email('Введите корректный email')
  .max(100, 'Email не должен превышать 100 символов');

export const loginSchema = z.object({
  email: emailSchema,
  password: z.string().min(1, 'Пароль обязателен').max(100, 'Пароль не должен превышать 100 символов'),
});

export const registerSchema = z.object({
  name: z.string().trim().max(100, 'Имя не должно превышать 100 символов'),
  organizationName: z.string().trim().max(100, 'Название организации не должно превышать 100 символов'),
  email: emailSchema,
  password: z
    .string()
    .min(6, 'Пароль должен содержать не менее 6 символов')
    .max(100, 'Пароль не должен превышать 100 символов')
    .refine((password) => /\p{Lu}/u.test(password), 'Добавьте заглавную букву')
    .refine((password) => /\p{Ll}/u.test(password), 'Добавьте строчную букву')
    .refine((password) => /\p{Nd}/u.test(password), 'Добавьте цифру')
    .refine((password) => /[^\p{L}\p{N}]/u.test(password), 'Добавьте специальный символ'),
});

export type LoginFormValues = z.infer<typeof loginSchema>;
export type RegisterFormValues = z.infer<typeof registerSchema>;
