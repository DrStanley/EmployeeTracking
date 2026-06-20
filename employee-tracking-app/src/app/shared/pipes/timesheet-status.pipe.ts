import { Pipe, PipeTransform } from '@angular/core';
import { TimesheetStatus } from '../../core/models/timesheet.models';

@Pipe({
  name: 'timesheetStatus',
  standalone: true
})
export class TimesheetStatusPipe implements PipeTransform {
  private readonly labels: Record<number, string> = {
    [TimesheetStatus.Draft]: 'Draft',
    [TimesheetStatus.Submitted]: 'Submitted',
    [TimesheetStatus.Approved]: 'Approved',
    [TimesheetStatus.Rejected]: 'Rejected',
    [TimesheetStatus.Locked]: 'Locked'
  };

  transform(value: TimesheetStatus | number): string {
    return this.labels[value] ?? 'Unknown';
  }
}
