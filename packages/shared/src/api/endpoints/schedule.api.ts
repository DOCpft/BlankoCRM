import { masterClient } from '../http/masterClient';
import type { CreateFreeSlotRequest, CreateOccupiedSlotRequest, ScheduleDto, UpdateSlotRequest } from '../types/schedule.types';

export const scheduleApi = {
  async getPeriod(dtFrom: string, dtTo: string): Promise<ScheduleDto[]> {
    const { data } = await masterClient.get<ScheduleDto[]>('/Schedule/get/period', {
      params: { dtFrom, dtTo },
    });
    return data;
  },

  async createFree(request: CreateFreeSlotRequest): Promise<ScheduleDto> {
    const { data } = await masterClient.post<ScheduleDto>('/Schedule/free', request);
    return data;
  },

  async createFreeBulk(requests: CreateFreeSlotRequest[]): Promise<ScheduleDto[]> {
    const { data } = await masterClient.post<ScheduleDto[]>('/Schedule/free/bulk', requests);
    return data;
  },

  async createOccupied(request: CreateOccupiedSlotRequest): Promise<ScheduleDto> {
    const { data } = await masterClient.post<ScheduleDto>('/Schedule/occupied', request);
    return data;
  },

  async update(id: string, request: UpdateSlotRequest): Promise<void> {
    await masterClient.put(`/Schedule/${id}`, request);
  },

  async delete(id: string): Promise<boolean> {
    const { data } = await masterClient.delete<boolean>(`/Schedule/${id}`);
    return data;
  },

  async deleteBulk(ids: string[]): Promise<number> {
    const { data } = await masterClient.delete<number>('/Schedule/delete/bulk', { data: ids });
    return data;
  },
};
