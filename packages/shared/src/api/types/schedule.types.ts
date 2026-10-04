import type { AppointmentDto, ServiceInput } from './appointment.types';

export interface ScheduleDto {
  id: string;
  startTime: string;
  endTime: string;
  appointment: AppointmentDto | null;
}

export interface CreateFreeSlotRequest {
  startTime: string;
  endTime: string;
  salonId?: string | null;
}

export interface CreateOccupiedSlotRequest {
  startTime: string;
  endTime?: string | null;
  appointmentData: {
    clientId: string;
    services: ServiceInput[];
    notes?: string | null;
    salonId?: string | null;
  };
}

export interface UpdateSlotRequest {
  startTime?: string | null;
  endTime?: string | null;
  appointmentData?: {
    clientId?: string | null;
    services?: ServiceInput[] | null;
    notes?: string | null;
    salonId?: string | null;
  } | null;
}
