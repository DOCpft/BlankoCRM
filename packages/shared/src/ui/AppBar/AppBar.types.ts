import type { ReactNode } from 'react';

export interface AppBarProps {
  title: string;
  onMenuClick: () => void;
  actions?: ReactNode;
}
