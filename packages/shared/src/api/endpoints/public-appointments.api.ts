import { publicClient } from '../http/publicClient';
import type { AppointmentDto } from '../types/appointment.types';
import type { CreatePublicAppointmentRequest, PublicFreeSlotDto, PublicServiceDto } from '../types/public-appointment.types';
import type { VkMiniAppAuthRequest } from '../types/auth.types';

const basePath = (organizationSlug: string) => `/public/${encodeURIComponent(organizationSlug)}`;

export const publicAppointmentsApi = {
  async getServices(organizationSlug: string): Promise<PublicServiceDto[]> {
    const { data } = await publicClient.get<PublicServiceDto[]>(`${basePath(organizationSlug)}/services`);
    return data;
  },

  async getSlots(organizationSlug: string, from: string, to: string): Promise<PublicFreeSlotDto[]> {
    const { data } = await publicClient.get<PublicFreeSlotDto[]>(`${basePath(organizationSlug)}/appointments/slots`, {
      params: { from, to, limit: 500 },
    });
    return data;
  },

  async create(organizationSlug: string, request: CreatePublicAppointmentRequest): Promise<AppointmentDto> {
    const { data } = await publicClient.post<AppointmentDto>(`${basePath(organizationSlug)}/appointments`, request);
    return data;
  },

  async getMine(organizationSlug: string, request: VkMiniAppAuthRequest): Promise<AppointmentDto[]> {
    const { data } = await publicClient.post<AppointmentDto[]>(`${basePath(organizationSlug)}/appointments/mine`, request);
    return data;
  },

  async withdraw(organizationSlug: string, id: string, request: VkMiniAppAuthRequest): Promise<void> {
    await publicClient.post(`${basePath(organizationSlug)}/appointments/${encodeURIComponent(id)}/withdraw`, request);
  },

  async cancel(organizationSlug: string, id: string, request: VkMiniAppAuthRequest): Promise<void> {
    await publicClient.post(`${basePath(organizationSlug)}/appointments/${encodeURIComponent(id)}/cancel`, request);
  },
};