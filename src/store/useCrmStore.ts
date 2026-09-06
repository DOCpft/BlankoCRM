import { create } from 'zustand';

import { Client, Service } from '../types';

interface CrmStore {
  clients: Client[];
  services: Service[];
  addClient: (client: Omit<Client, 'id'>) => void;
  updateClient: (id: string, updated: Partial<Client>) => void;
  deleteClient: (id: string) => void;
  addService: (service: Omit<Service, 'id'>) => void;
  updateService: (id: string, updated: Partial<Service>) => void;
  deleteService: (id: string) => void;
}

const createEntityId = (): string => {
  if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }

  return `${Date.now()}-${Math.random().toString(36).slice(2)}`;
};

export const useCrmStore = create<CrmStore>((set) => ({
  clients: [],
  services: [],

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
}));

export type { CrmStore };
