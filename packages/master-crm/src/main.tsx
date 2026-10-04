import { createRoot } from 'react-dom/client';
import { vkService } from '@blanko/shared';
import { AppConfig } from './AppConfig';
import '../../shared/src/ui/styles.css';

function bootstrap() {
  const rootElement = document.getElementById('root');
  if (!rootElement) throw new Error('Не найден корневой элемент приложения');

  createRoot(rootElement).render(<AppConfig />);

  if (vkService.isAvailable()) {
    void vkService.init().catch((error: unknown) => {
      console.warn('Не удалось инициализировать VK Bridge', error);
    });
  }
}

bootstrap();
