import { AppRouter } from './app/router/AppRouter';
import { AppProvider } from './app/providers/AppProvider';

export function AppConfig() {
  return (
    <AppProvider>
      <AppRouter />
    </AppProvider>
  );
}
