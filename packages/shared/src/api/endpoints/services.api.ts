import { masterClient } from '../http/masterClient';
import type { CreateServiceRequest, ServiceDto, UpdateServiceRequest } from '../types/service.types';

export const servicesApi = {
  async getAll(): Promise<ServiceDto[]> {
    const { data } = await masterClient.get<ServiceDto[]>('/Services');
    return data;
  },

  async getById(id: string): Promise<ServiceDto> {
    const { data } = await masterClient.get<ServiceDto>(`/Services/${id}`);
    return data;
  },

  async create(request: CreateServiceRequest): Promise<ServiceDto> {
    const { data } = await masterClient.post<ServiceDto>('/Services', request);
    return data;
  },

  async update(id: string, request: UpdateServiceRequest): Promise<void> {
    await masterClient.put(`/Services/${id}`, request);
  },

  async delete(id: string): Promise<void> {
    await masterClient.delete(`/Services/${id}`);
  },
};
