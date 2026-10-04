import type { ChangeEventHandler, FocusEventHandler } from 'react';

export interface TextareaProps {
  label?: string;
  error?: string;
  id?: string;
  name?: string;
  value?: string;
  defaultValue?: string;
  placeholder?: string;
  disabled?: boolean;
  required?: boolean;
  readOnly?: boolean;
  rows?: number;
  maxLength?: number;
  minLength?: number;
  autoComplete?: string;
  onChange?: ChangeEventHandler<HTMLTextAreaElement>;
  onBlur?: FocusEventHandler<HTMLTextAreaElement>;
}
