import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { scheduleApi } from '@blanko/shared';
import type { CreateFreeSlotRequest, CreateOccupiedSlotRequest, UpdateSlotRequest } from '@blanko/shared';

export const scheduleKeys = {
  all: ['schedule'] as const,
  periods: () => [...scheduleKeys.all, 'period'] as const,
  period: (from: string, to: string) => [...scheduleKeys.periods(), { from, to }] as const,
};

export function useSchedulePeriod(from: string, to: string) {
  return useQuery({
    queryKey: scheduleKeys.period(from, to),
    queryFn: () => scheduleApi.getPeriod(from, to),
    enabled: Boolean(from && to),
  });
}

function useInvalidateScheduleMutation<TVariables>(mutationFn: (variables: TVariables) => Promise<unknown>) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: scheduleKeys.all }),
  });
}

export function useCreateFreeSlot() {
  return useInvalidateScheduleMutation((request: CreateFreeSlotRequest) => scheduleApi.createFree(request));
}

export function useCreateFreeSlots() {
  return useInvalidateScheduleMutation((requests: CreateFreeSlotRequest[]) => scheduleApi.createFreeBulk(requests));
}

export function useCreateOccupiedSlot() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreateOccupiedSlotRequest) => scheduleApi.createOccupied(request),
    onSuccess: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: scheduleKeys.all }),
        queryClient.invalidateQueries({ queryKey: ['appointments'] }),
      ]);
    },
  });
}

export function useUpdateSlot() {
  return useInvalidateScheduleMutation(({ id, request }: { id: string; request: UpdateSlotRequest }) =>
    scheduleApi.update(id, request));
}

export function useDeleteSlot() {
  return useInvalidateScheduleMutation((id: string) => scheduleApi.delete(id));
}

export function useDeleteSlots() {
  return useInvalidateScheduleMutation((ids: string[]) => scheduleApi.deleteBulk(ids));
}
