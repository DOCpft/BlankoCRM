import { FC, PointerEvent, useRef, useState } from 'react';
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
  const gestureStart = useRef<{ x: number; y: number } | null>(null);

  const handlePointerDown = (event: PointerEvent<HTMLDivElement>) => {
    if (event.pointerType === 'mouse' && event.button !== 0) {
      return;
    }

    gestureStart.current = { x: event.clientX, y: event.clientY };
  };

  const handlePointerUp = (event: PointerEvent<HTMLDivElement>) => {
    const start = gestureStart.current;
    gestureStart.current = null;

    if (!start) {
      return;
    }

    const deltaX = event.clientX - start.x;
    const deltaY = event.clientY - start.y;
    const isHorizontalSwipe = Math.abs(deltaX) >= 60 && Math.abs(deltaX) > Math.abs(deltaY) * 1.25;

    if (!isHorizontalSwipe) {
      return;
    }

    if (!showNav && deltaX > 0) {
      setShowNav(true);
    } else if (showNav && deltaX < 0 && start.x <= 320) {
      setShowNav(false);
    }
  };

  const handlePointerCancel = () => {
    gestureStart.current = null;
  };

  return (
    <div
      className="crm-root"
      onPointerDown={handlePointerDown}
      onPointerUp={handlePointerUp}
      onPointerCancel={handlePointerCancel}
    >
      <aside className={`crm-side ${showNav ? 'crm-side--open' : 'crm-side--collapsed'}`}>
        <SideNav collapsed={false} onToggle={() => setShowNav(false)} onNavigate={() => setShowNav(false)} />
      </aside>

      {!showNav && (
        <div className="crm-swipe-edge" aria-hidden="true" />
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
