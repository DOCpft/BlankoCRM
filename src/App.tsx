import { FC, useState } from 'react';
import { View, IconButton } from '@vkontakte/vkui';
import { Icon28MenuOutline } from '@vkontakte/icons';
import { useActiveVkuiLocation } from '@vkontakte/vk-mini-apps-router';

import { Dashboard, Clients, Appointments, Profile, Settings, Services, Slots } from './panels';
import { SideNav } from './components/SideNav';
import { DEFAULT_VIEW, DEFAULT_VIEW_PANELS } from './routes';

export const App: FC = () => {
  const { panel: activePanel = DEFAULT_VIEW_PANELS.DASHBOARD } = useActiveVkuiLocation();
  // showNav: whether the full side nav panel is visible
  const [showNav, setShowNav] = useState(true);

  return (
    <div className="crm-root">
      {showNav && (
        <aside className="crm-side crm-side--open">
          <SideNav collapsed={false} onToggle={() => setShowNav(false)} onNavigate={() => setShowNav(false)} />
        </aside>
      )}

      {!showNav && (
        <div className="crm-burger-overlay">
          <IconButton aria-label="open menu" onClick={() => setShowNav(true)}>
            <Icon28MenuOutline />
          </IconButton>
        </div>
      )}

      <main className="crm-main">
        <View id={DEFAULT_VIEW} activePanel={activePanel}>
          <Dashboard id={DEFAULT_VIEW_PANELS.DASHBOARD} />
          <Clients id={DEFAULT_VIEW_PANELS.CLIENTS} />
          <Services id={DEFAULT_VIEW_PANELS.SERVICES} />
          <Appointments id={DEFAULT_VIEW_PANELS.APPOINTMENTS} />
          <Slots id={DEFAULT_VIEW_PANELS.SLOTS} />
          <Profile id={DEFAULT_VIEW_PANELS.PROFILE} />
          <Settings id={DEFAULT_VIEW_PANELS.SETTINGS} />
        </View>
      </main>
    </div>
  );
};
