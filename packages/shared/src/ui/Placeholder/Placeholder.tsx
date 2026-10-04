import type { ReactNode } from 'react';

export function Placeholder({ children }: { children: ReactNode }) {
  return <div className="crm-data-list__state">{children}</div>;
}
