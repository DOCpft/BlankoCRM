import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { publicAppointmentsApi } from '@blanko/shared';
import type { CreatePublicAppointmentRequest } from '@blanko/shared';
import type { VkMiniAppAuthRequest } from '@blanko/shared';

export const publicBookingKeys = {
  services: (slug: string) => ['public-booking', slug, 'services'] as const,
  slots: (slug: string, from: string, to: string) => ['public-booking', slug, 'slots', from, to] as const,
  mine: (slug: string, vkParams: Record<string, string>) => ['public-booking', slug, 'mine', vkParams.vk_user_id] as const,
};

export function usePublicServices(slug: string) {
  return useQuery({ queryKey: publicBookingKeys.services(slug), queryFn: () => publicAppointmentsApi.getServices(slug), enabled: Boolean(slug) });
}

export function usePublicSlots(slug: string, from: string, to: string) {
  return useQuery({ queryKey: publicBookingKeys.slots(slug, from, to), queryFn: () => publicAppointmentsApi.getSlots(slug, from, to), enabled: Boolean(slug && from && to) });
}

export function useMyPublicAppointments(slug: string, request: VkMiniAppAuthRequest) {
  return useQuery({ queryKey: publicBookingKeys.mine(slug, request.vkParams), queryFn: () => publicAppointmentsApi.getMine(slug, request), enabled: Boolean(slug && request.vkParams.vk_user_id && request.vkParams.sign) });
}

export function useCreatePublicAppointment(slug: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: CreatePublicAppointmentRequest) => publicAppointmentsApi.create(slug, request),
    onSuccess: async () => queryClient.invalidateQueries({ queryKey: ['public-booking', slug] }),
  });
}

export function useCancelMyPublicAppointment(slug: string, request: VkMiniAppAuthRequest) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, pending }: { id: string; pending: boolean }) => pending
      ? publicAppointmentsApi.withdraw(slug, id, request)
      : publicAppointmentsApi.cancel(slug, id, request),
    onSettled: async () => queryClient.invalidateQueries({ queryKey: publicBookingKeys.mine(slug, request.vkParams) }),
  });
}
