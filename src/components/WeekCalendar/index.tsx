import { FC, useCallback, useMemo } from 'react';
import { Slot } from '../../types';
import {
  combineDateAndTime,
  generateTimeSlots,
  getDurationRows,
  getGridRow,
  getWeekDays,
  isSameDay,
} from '../../utils/dateUtils';
import { AppointmentBlock } from './AppointmentBlock';
import { SlotCell } from './SlotCell';

interface WeekCalendarProps {
  weekStart: Date;
  slots: Slot[];
  clients: Array<{ id: string; name: string }>;
  services: Array<{ id: string; name: string }>;
  onSlotClick: (date: Date) => void;
  onSlotSelect: (slot: Slot) => void;
}

export const WeekCalendar: FC<WeekCalendarProps> = ({
  weekStart,
  slots,
  clients,
  services,
  onSlotClick,
  onSlotSelect,
}) => {
  const days = useMemo(() => getWeekDays(weekStart), [weekStart]);
  const times = useMemo(() => generateTimeSlots(), []);
  const slotsByDay = useMemo(() => {
    return days.map((day) => slots.filter((slot) => isSameDay(slot.startTime, day)));
  }, [days, slots]);

  const findSlotAt = useCallback(
    (day: Date, time: string) => {
      const cellStart = combineDateAndTime(day, time);
      return slotsByDay[days.findIndex((item) => isSameDay(item, day))].find(
        (slot) => cellStart >= slot.startTime && cellStart < slot.endTime,
      );
    },
    [days, slotsByDay],
  );

  return (
    <div className="week-calendar-wrap">
      <div className="week-calendar" style={{ '--week-rows': times.length } as React.CSSProperties}>
        <div className="week-calendar__corner" />
        {days.map((day) => (
          <div className="week-calendar__day" key={day.toISOString()}>
            {day.toLocaleDateString('ru-RU', { weekday: 'short', day: 'numeric', month: 'short' })}
          </div>
        ))}
        {times.map((time, timeIndex) => (
          <div className="week-calendar__time" style={{ gridRow: timeIndex + 2 }} key={time}>{time}</div>
        ))}
        {days.flatMap((day, dayIndex) =>
          times.map((time, timeIndex) => {
            const existingSlot = findSlotAt(day, time);
            if (existingSlot) return null;
            return (
              <SlotCell
                key={`${day.toISOString()}-${time}`}
                day={day}
                time={time}
                dayColumn={dayIndex + 2}
                gridRow={timeIndex + 2}
                onClick={() => onSlotClick(combineDateAndTime(day, time))}
              />
            );
          }),
        )}
        {slots.map((slot) => {
          const dayIndex = days.findIndex((day) => isSameDay(day, slot.startTime));
          if (dayIndex < 0) return null;
          const clientName = clients.find((client) => client.id === slot.clientId)?.name;
          const serviceName = services.find((service) => service.id === slot.serviceId)?.name;
          return (
            <AppointmentBlock
              key={slot.id}
              slot={slot}
              clientName={clientName}
              serviceName={serviceName}
              dayColumn={dayIndex + 2}
              gridRow={getGridRow(slot.startTime)}
              rowSpan={getDurationRows(slot.startTime, slot.endTime)}
              onClick={() => onSlotSelect(slot)}
            />
          );
        })}
      </div>
    </div>
  );
};

export default WeekCalendar;