export enum PTORequestStatus {
  Pending = 0,
  Approved = 1,
  Rejected = 2,
  Cancelled = 3
}

export interface PTORequest {
  id: string;
  employeeId: string;
  employeeFullName: string;
  startDate: string;
  endDate: string;
  hoursRequested: number;
  status: PTORequestStatus;
  notes?: string;
  reviewerNotes?: string;
  reviewedByFullName?: string;
  reviewedAt?: string;
  createdAt: string;
}

export interface PTOBalance {
  employeeId: string;
  employeeFullName: string;
  year: number;
  availableHours: number;
  usedHours: number;
  accruedHours: number;
}

export interface SubmitPTORequest {
  startDate: string;
  endDate: string;
  hoursRequested: number;
  notes?: string;
}

export interface ApprovePTORequest {
  notes?: string;
}

export interface RejectPTORequest {
  reason: string;
}
