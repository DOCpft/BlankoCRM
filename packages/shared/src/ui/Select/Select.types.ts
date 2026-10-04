import type { ChangeEvent, FocusEventHandler } from 'react';

type SelectValue = string | number | readonly string[] | null;

export interface SelectOption {
  value: string;
  label: string;
}

export interface SelectProps {
  label?: string;
  error?: string;
  id?: string;
  name?: string;
  value?: SelectValue;
  defaultValue?: SelectValue;
  placeholder?: string;
  disabled?: boolean;
  required?: boolean;
  autoFocus?: boolean;
  onChange?: (event: ChangeEvent<HTMLSelectElement>, value: SelectValue) => void;
  onBlur?: FocusEventHandler<HTMLSelectElement>;
  options: SelectOption[];
}
