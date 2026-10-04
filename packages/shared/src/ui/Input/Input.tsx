import { forwardRef } from 'react';
import { FormItem, Input as VkInput } from '@vkontakte/vkui';
import type { InputProps } from './Input.types';

export const Input = forwardRef<HTMLInputElement, InputProps>(function Input(
  { label, error, ...props },
  ref,
) {
  return (
    <FormItem top={label} status={error ? 'error' : 'default'} bottom={error}>
      <VkInput {...props} getRef={ref} />
    </FormItem>
  );
});
