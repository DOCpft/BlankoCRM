export interface ReminderSettingDto {
  offsetHours: number;
  enabled: boolean;
}

export interface UpdateReminderSettingsRequest {
  settings: ReminderSettingDto[];
}

export interface TelegramLinkResponse {
  url: string;
  botUsername: string;
}

export interface TelegramLinkStatus {
  linked: boolean;
  notificationsEnabled: boolean;
}

export interface UpdateTimeZoneRequest {
  timeZone: string;
}