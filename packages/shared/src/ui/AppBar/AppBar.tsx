import { Button } from '../Button';
import { Icon } from '../Icon';
import type { AppBarProps } from './AppBar.types';

export function AppBar({ title, onMenuClick, actions }: AppBarProps) {
  return (
    <header className="crm-appbar">
      <Button ariaLabel="Открыть меню" variant="tertiary" onClick={onMenuClick}>
        <Icon name="menu" />
      </Button>
      <h1>{title}</h1>
      <div className="crm-appbar__actions">{actions}</div>
    </header>
  );
}
