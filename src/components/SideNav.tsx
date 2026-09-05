import { FC } from 'react';
import { Panel, PanelHeader, Group, Div, IconButton, Button } from '@vkontakte/vkui';
import { Icon28MenuOutline } from '@vkontakte/icons';
import { useRouteNavigator } from '@vkontakte/vk-mini-apps-router';
import { DEFAULT_VIEW_PANELS } from '../routes';

interface SideNavProps {
  collapsed: boolean;
  onToggle: () => void;
  onNavigate?: (panelId: string) => void;
}

export const SideNav: FC<SideNavProps> = ({ collapsed, onToggle, onNavigate }) => {
  const routeNavigator = useRouteNavigator();

  // SideNav mounted (no debug logs)

  return (
    <Panel id="sidenav" style={{ height: '100%' }}>
      <PanelHeader>
        <Div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
          <IconButton aria-label="menu" onClick={onToggle} style={{ padding: 6 }}>
            <Icon28MenuOutline width={20} height={20} />
          </IconButton>
          {!collapsed && (
            <div
              style={{
                fontWeight: 600,
                fontSize: 16,
                whiteSpace: 'nowrap',
                overflow: 'visible',
                flexShrink: 0,
              }}
            >
              BLANKO CRM
            </div>
          )}
        </Div>
      </PanelHeader>

      <Group>
        <Div style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
          <Button
            stretched
            size="l"
            mode="secondary"
            onClick={() => {
              if (onNavigate) onNavigate(DEFAULT_VIEW_PANELS.DASHBOARD);
              routeNavigator.push('/');
            }}
          >
            {!collapsed ? 'Главная' : ' '}
          </Button>
          
          <Button
            stretched
            size="l"
            mode="secondary"
            onClick={() => {
              if (onNavigate) onNavigate(DEFAULT_VIEW_PANELS.SERVICES);
              routeNavigator.push(`/${DEFAULT_VIEW_PANELS.SERVICES}`);
            }}
          >
            {!collapsed ? 'Услуги' : ' '}
          </Button>

          <Button
            stretched
            size="l"
            mode="secondary"
            onClick={() => {
              if (onNavigate) onNavigate(DEFAULT_VIEW_PANELS.CLIENTS);
              routeNavigator.push(`/${DEFAULT_VIEW_PANELS.CLIENTS}`);
            }}
          >
            {!collapsed ? 'Клиенты' : ' '}
          </Button>

          <Button
            stretched
            size="l"
            mode="secondary"
            onClick={() => {
              if (onNavigate) onNavigate(DEFAULT_VIEW_PANELS.APPOINTMENTS);
              routeNavigator.push(`/${DEFAULT_VIEW_PANELS.APPOINTMENTS}`);
            }}
          >
            {!collapsed ? 'Записи' : ' '}
          </Button>
          
          <Button
            stretched
            size="l"
            mode="secondary"
            onClick={() => {
              if (onNavigate) onNavigate(DEFAULT_VIEW_PANELS.SLOTS);
              routeNavigator.push(`/${DEFAULT_VIEW_PANELS.SLOTS}`);
            }}
          >
            {!collapsed ? 'Слоты' : ' '}
          </Button>

          <Button
            stretched
            size="l"
            mode="secondary"
            onClick={() => {
              if (onNavigate) onNavigate(DEFAULT_VIEW_PANELS.PROFILE);
              routeNavigator.push(`/${DEFAULT_VIEW_PANELS.PROFILE}`);
            }}
          >
            {!collapsed ? 'Профиль' : ' '}
          </Button>
    
          <Button
            stretched
            size="l"
            mode="tertiary"
            onClick={() => {
              if (onNavigate) onNavigate(DEFAULT_VIEW_PANELS.SETTINGS);
              routeNavigator.push(`/${DEFAULT_VIEW_PANELS.SETTINGS}`);
            }}
          >
            {!collapsed ? 'Настройки' : ' '}
          </Button>
        </Div>
      </Group>

      {collapsed && (
        <div style={{ position: 'absolute', left: 0, top: 0, bottom: 0, width: 6, background: 'var(--crm-accent)' }} />
      )}
    </Panel>
  );
};

export default SideNav;
