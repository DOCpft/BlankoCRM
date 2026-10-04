import type { CSSProperties } from 'react';
import type { CardProps } from './Card.types';

export function Card({ children, onClick, accentColor, disabled = false, className = '' }: CardProps) {
  const style = { '--card-accent': accentColor } as CSSProperties;
  const content = (
    <>
      {accentColor && <span aria-hidden="true" className="crm-card__accent" />}
      {children}
    </>
  );

  if (onClick) {
    return (
      <button
        type="button"
        className={`crm-card crm-card--interactive ${disabled ? 'crm-card--disabled' : ''} ${className}`}
        style={style}
        onClick={onClick}
        disabled={disabled}
      >
        {content}
      </button>
    );
  }

  return (
    <article
      className={`crm-card ${disabled ? 'crm-card--disabled' : ''} ${className}`}
      style={style}
    >
      {content}
    </article>
  );
}
