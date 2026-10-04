import type { CSSProperties } from 'react';
import type { AvatarProps } from './Avatar.types';

export function Avatar({ name, size = 48 }: AvatarProps) {
  const initials = name.trim().split(/\s+/).slice(0, 2).map((part) => part[0]?.toLocaleUpperCase()).join('');
  return (
    <span className="crm-avatar" style={{ '--avatar-size': `${size}px` } as CSSProperties} aria-hidden="true">
      {initials || '?'}
    </span>
  );
}
