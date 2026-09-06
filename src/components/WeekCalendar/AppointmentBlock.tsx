import { FC } from 'react';
import { Slot } from '../../types';

interface AppointmentBlockProps {
  slot: Slot;
  clientName?: string;
  serviceName?: string;
  dayColumn: number;
  gridRow: number;
  rowSpan: number;
  onClick: () => void;
}

export const AppointmentBlock: FC<AppointmentBlockProps> = ({
  slot,
  clientName,
  serviceName,
  dayColumn,
  gridRow,
  rowSpan,
  onClick,
}) => (
  <button
    type="button"
    className={`appointment-block${slot.isFree ? ' appointment-block--empty' : ''}`}
    style={{ gridColumn: dayColumn, gridRow: `${gridRow} / span ${rowSpan}` }}
    onClick={onClick}
  >
    <strong>{slot.isFree ? 'Пустой слот' : clientName || 'Запись'}</strong>
    {!slot.isFree && serviceName && <span>{serviceName}</span>}
    <small>
      {slot.startTime.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })}
      {' - '}
      {slot.endTime.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })}
    </small>
  </button>
);

export default AppointmentBlock;