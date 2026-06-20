export enum NotificationType {
  MissedPunch = 0,
  PendingApproval = 1,
  TimesheetApproved = 2,
  TimesheetRejected = 3,
  LowPTOBalance = 4,
  OvertimeAlert = 5
}

export interface Notification {
  id: string;
  type: NotificationType;
  message: string;
  isRead: boolean;
  sentAt: string;
}
