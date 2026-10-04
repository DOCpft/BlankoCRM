export type VkLaunchParams = Record<string, string>;

export interface IVkService {
  init(): Promise<void>;
  getLaunchParams(): Promise<VkLaunchParams>;
  allowNotifications(): Promise<boolean>;
  isWebView(): boolean;
  isAvailable(): boolean;
}
