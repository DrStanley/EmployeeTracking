export interface AttendancePolicy {
  id: string;
  name: string;
  dailyOvertimeThresholdHours: number;
  weeklyOvertimeThresholdHours: number;
  overtimeMultiplier: number;
  hasPaidBreaks: boolean;
  paidBreakMinutes: number;
  unpaidBreakMinutes: number;
  ptoAccrualRatePerPayPeriod: number;
  maxPTOBalanceHours: number;
}

export interface OvertimeRule {
  id: string;
  attendancePolicyId: string;
  attendancePolicyName: string;
  dailyThresholdHours: number;
  weeklyThresholdHours: number;
  premiumMultiplier: number;
  hasDoubleTime: boolean;
  doubleTimeDailyThreshold: number;
}

export interface Shift {
  id: string;
  name: string;
  plannedStart: string;
  plannedEnd: string;
  gracePeriodMinutes: number;
  isActive: boolean;
}

export interface Holiday {
  id: string;
  name: string;
  date: string;
  isRecurringAnnually: boolean;
}

export interface PayPeriod {
  id: string;
  name: string;
  startDate: string;
  endDate: string;
  isLocked: boolean;
}

export interface PTOAccrualSettings {
  attendancePolicyId: string;
  attendancePolicyName: string;
  accrualRatePerPayPeriod: number;
  maxBalanceHours: number;
  allowNegativeBalance: boolean;
  carryOverLimitHours: number;
}
