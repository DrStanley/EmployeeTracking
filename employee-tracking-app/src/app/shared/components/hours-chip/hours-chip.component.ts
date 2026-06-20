import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-hours-chip',
  standalone: true,
  imports: [CommonModule],
  template: `<span class="chip" [ngClass]="cssClass">{{ display }}</span>`,
  styles: [`
    .chip {
      display: inline-flex;
      align-items: center;
      padding: 2px 10px;
      border-radius: 9999px;
      font-size: 13px;
      font-weight: 600;
      font-family: 'SF Mono', monospace;
    }
    .green  { background: #dcfce7; color: #15803d; }
    .yellow { background: #fef9c3; color: #ca8a04; }
    .red    { background: #fee2e2; color: #dc2626; }
    .gray   { background: #f1f5f9; color: #475569; }
  `]
})
export class HoursChipComponent {
  @Input() hours  = 0;
  @Input() threshold = 40;

  get display(): string { return `${this.hours.toFixed(1)}h`; }
  get cssClass(): string {
    if (this.hours === 0) return 'gray';
    if (this.hours > this.threshold) return 'red';
    if (this.hours >= this.threshold * 0.8) return 'yellow';
    return 'green';
  }
}
