import { masterClient } from '../http/masterClient';
import type { AppointmentDto, CreateAppointmentRequest, UpdateAppointmentRequest } from '../types/appointment.types';

export const appointmentsApi = {
  async getAll(): Promise<AppointmentDto[]> {
    const { data } = await masterClient.get<AppointmentDto[]>('/Appointments/all');
    return data;
  },

  async getForDay(date: string): Promise<AppointmentDto[]> {
    const { data } = await masterClient.get<AppointmentDto[]>('/Appointments/calendar', { params: { date } });
    return data;
  },

  async getPending(): Promise<AppointmentDto[]> {
    const { data } = await masterClient.get<AppointmentDto[]>('/Appointments/pending');
    return data;
  },

  async getById(id: string): Promise<AppointmentDto> {
    const { data } = await masterClient.get<AppointmentDto>(`/Appointments/${id}`);
    return data;
  },

  async create(request: CreateAppointmentRequest): Promise<AppointmentDto> {
    const { data } = await masterClient.post<AppointmentDto>('/Appointments', request);
    return data;
  },

  async update(id: string, request: UpdateAppointmentRequest): Promise<void> {
    await masterClient.put(`/Appointments/${id}`, request);
  },

  async delete(id: string): Promise<void> {
    await masterClient.delete(`/Appointments/${id}`);
  },

  async cancel(id: string): Promise<void> {
    await masterClient.patch(`/Appointments/${id}/cancel`);
  },

  async confirm(id: string): Promise<void> {
    await masterClient.post(`/Appointments/${id}/confirm`);
  },

  async reject(id: string): Promise<void> {
    await masterClient.post(`/Appointments/${id}/reject`);
  },

  async complete(id: string): Promise<void> {
    await masterClient.post(`/Appointments/${id}/complete`);
  },

  async noShow(id: string): Promise<void> {
    await masterClient.post(`/Appointments/${id}/no-show`);
  },
};
