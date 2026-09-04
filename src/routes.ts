import {
  createHashRouter,
  createPanel,
  createRoot,
  createView,
  RoutesConfig,
} from '@vkontakte/vk-mini-apps-router';

export const DEFAULT_ROOT = 'default_root';

export const DEFAULT_VIEW = 'default_view';

export const DEFAULT_VIEW_PANELS = {
  DASHBOARD: 'dashboard',
  CLIENTS: 'clients',
  APPOINTMENTS: 'appointments',
  PROFILE: 'profile',
  SETTINGS: 'settings',
} as const;

export const routes = RoutesConfig.create([
  createRoot(DEFAULT_ROOT, [
    createView(DEFAULT_VIEW, [
      createPanel(DEFAULT_VIEW_PANELS.DASHBOARD, '/', []),
      createPanel(DEFAULT_VIEW_PANELS.CLIENTS, `/${DEFAULT_VIEW_PANELS.CLIENTS}`, []),
      createPanel(DEFAULT_VIEW_PANELS.APPOINTMENTS, `/${DEFAULT_VIEW_PANELS.APPOINTMENTS}`, []),
      createPanel(DEFAULT_VIEW_PANELS.PROFILE, `/${DEFAULT_VIEW_PANELS.PROFILE}`, []),
      createPanel(DEFAULT_VIEW_PANELS.SETTINGS, `/${DEFAULT_VIEW_PANELS.SETTINGS}`, []),
    ]),
  ]),
]);

export const router = createHashRouter(routes.getRoutes());
