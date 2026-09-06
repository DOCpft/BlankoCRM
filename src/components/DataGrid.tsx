import { CSSProperties, FC, ReactNode } from 'react';

export interface DataGridColumn {
  key: string;
  label: string;
}

interface DataGridProps {
  columns: DataGridColumn[];
  rows: Array<{ id: number; cells: ReactNode[] }>;
  onRowClick?: (rowId: number) => void;
}

export const DataGrid: FC<DataGridProps> = ({ columns, rows, onRowClick }) => {
  return (
    <div className="crm-grid-wrap" role="region" aria-label="Список">
      <div
        className="crm-grid"
        role="table"
        style={{ '--crm-grid-columns': `repeat(${columns.length}, minmax(0, 1fr))` } as CSSProperties}
      >
        <div className="crm-grid__row crm-grid__row--header" role="row">
          {columns.map((column) => (
            <div className="crm-grid__cell" role="columnheader" key={column.key}>
              {column.label}
            </div>
          ))}
        </div>
        {rows.map((row) => (
          <div
            className={`crm-grid__row${onRowClick ? ' crm-grid__row--interactive' : ''}`}
            role={onRowClick ? 'button' : 'row'}
            tabIndex={onRowClick ? 0 : undefined}
            key={row.id}
            onClick={() => onRowClick?.(row.id)}
            onKeyDown={(event) => {
              if (onRowClick && (event.key === 'Enter' || event.key === ' ')) {
                event.preventDefault();
                onRowClick(row.id);
              }
            }}
          >
            {row.cells.map((cell, index) => (
              <div className="crm-grid__cell" role="cell" key={`${row.id}-${columns[index].key}`}>
                {cell || '—'}
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
};

export default DataGrid;
