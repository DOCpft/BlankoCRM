const MINUTES_PER_DAY = 24 * 60;

export const startOfDay = (date: Date): Date => {
  const result = new Date(date);
  result.setHours(0, 0, 0, 0);
  return result;
};

export const getWeekStart = (date: Date): Date => {
  const result = startOfDay(date);
  const day = result.getDay();
  const daysFromMonday = day === 0 ? 6 : day - 1;
  result.setDate(result.getDate() - daysFromMonday);
  return result;
};

export const getWeekDays = (weekStart: Date): Date[] =>
  Array.from({ length: 7 }, (_, index) => {
    const day = new Date(weekStart);
    day.setDate(day.getDate() + index);
    return day;
  });

export const generateTimeSlots = (fromHour = 9, toHour = 21, stepMinutes = 30): string[] => {
  const result: string[] = [];
  for (let minutes = fromHour * 60; minutes < toHour * 60; minutes += stepMinutes) {
    const hours = Math.floor(minutes / 60).toString().padStart(2, '0');
    const remainder = (minutes % 60).toString().padStart(2, '0');
    result.push(`${hours}:${remainder}`);
  }
  return result;
};

export const combineDateAndTime = (day: Date, time: string): Date => {
  const [hours, minutes] = time.split(':').map(Number);
  const result = new Date(day);
  result.setHours(hours, minutes, 0, 0);
  return result;
};

export const getWeekRange = (date: Date): { start: Date; end: Date } => {
  const start = getWeekStart(date);
  const end = new Date(start);
  end.setDate(end.getDate() + 7);
  return { start, end };
};

export const isSameDay = (left: Date, right: Date): boolean =>
  left.getFullYear() === right.getFullYear() &&
  left.getMonth() === right.getMonth() &&
  left.getDate() === right.getDate();

export const isTimeBetween = (time: Date, start: Date, end: Date): boolean =>
  time >= start && time < end;

export const getMinutesFromMidnight = (date: Date): number =>
  date.getHours() * 60 + date.getMinutes();

export const getDateTimeInputValue = (date: Date): string => {
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
};

export const parseDateTimeInputValue = (value: string): Date => new Date(value);

export const getGridRow = (date: Date, fromHour = 9, stepMinutes = 30): number =>
  Math.floor((getMinutesFromMidnight(date) - fromHour * 60) / stepMinutes) + 2;

export const getDurationRows = (start: Date, end: Date, stepMinutes = 30): number =>
  Math.max(1, Math.ceil((end.getTime() - start.getTime()) / (stepMinutes * 60000)));

export const minutesPerDay = MINUTES_PER_DAY;