import type { ReactNode } from 'react';

export interface DataListColumn<T> {
  key: string;
  label: string;
  render: (item: T) => ReactNode;
}

export interface DataListProps<T> {
  items: T[];
  getId: (item: T) => string;
  columns: DataListColumn<T>[];
  onItemClick?: (item: T) => void;
  renderCardLeading?: (item: T) => ReactNode;
  renderCardTitle: (item: T) => ReactNode;
  renderCardSubtitle?: (item: T) => ReactNode;
  renderCardDescription?: (item: T) => ReactNode;
  renderCardBadge?: (item: T) => ReactNode;
  renderCardExpiry?: (item: T) => ReactNode;
  cardActions?: (item: T) => ReactNode;
  cardAccent?: (item: T) => string | undefined;
  loading?: boolean;
  emptyState?: ReactNode;
  variant?: 'default' | 'action-cards';
}
