import type { KeyboardEvent } from 'react';
import { Card } from '../Card';
import type { DataListProps } from './DataList.types';

export function DataList<T>({
  items,
  getId,
  columns,
  onItemClick,
  renderCardLeading,
  renderCardTitle,
  renderCardSubtitle,
  renderCardDescription,
  renderCardBadge,
  renderCardExpiry,
  cardActions,
  cardAccent,
  loading = false,
  emptyState = 'Нет данных',
  variant = 'default',
}: DataListProps<T>) {
  if (loading) return <div className="crm-data-list__state" role="status">Загрузка…</div>;
  if (items.length === 0) return <div className="crm-data-list__state">{emptyState}</div>;

  const handleRowKeyDown = (event: KeyboardEvent<HTMLTableRowElement>, item: T) => {
    if ((event.key === 'Enter' || event.key === ' ') && onItemClick) {
      event.preventDefault();
      onItemClick(item);
    }
  };

  return (
    <>
      <div className="crm-data-list__cards">
        {items.map((item) => {
          const accentColor = cardAccent?.(item);
          const content = (
            <>
              {renderCardLeading?.(item)}
              <div className="crm-data-list__body">
                <div className="crm-data-list__heading">
                  <strong>{renderCardTitle(item)}</strong>
                  {renderCardBadge?.(item)}
                </div>
                {renderCardSubtitle && <div className="crm-data-list__subtitle">{renderCardSubtitle(item)}</div>}
                {renderCardDescription && <div className="crm-data-list__description">{renderCardDescription(item)}</div>}
                {renderCardExpiry && <div className="crm-data-list__expiry">{renderCardExpiry(item)}</div>}
                {cardActions && <div className="crm-data-list__actions">{cardActions(item)}</div>}
              </div>
              {onItemClick && variant === 'default' && <span className="crm-data-list__arrow" aria-hidden="true">→</span>}
            </>
          );

          return (
            <Card
              key={getId(item)}
              onClick={variant === 'default' ? () => onItemClick?.(item) : undefined}
              accentColor={accentColor}
              className={variant === 'action-cards' ? 'crm-data-list__action-card' : ''}
            >
              {content}
            </Card>
          );
        })}
      </div>
      <div className="crm-data-list__table-wrap">
        <table className="crm-data-list__table">
          <thead>
            <tr>{columns.map((column) => <th key={column.key} scope="col">{column.label}</th>)}</tr>
          </thead>
          <tbody>
            {items.map((item) => (
              <tr
                key={getId(item)}
                tabIndex={onItemClick ? 0 : undefined}
                onClick={onItemClick ? () => onItemClick(item) : undefined}
                onKeyDown={onItemClick ? (event) => handleRowKeyDown(event, item) : undefined}
              >
                {columns.map((column) => <td key={column.key}>{column.render(item)}</td>)}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </>
  );
}
