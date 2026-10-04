import { forwardRef } from 'react';
import { FormItem, Select as VkSelect } from '@vkontakte/vkui';
import type { SelectProps } from './Select.types';

export const Select = forwardRef<HTMLSelectElement, SelectProps>(function Select(
  { label, error, options, ...props },
  ref,
) {
  return (
    <FormItem top={label} status={error ? 'error' : 'default'} bottom={error}>
      <VkSelect {...props} getRef={ref} options={options} />
    </FormItem>
  );
});
