export enum TimeEntryType {
  ClockIn = 0,
  ClockOut = 1,
  BreakStart = 2,
  BreakEnd = 3,
  ManagerCorrection = 4
}

export enum TimeEntrySource {
  WebApp = 0,
  MobileApp = 1,
  Kiosk = 2,
  ManagerEntry = 3,
  SystemCorrection = 4
}

export interface TimeEntry {
  id: string;
  employeeId: string;
  entryType: TimeEntryType;
  source: TimeEntrySource;
  timestamp: string;
  notes?: string;
  isDeleted: boolean;
}

export interface ClockInRequest {
  source: TimeEntrySource;
  latitude?: number;
  longitude?: number;
  deviceId?: string;
}

export interface ClockInResponse {
  entryId: string;
  employeeId: string;
  timestamp: string;
  message: string;
}

export interface ClockOutResponse {
  entryId: string;
  employeeId: string;
  timestamp: string;
  hoursWorked: number;
  message: string;
}

export type ClockStatus = 'clocked-out' | 'clocked-in' | 'on-break';
