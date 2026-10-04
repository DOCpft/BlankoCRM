import vkBridge from '@vkontakte/vk-bridge';
import type { IVkService, VkLaunchParams } from './IVkService';

function isAvailable(): boolean {
  return vkBridge.isEmbedded() || new URLSearchParams(window.location.search).has('vk_user_id');
}

export const vkService: IVkService = {
  async init() {
    if (isAvailable()) await vkBridge.send('VKWebAppInit');
  },

  async getLaunchParams(): Promise<VkLaunchParams> {
    //return Object.fromEntries(new URLSearchParams(window.location.search).entries());
      const data = await vkBridge.send('VKWebAppGetLaunchParams');

      // Возвращаем объект с параметрами
      return data as unknown as VkLaunchParams;
  },

  async allowNotifications() {
    if (!isAvailable()) return false;
    const response = await vkBridge.send('VKWebAppAllowNotifications');
    return 'result' in response && response.result === true;
  },

  isWebView: () => vkBridge.isWebView(),
  isAvailable,
};
