export interface AppUsageItem {
  appName: string;
  hours: number;
  sessionCount: number;
  lastUsedAt: string;
  percent: number;
}

export interface AppUsageSessionItem {
  id: string;
  appName: string;
  startedAt: string;
  endedAt: string;
  hours: number;
  source: string;
}

export interface AppActivityReport {
  employeeId: string;
  employeeName: string;
  from: string;
  to: string;
  totalHours: number;
  appCount: number;
  sessionCount: number;
  apps: AppUsageItem[];
  sessions: AppUsageSessionItem[];
}

export interface UserActivitySummary {
  employeeId: string;
  employeeName: string;
  jobTitle: string;
  totalHours: number;
  appCount: number;
  topApp: string | null;
  topAppHours: number;
}

export interface RecordAppUsageRequest {
  appName: string;
  startedAt: string;
  endedAt: string;
  source: 'System' | 'Manual';
}

export interface RecordAppUsageResult {
  id: string;
  hours: number;
}

export type ActivityRangePreset = 'today' | 'week' | 'month';

export function activityRange(preset: ActivityRangePreset): { from: string; to: string } {
  const now = new Date();
  const start = new Date(now);

  if (preset === 'today') {
    start.setHours(0, 0, 0, 0);
  } else if (preset === 'week') {
    const day = start.getDay();
    const diff = day === 0 ? 6 : day - 1;
    start.setDate(start.getDate() - diff);
    start.setHours(0, 0, 0, 0);
  } else {
    start.setDate(1);
    start.setHours(0, 0, 0, 0);
  }

  return { from: start.toISOString(), to: now.toISOString() };
}

export function formatHours(hours: number): string {
  if (!hours || hours <= 0) return '0h';
  if (hours < 0.1) {
    const minutes = Math.max(1, Math.round(hours * 60));
    return `${minutes}m`;
  }
  const text = (Math.round(hours * 100) / 100).toFixed(2).replace(/\.?0+$/, '');
  return `${text}h`;
}
