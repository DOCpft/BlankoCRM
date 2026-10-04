export enum ContactType {
  Telegram = 'Telegram',
  Vk = 'Vk',
  Instagram = 'Instagram',
  WhatsApp = 'WhatsApp',
  Viber = 'Viber',
  MAX = 'MAX',
}

export interface ClientContactDto {
  type: ContactType;
  value: string;
}

export interface ClientDto {
  id: string;
  name: string;
  phone: string | null;
  contacts: ClientContactDto[];
  birthDate: string | null;
  notes: string | null;
  salonId: string | null;
}

export interface CreateClientRequest {
  name: string;
  phone: string | null;
  contacts: ClientContactDto[];
  birthDate: string | null;
  notes: string | null;
  salonId?: string | null;
}

export type UpdateClientRequest = Partial<CreateClientRequest>;
