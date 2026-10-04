import type { AppointmentDto } from './appointment.types';

export interface PublicFreeSlotDto {
  id: string;
  startTime: string;
  endTime: string;
}

export interface PublicServiceDto {
  id: string;
  name: string;
  durationMinutes: number;
  price: number;
  description: string | null;
  salonId: string | null;
}

export interface PublicServiceInput {
  serviceId: string;
  quantity: number;
}

export interface CreatePublicAppointmentRequest {
  freeSlotId: string;
  vkParams: Record<string, string>;
  name: string;
  phone: string | null;
  services: PublicServiceInput[];
  startTime: string;
  notes: string | null;
}

export type CreatePublicAppointmentResponse = AppointmentDto;