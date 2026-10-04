import type { ReactNode, MouseEventHandler } from 'react';

export type ButtonVariant = 'primary' | 'secondary' | 'tertiary' | 'danger';
export type ButtonSize = 'sm' | 'md' | 'lg';

export interface ButtonProps {
  variant?: ButtonVariant;
  size?: ButtonSize;
  loading?: boolean;
  disabled?: boolean;
  fullWidth?: boolean;
  type?: 'button' | 'submit';
  ariaLabel?: string;
  leftIcon?: ReactNode;
  rightIcon?: ReactNode;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  children: ReactNode;
}
