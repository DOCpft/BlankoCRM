import { useState } from 'react';

export const useModal = <T,>() => {
  const [isOpen, setIsOpen] = useState(false);
  const [selected, setSelected] = useState<T | null>(null);

  const open = (entity?: T) => {
    setSelected(entity ?? null);
    setIsOpen(true);
  };

  const close = () => {
    setIsOpen(false);
    setSelected(null);
  };

  return {
    isOpen,
    selected,
    open,
    close,
  };
};
