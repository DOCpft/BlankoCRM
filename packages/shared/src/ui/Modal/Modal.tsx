import { useRef } from 'react';
import { Button } from '../Button';
import type { ModalProps } from './Modal.types';

export function Modal({ open, title, onClose, children }: ModalProps) {
  const startY = useRef<number | null>(null);
  if (!open) return null;

  return (
    <div className="crm-modal-layer">
      <button className="crm-modal-backdrop" type="button" aria-label="Закрыть окно" onClick={onClose} />
      <section
        className="crm-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="crm-modal-title"
        data-no-swipe
        onKeyDown={(event) => {
          if (event.key === 'Escape') onClose();
        }}
        onPointerDown={(event) => { startY.current = event.clientY; }}
        onPointerUp={(event) => {
          if (startY.current !== null && event.clientY - startY.current > 100) onClose();
          startY.current = null;
        }}
        onPointerCancel={() => { startY.current = null; }}
      >
        <header className="crm-modal__header">
          <h2 id="crm-modal-title">{title}</h2>
          <Button variant="tertiary" ariaLabel="Закрыть" onClick={onClose}>×</Button>
        </header>
        {children}
      </section>
    </div>
  );
}
