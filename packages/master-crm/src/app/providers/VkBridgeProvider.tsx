import { useEffect, type ReactNode } from 'react';
import { vkService } from '@blanko/shared';

export function VkBridgeProvider({ children }: { children: ReactNode }) {
  useEffect(() => {
    if (vkService.isAvailable()) void vkService.init();
  }, []);

  return children;
}
