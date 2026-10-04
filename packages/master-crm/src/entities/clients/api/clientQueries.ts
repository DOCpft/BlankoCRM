import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { clientsApi } from '@blanko/shared';
import type { CreateClientRequest, UpdateClientRequest } from '@blanko/shared';

export const clientKeys = {
  all: ['clients'] as const,
  lists: () => [...clientKeys.all, 'list'] as const,
  list: (search = '') => [...clientKeys.lists(), { search }] as const,
  details: () => [...clientKeys.all, 'detail'] as const,
  detail: (id: string) => [...clientKeys.details(), id] as const,
};

export function useClients(search = '') {
  return useQuery({ queryKey: clientKeys.list(search), queryFn: () => clientsApi.getAll(search) });
}

export function useClient(id: string) {
  return useQuery({ queryKey: clientKeys.detail(id), queryFn: () => clientsApi.getById(id), enabled: Boolean(id) });
}

export function useCreateClient() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateClientRequest) => clientsApi.create(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: clientKeys.all }),
  });
}

export function useUpdateClient() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateClientRequest }) => clientsApi.update(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: clientKeys.all }),
  });
}

export function useDeleteClient() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => clientsApi.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: clientKeys.all }),
  });
}
