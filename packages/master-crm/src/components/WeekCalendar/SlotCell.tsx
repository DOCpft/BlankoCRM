import { FC } from 'react';

interface SlotCellProps {
  day: Date;
  time: string;
  dayColumn: number;
  gridRow: number;
  onClick: () => void;
}

export const SlotCell: FC<SlotCellProps> = ({ day, time, dayColumn, gridRow, onClick }) => (
  <button
    type="button"
    className="slot-cell"
    style={{ gridColumn: dayColumn, gridRow }}
    aria-label={`Свободный слот ${day.toLocaleDateString('ru-RU')} ${time}`}
    onClick={onClick}
  />
);

export default SlotCell;