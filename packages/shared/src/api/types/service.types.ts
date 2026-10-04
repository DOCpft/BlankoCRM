export interface ServiceDto {
  id: string;
  name: string;
  durationMinutes: number;
  price: number;
  description: string | null;
  salonId: string | null;
}

export interface CreateServiceRequest {
  name: string;
  durationMinutes: number;
  price: number;
  description: string | null;
  salonId?: string | null;
}

export type UpdateServiceRequest = Partial<CreateServiceRequest>;
