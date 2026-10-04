import type { AppointmentStatus } from '../../api/types/appointment.types';

export interface StatusBadgeProps {
  status: AppointmentStatus;
  compact?: boolean;
}
