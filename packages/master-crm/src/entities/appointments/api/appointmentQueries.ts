import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { appointmentsApi } from '@blanko/shared';
import type { CreateAppointmentRequest, UpdateAppointmentRequest } from '@blanko/shared';

export const appointmentKeys = {
  all: ['appointments'] as const,
  lists: () => [...appointmentKeys.all, 'list'] as const,
  list: () => [...appointmentKeys.lists()] as const,
  calendar: (date: string) => [...appointmentKeys.all, 'calendar', date] as const,
  pending: () => [...appointmentKeys.all, 'pending'] as const,
  details: () => [...appointmentKeys.all, 'detail'] as const,
  detail: (id: string) => [...appointmentKeys.details(), id] as const,
};

export function useAppointments() {
  return useQuery({ queryKey: appointmentKeys.list(), queryFn: appointmentsApi.getAll });
}

export function useAppointmentsForDay(date: string) {
  return useQuery({
    queryKey: appointmentKeys.calendar(date),
    queryFn: () => appointmentsApi.getForDay(date),
    enabled: Boolean(date),
  });
}

export function usePendingAppointments() {
  return useQuery({ queryKey: appointmentKeys.pending(), queryFn: appointmentsApi.getPending });
}

export function useAppointment(id: string) {
  return useQuery({ queryKey: appointmentKeys.detail(id), queryFn: () => appointmentsApi.getById(id), enabled: Boolean(id) });
}

function useInvalidateAppointmentMutation<TVariables>(mutationFn: (variables: TVariables) => Promise<unknown>) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn,
    onSettled: async () => {
      await Promise.all([
        queryClient.invalidateQueries({ queryKey: appointmentKeys.all }),
        queryClient.invalidateQueries({ queryKey: ['schedule'] }),
      ]);
    },
  });
}

export function useCreateAppointment() {
  return useInvalidateAppointmentMutation((request: CreateAppointmentRequest) => appointmentsApi.create(request));
}

export function useUpdateAppointment() {
  return useInvalidateAppointmentMutation(({ id, request }: { id: string; request: UpdateAppointmentRequest }) =>
    appointmentsApi.update(id, request));
}

export function useDeleteAppointment() {
  return useInvalidateAppointmentMutation((id: string) => appointmentsApi.delete(id));
}

export function useCancelAppointment() {
  return useInvalidateAppointmentMutation((id: string) => appointmentsApi.cancel(id));
}

export function useConfirmAppointment() {
  return useInvalidateAppointmentMutation((id: string) => appointmentsApi.confirm(id));
}

export function useRejectAppointment() {
  return useInvalidateAppointmentMutation((id: string) => appointmentsApi.reject(id));
}

export function useCompleteAppointment() {
  return useInvalidateAppointmentMutation((id: string) => appointmentsApi.complete(id));
}

export function useMarkAppointmentNoShow() {
  return useInvalidateAppointmentMutation((id: string) => appointmentsApi.noShow(id));
}
