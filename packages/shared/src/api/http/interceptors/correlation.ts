import type { AxiosInstance } from 'axios';

export function addCorrelationInterceptor(client: AxiosInstance): void {
  client.interceptors.request.use((config) => {
    config.headers.set('X-Correlation-ID', crypto.randomUUID());
    return config;
  });
}
