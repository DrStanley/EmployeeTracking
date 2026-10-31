import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { WorkspaceService } from '../../core/services/workspace.service';
import { apiErrorMessage } from '../../core/http/api-error';
import { CalendarAbsence, CalendarHoliday } from '../../core/models/workspace.models';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';

interface CalendarCell {
  date: Date | null;
  key: string;
  holidays: CalendarHoliday[];
  absences: CalendarAbsence[];
}

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, MatIconModule, LoadingSkeletonComponent],
  templateUrl: './calendar.component.html',
  styleUrl: './calendar.component.scss'
})
export class CalendarComponent implements OnInit {
  private readonly workspace = inject(WorkspaceService);
  readonly weekdays = ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];

  cursor = signal(new Date());
  loading = signal(true);
  cells = signal<CalendarCell[]>([]);
  errorMsg = signal<string | null>(null);

  ngOnInit(): void {
    this.load();
  }

  isToday(date: Date | null): boolean {
    if (!date) return false;
    const now = new Date();
    return date.getFullYear() === now.getFullYear()
      && date.getMonth() === now.getMonth()
      && date.getDate() === now.getDate();
  }

  title(): string {
    return this.cursor().toLocaleDateString(undefined, { month: 'long', year: 'numeric' });
  }

  shift(months: number): void {
    const next = new Date(this.cursor());
    next.setMonth(next.getMonth() + months);
    this.cursor.set(next);
    this.load();
  }

  private load(): void {
    const cursor = this.cursor();
    this.loading.set(true);
    this.errorMsg.set(null);
    this.workspace.calendar(cursor.getFullYear(), cursor.getMonth() + 1).subscribe({
      next: month => {
        this.cells.set(this.buildCells(cursor.getFullYear(), cursor.getMonth(), month.holidays, month.absences));
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(apiErrorMessage(err, 'Could not load the calendar.'));
        this.loading.set(false);
      }
    });
  }

  private buildCells(
    year: number, monthIndex: number,
    holidays: CalendarHoliday[], absences: CalendarAbsence[]): CalendarCell[] {
    const first = new Date(year, monthIndex, 1);
    const lead = (first.getDay() + 6) % 7;
    const daysInMonth = new Date(year, monthIndex + 1, 0).getDate();
    const cells: CalendarCell[] = [];
    for (let i = 0; i < lead; i++) cells.push({ date: null, key: `pad-${i}`, holidays: [], absences: [] });
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(year, monthIndex, day);
      const key = this.keyFor(date);
      cells.push({
        date,
        key,
        holidays: holidays.filter(holiday => holiday.date.slice(0, 10) === key),
        absences: absences.filter(absence => key >= absence.startDate.slice(0, 10) && key <= absence.endDate.slice(0, 10))
      });
    }
    return cells;
  }

  private keyFor(date: Date): string {
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${date.getFullYear()}-${month}-${day}`;
  }
}
