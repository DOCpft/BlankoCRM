import { CSSProperties, FC, ReactNode } from 'react';

export interface DataGridColumn {
  key: string;
  label: string;
}

interface DataGridProps {
  columns: DataGridColumn[];
  rows: Array<{ id: number; cells: ReactNode[] }>;
}

export const DataGrid: FC<DataGridProps> = ({ columns, rows }) => {
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
          <div className="crm-grid__row" role="row" key={row.id}>
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
