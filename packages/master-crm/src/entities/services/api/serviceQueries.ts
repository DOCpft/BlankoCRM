import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { servicesApi } from '@blanko/shared';
import type { CreateServiceRequest, UpdateServiceRequest } from '@blanko/shared';

export const serviceKeys = {
  all: ['services'] as const,
  lists: () => [...serviceKeys.all, 'list'] as const,
  details: () => [...serviceKeys.all, 'detail'] as const,
  detail: (id: string) => [...serviceKeys.details(), id] as const,
};

export function useServices() {
  return useQuery({ queryKey: serviceKeys.lists(), queryFn: servicesApi.getAll });
}

export function useCreateService() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateServiceRequest) => servicesApi.create(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: serviceKeys.all }),
  });
}

export function useUpdateService() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, request }: { id: string; request: UpdateServiceRequest }) => servicesApi.update(id, request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: serviceKeys.all }),
  });
}

export function useDeleteService() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => servicesApi.delete(id),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: serviceKeys.all }),
  });
}
