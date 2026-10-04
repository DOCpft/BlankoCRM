import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { profileApi } from '@blanko/shared';
import type { UpdateReminderSettingsRequest, UpdateTimeZoneRequest } from '@blanko/shared';

export const profileKeys = {
  all: ['profile'] as const,
  telegram: () => [...profileKeys.all, 'telegram'] as const,
  reminders: () => [...profileKeys.all, 'reminders'] as const,
};

export function useTelegramStatus() {
  return useQuery({ queryKey: profileKeys.telegram(), queryFn: profileApi.getTelegramStatus });
}

export function useReminderSettings() {
  return useQuery({ queryKey: profileKeys.reminders(), queryFn: profileApi.getReminderSettings });
}

export function useGenerateTelegramLink() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: profileApi.generateTelegramLink,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: profileKeys.telegram() }),
  });
}

export function useUnlinkTelegram() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: profileApi.unlinkTelegram,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: profileKeys.telegram() }),
  });
}

export function useUpdateReminderSettings() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: UpdateReminderSettingsRequest) => profileApi.updateReminderSettings(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: profileKeys.reminders() }),
  });
}

export function useUpdateTimeZone() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: UpdateTimeZoneRequest) => profileApi.updateTimeZone(request),
    onSuccess: () => queryClient.invalidateQueries({ queryKey: profileKeys.all }),
  });
}
