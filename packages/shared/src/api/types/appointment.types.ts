export enum AppointmentStatus {
  Scheduled = 'Scheduled',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  NoShow = 'NoShow',
  Archived = 'Archived',
  Pending = 'Pending',
  Rejected = 'Rejected',
  Withdrawn = 'Withdrawn',
}

export interface AppointmentServiceDto {
  serviceId: string;
  serviceName: string | null;
  price: number;
  durationMinutes: number;
  quantity: number;
}

export interface AppointmentDto {
  id: string;
  clientId: string;
  clientName: string;
  services: AppointmentServiceDto[];
  startTime: string;
  endTime: string;
  status: AppointmentStatus;
  notes: string | null;
  totalPrice: number;
  totalDurationMinutes: number;
}

export interface ServiceInput {
  serviceId: string;
  durationMinutes?: number | null;
  price?: number | null;
  quantity?: number | null;
}

export interface CreateAppointmentRequest {
  clientId: string;
  services: ServiceInput[];
  startTime: string;
  notes?: string | null;
  salonId?: string | null;
}

export interface UpdateAppointmentRequest {
  clientId?: string | null;
  services?: ServiceInput[] | null;
  startTime?: string | null;
  endTime?: string | null;
  notes?: string | null;
  salonId?: string | null;
}
