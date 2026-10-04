import type { ReactNode } from 'react';

export interface SidebarItem {
  id: string;
  label: string;
  icon: ReactNode;
  href: string;
  badge?: number;
}

export interface SidebarProps {
  items: SidebarItem[];
  isOpen: boolean;
  onClose: () => void;
  logo?: ReactNode;
  dragOffset?: number;
  isDragging?: boolean;
}
