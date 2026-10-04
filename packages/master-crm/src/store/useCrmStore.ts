import { create } from 'zustand';

import { Client, Service, Slot } from '../types';
import { getWeekRange } from '../utils/dateUtils';

interface CrmStore {
  clients: Client[];
  services: Service[];
  slots: Slot[];
  addClient: (client: Omit<Client, 'id'>) => void;
  updateClient: (id: string, updated: Partial<Client>) => void;
  deleteClient: (id: string) => void;
  addService: (service: Omit<Service, 'id'>) => void;
  updateService: (id: string, updated: Partial<Service>) => void;
  deleteService: (id: string) => void;
  addSlot: (slot: Omit<Slot, 'id'>) => void;
  updateSlot: (id: string, updated: Partial<Slot>) => void;
  deleteSlot: (id: string) => void;
  getSlotsForWeek: (startDate: Date) => Slot[];
}

const createEntityId = (): string => {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }

  return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
};

export const useCrmStore = create<CrmStore>((set, get) => ({
  clients: [],
  services: [],
  slots: [],

  addClient: (client) =>
    set((state) => ({
      clients: [...state.clients, { ...client, id: createEntityId() }],
    })),

  updateClient: (id, updated) =>
    set((state) => ({
      clients: state.clients.map((client) =>
        client.id === id ? { ...client, ...updated, id: client.id } : client,
      ),
    })),

  deleteClient: (id) =>
    set((state) => ({
      clients: state.clients.filter((client) => client.id !== id),
    })),

  addService: (service) =>
    set((state) => ({
      services: [...state.services, { ...service, id: createEntityId() }],
    })),

  updateService: (id, updated) =>
    set((state) => ({
      services: state.services.map((service) =>
        service.id === id ? { ...service, ...updated, id: service.id } : service,
      ),
    })),

  deleteService: (id) =>
    set((state) => ({
      services: state.services.filter((service) => service.id !== id),
    })),

  addSlot: (slot) =>
    set((state) => ({
      slots: [...state.slots, { ...slot, id: createEntityId() }],
    })),

  updateSlot: (id, updated) =>
    set((state) => ({
      slots: state.slots.map((slot) =>
        slot.id === id ? { ...slot, ...updated, id: slot.id } : slot,
      ),
    })),

  deleteSlot: (id) =>
    set((state) => ({
      slots: state.slots.filter((slot) => slot.id !== id),
    })),

  getSlotsForWeek: (startDate) => {
    const { start, end } = getWeekRange(startDate);
    return get().slots.filter((slot) => slot.startTime < end && slot.endTime > start);
  },
}));

export type { CrmStore };
