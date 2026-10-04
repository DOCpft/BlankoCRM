import type { SnackbarProps } from './Snackbar.types';

export function Snackbar({ message, variant = 'error' }: SnackbarProps) {
  return (
    <div className={`crm-snackbar crm-snackbar--${variant}`} role={variant === 'error' ? 'alert' : 'status'}>
      {message}
    </div>
  );
}
