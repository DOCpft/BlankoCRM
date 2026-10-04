import type { AxiosInstance } from 'axios';

function readToken(): string | null {
  try {
    const raw: unknown = JSON.parse(localStorage.getItem('blanko-crm-auth') ?? 'null');
    if (typeof raw !== 'object' || raw === null || !('state' in raw)) return null;
    const state = raw.state;
    if (typeof state !== 'object' || state === null || !('token' in state)) return null;
    return typeof state.token === 'string' ? state.token : null;
  } catch {
    return null;
  }
}

export function addAuthInterceptor(client: AxiosInstance): void {
  client.interceptors.request.use((config) => {
    const token = readToken();
    if (token) config.headers.set('Authorization', `Bearer ${token}`);
    return config;
  });

  client.interceptors.response.use((response) => response, (error: unknown) => {
    const requestUrl =
      typeof error === 'object' && error !== null && 'config' in error &&
      typeof error.config === 'object' && error.config !== null && 'url' in error.config &&
      typeof error.config.url === 'string'
        ? error.config.url.toLowerCase()
        : '';
    if (
      !requestUrl.includes('/auth/') &&
      typeof error === 'object' && error !== null && 'response' in error &&
      typeof error.response === 'object' && error.response !== null &&
      'status' in error.response && error.response.status === 401
    ) {
      window.dispatchEvent(new Event('blanko:unauthorized'));
    }
    return Promise.reject(error);
  });
}
