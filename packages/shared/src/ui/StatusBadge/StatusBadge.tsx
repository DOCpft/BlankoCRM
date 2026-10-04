import { AppointmentStatus } from '../../api/types/appointment.types';
import type { StatusBadgeProps } from './StatusBadge.types';

const statusInfo: Record<AppointmentStatus, { label: string; icon: string; className: string }> = {
  [AppointmentStatus.Scheduled]: { label: 'Подтверждена', icon: '✓', className: 'scheduled' },
  [AppointmentStatus.Completed]: { label: 'Выполнена', icon: '✓✓', className: 'completed' },
  [AppointmentStatus.Cancelled]: { label: 'Отменена', icon: '×', className: 'cancelled' },
  [AppointmentStatus.NoShow]: { label: 'Не пришёл', icon: '—', className: 'noshow' },
  [AppointmentStatus.Archived]: { label: 'Архив', icon: '▣', className: 'archived' },
  [AppointmentStatus.Pending]: { label: 'Ожидает', icon: '◷', className: 'pending' },
  [AppointmentStatus.Rejected]: { label: 'Отклонена', icon: '×', className: 'rejected' },
  [AppointmentStatus.Withdrawn]: { label: 'Отозвана', icon: '↩', className: 'withdrawn' },
};

export function StatusBadge({ status, compact = false }: StatusBadgeProps) {
  const info = statusInfo[status];
  return (
    <span className={`crm-status-badge crm-status-badge--${info.className}`}>
      <span aria-hidden="true">{info.icon}</span>
      {!compact && <span>{info.label}</span>}
    </span>
  );
}
