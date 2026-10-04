import { forwardRef } from 'react';
import { FormItem, Textarea as VkTextarea } from '@vkontakte/vkui';
import type { TextareaProps } from './Textarea.types';

export const Textarea = forwardRef<HTMLTextAreaElement, TextareaProps>(function Textarea(
  { label, error, ...props },
  ref,
) {
  return (
    <FormItem top={label} status={error ? 'error' : 'default'} bottom={error}>
      <VkTextarea {...props} getRef={ref} />
    </FormItem>
  );
});
