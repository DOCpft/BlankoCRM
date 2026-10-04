import { Icon28MenuOutline } from '@vkontakte/icons';

export type IconName = 'menu' | 'home' | 'clients' | 'services' | 'appointments' | 'schedule' | 'profile' | 'settings';

const glyphs: Record<Exclude<IconName, 'menu'>, string> = {
  home: '⌂',
  clients: '◉',
  services: '✦',
  appointments: '▦',
  schedule: '◷',
  profile: '●',
  settings: '⚙',
};

export function Icon({ name }: { name: IconName }) {
  if (name === 'menu') return <Icon28MenuOutline aria-hidden="true" />;
  return <span aria-hidden="true">{glyphs[name]}</span>;
}
