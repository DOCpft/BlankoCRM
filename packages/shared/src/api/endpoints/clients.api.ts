import { masterClient } from '../http/masterClient';
import type { ClientDto, CreateClientRequest, UpdateClientRequest } from '../types/client.types';

export const clientsApi = {
  async getAll(search?: string): Promise<ClientDto[]> {
    const { data } = await masterClient.get<ClientDto[]>('/Clients/all', { params: { search } });
    return data;
  },

  async getById(id: string): Promise<ClientDto> {
    const { data } = await masterClient.get<ClientDto>(`/Clients/${id}`);
    return data;
  },

  async create(request: CreateClientRequest): Promise<ClientDto> {
    const { data } = await masterClient.post<ClientDto>('/Clients/create', request);
    return data;
  },

  async update(id: string, request: UpdateClientRequest): Promise<void> {
    await masterClient.put(`/Clients/update/${id}`, request);
  },

  async delete(id: string): Promise<void> {
    await masterClient.delete(`/Clients/delete/${id}`);
  },
};
