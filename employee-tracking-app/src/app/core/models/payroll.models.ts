export interface PayrollLine {
  employeeId: string;
  employeeFullName: string;
  employeeNumber: string;
  department: string;
  regularHours: number;
  overtimeHours: number;
  ptoHours: number;
  unpaidHours: number;
  totalPayableHours: number;
  hasExceptions: boolean;
  exceptionNotes?: string;
  timesheetStatus: string;
}

export interface PayrollReport {
  payPeriodId: string;
  payPeriodName: string;
  startDate: string;
  endDate: string;
  lines: PayrollLine[];
  totalRegularHours: number;
  totalOvertimeHours: number;
  totalPTOHours: number;
  totalUnpaidHours: number;
  totalEmployees: number;
  totalExceptions: number;
  generatedAt: string;
}
