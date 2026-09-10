import { CalendarView } from './calendar.models';

const MONTH_NAMES = [
  'Ocak', 'Şubat', 'Mart', 'Nisan', 'Mayıs', 'Haziran',
  'Temmuz', 'Ağustos', 'Eylül', 'Ekim', 'Kasım', 'Aralık'
];

export function createCalendarView(displayDate: Date, today: Date): CalendarView {
  const year = displayDate.getFullYear();
  const month = displayDate.getMonth();
  const firstDay = (new Date(year, month, 1).getDay() + 6) % 7;
  const daysInMonth = new Date(year, month + 1, 0).getDate();
  const daysInPreviousMonth = new Date(year, month, 0).getDate();
  const days = [];

  for (let offset = firstDay - 1; offset >= 0; offset--) {
    days.push({ day: daysInPreviousMonth - offset, isCurrentMonth: false, isToday: false });
  }

  for (let day = 1; day <= daysInMonth; day++) {
    days.push({
      day,
      isCurrentMonth: true,
      isToday: day === today.getDate() && month === today.getMonth() && year === today.getFullYear()
    });
  }

  for (let day = 1; days.length < 42; day++) {
    days.push({ day, isCurrentMonth: false, isToday: false });
  }

  return { title: `${MONTH_NAMES[month]} ${year}`, days };
}
