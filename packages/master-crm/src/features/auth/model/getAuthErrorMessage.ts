import { ApiError } from '@blanko/shared';

export function getAuthErrorMessage(error: unknown): string {
  if (error instanceof ApiError) return error.message;
  return 'Не удалось выполнить запрос. Проверьте подключение к интернету и попробуйте ещё раз.';
}
