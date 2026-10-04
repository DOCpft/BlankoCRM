import axios from 'axios';
import type { AxiosInstance } from 'axios';
import { ApiError } from '../../ApiError';

function getErrorMessage(data: unknown): string | undefined {
  if (typeof data !== 'object' || data === null) return undefined;
  if ('detail' in data && typeof data.detail === 'string') return data.detail;
  if ('title' in data && typeof data.title === 'string') return data.title;
  if ('message' in data && typeof data.message === 'string') return data.message;
  return undefined;
}

export function addErrorInterceptor(client: AxiosInstance): void {
  client.interceptors.response.use(
    (response) => response,
    (error: unknown) => {
      if (axios.isAxiosError(error)) {
        const status = error.response?.status ?? 0;
        const message = error.response
          ? getErrorMessage(error.response.data) ?? error.message
          : 'Сетевая ошибка';
        return Promise.reject(new ApiError(status, message, error.response?.data));
      }
      return Promise.reject(error);
    },
  );
}
