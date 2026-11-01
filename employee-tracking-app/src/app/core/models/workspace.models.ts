export interface WeeklyHours {
  mon: number;
  tue: number;
  wed: number;
  thu: number;
  fri: number;
  sat: number;
  sun: number;
  total: number;
}

export interface WorkedHours {
  hours: number;
}

export interface Presence {
  employeeId: string;
  fullName: string;
  jobTitle: string;
  status: 'clocked-in' | 'on-break' | 'clocked-out' | string;
  since?: string | null;
}

export interface EmployeeProfile {
  id: string;
  fullName: string;
  email: string;
  jobTitle: string;
  location?: string | null;
  department: string;
  managerName?: string | null;
  shiftName?: string | null;
  shiftStart?: string | null;
  shiftEnd?: string | null;
  employmentType: string;
  availablePtoHours: number;
  firstName: string;
  lastName: string;
  gender: string;
  dateOfBirth?: string | null;
  maritalStatus: string;
  phone?: string | null;
  addressLine?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
}

export interface PersonalDetails {
  firstName: string;
  lastName: string;
  gender: string;
  dateOfBirth?: string | null;
  maritalStatus: string;
  phone?: string | null;
  addressLine?: string | null;
  city?: string | null;
  state?: string | null;
  postalCode?: string | null;
  country?: string | null;
}

export interface ScheduleDay {
  date: string;
  dayName: string;
  isToday: boolean;
  isWorkday: boolean;
  start?: string | null;
  end?: string | null;
}

export interface Schedule {
  shiftName?: string | null;
  shiftStart?: string | null;
  shiftEnd?: string | null;
  days: ScheduleDay[];
}

export interface CalendarHoliday {
  date: string;
  name: string;
}

export interface CalendarAbsence {
  employeeId: string;
  employeeName: string;
  startDate: string;
  endDate: string;
  hours: number;
}

export interface CalendarMonth {
  year: number;
  month: number;
  holidays: CalendarHoliday[];
  absences: CalendarAbsence[];
}

export interface AuditLogEntry {
  id: string;
  entityName: string;
  entityId: string;
  action: string;
  detail?: string | null;
  performedByName?: string | null;
  timestamp: string;
}
