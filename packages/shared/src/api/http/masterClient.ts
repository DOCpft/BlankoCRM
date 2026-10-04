import axios from 'axios';
import { addAuthInterceptor } from './interceptors/auth';
import { addCorrelationInterceptor } from './interceptors/correlation';
import { addErrorInterceptor } from './interceptors/error';

export const masterClient = axios.create({
  baseURL: '/api',
  headers: { 'Content-Type': 'application/json' },
});

addAuthInterceptor(masterClient);
addCorrelationInterceptor(masterClient);
addErrorInterceptor(masterClient);
