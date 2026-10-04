import axios from 'axios';
import { addCorrelationInterceptor } from './interceptors/correlation';
import { addErrorInterceptor } from './interceptors/error';

const apiOrigin = import.meta.env.VITE_PUBLIC_API_BASE_URL?.replace(/\/$/, '');

export const publicClient = axios.create({
  baseURL: apiOrigin ? `${apiOrigin}/api` : '/api',
  headers: { 'Content-Type': 'application/json' },
});

addCorrelationInterceptor(publicClient);
addErrorInterceptor(publicClient);
