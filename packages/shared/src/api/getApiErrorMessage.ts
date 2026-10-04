import { ApiError } from './ApiError';

export function getApiErrorMessage(error: unknown): string {
  if (error instanceof ApiError) return error.message;
  return 'Не удалось выполнить запрос. Проверьте подключение к интернету и попробуйте ещё раз.';
}
