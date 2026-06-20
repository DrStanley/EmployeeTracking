export enum TimesheetStatus {
  Draft = 0,
  Submitted = 1,
  Approved = 2,
  Rejected = 3,
  Locked = 4
}

export enum ApprovalDecision {
  Approved = 0,
  Rejected = 1,
  ReturnedForCorrection = 2
}

export interface TimesheetLine {
  id: string;
  timesheetId: string;
  workDate: string;
  regularHours: number;
  overtimeHours: number;
  breakHours: number;
  ptoHours: number;
  notes?: string;
  totalHours: number;
}

export interface ApprovalAction {
  reviewerId: string;
  reviewerFullName: string;
  decision: ApprovalDecision;
  notes?: string;
  decidedAt: string;
}

export interface Timesheet {
  id: string;
  employeeId: string;
  employeeFullName: string;
  payPeriodId: string;
  payPeriodName: string;
  status: TimesheetStatus;
  totalRegularHours: number;
  totalOvertimeHours: number;
  totalPTOHours: number;
  totalUnpaidHours: number;
  submittedAt?: string;
  approvedAt?: string;
  rejectionReason?: string;
  lines: TimesheetLine[];
  approvalHistory: ApprovalAction[];
}

export interface CreateTimesheetRequest {
  employeeId: string;
  payPeriodId: string;
}

export interface ApproveTimesheetRequest {
  notes?: string;
}

export interface RejectTimesheetRequest {
  reason: string;
}
