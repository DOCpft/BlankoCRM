import type { ReactNode } from 'react';

export interface CardProps {
  children: ReactNode;
  onClick?: () => void;
  accentColor?: string;
  disabled?: boolean;
  className?: string;
}
