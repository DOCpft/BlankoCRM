export interface Client {
  id: string;
  name: string;
  phone?: string;
  network: string;
}

export interface Service {
  id: string;
  name: string;
  price: number;
  duration: number;
  description?: string;
}

export interface Slot {
  id: string;
  startTime: Date;
  endTime: Date;
  clientId?: string;
  serviceId?: string;
  isFree: boolean;
  appointmentId?: string;
}
