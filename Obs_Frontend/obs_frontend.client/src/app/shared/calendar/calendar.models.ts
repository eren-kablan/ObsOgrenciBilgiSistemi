export interface CalendarDay {
  day: number;
  isCurrentMonth: boolean;
  isToday: boolean;
}

export interface CalendarView {
  title: string;
  days: CalendarDay[];
}
