import { masterClient } from '../http/masterClient';
import type { ReminderSettingDto, TelegramLinkResponse, TelegramLinkStatus, UpdateReminderSettingsRequest, UpdateTimeZoneRequest } from '../types/profile.types';

export const profileApi = {
  async generateTelegramLink(): Promise<TelegramLinkResponse> {
    const { data } = await masterClient.post<TelegramLinkResponse>('/profile/telegram/link');
    return data;
  },

  async unlinkTelegram(): Promise<void> {
    await masterClient.delete('/profile/telegram/link');
  },

  async getTelegramStatus(): Promise<TelegramLinkStatus> {
    const { data } = await masterClient.get<TelegramLinkStatus>('/profile/telegram/status');
    return data;
  },

  async getReminderSettings(): Promise<ReminderSettingDto[]> {
    const { data } = await masterClient.get<ReminderSettingDto[]>('/profile/reminder-settings');
    return data;
  },

  async updateReminderSettings(request: UpdateReminderSettingsRequest): Promise<void> {
    await masterClient.put('/profile/reminder-settings', request);
  },

  async updateTimeZone(request: UpdateTimeZoneRequest): Promise<void> {
    await masterClient.patch('/profile/timezone', request);
  },
};