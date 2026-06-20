import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-empty-state',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="empty-state">
      <div class="icon-wrap">
        <span class="material-icons">{{ icon }}</span>
      </div>
      <h3>{{ title }}</h3>
      <p>{{ message }}</p>
      @if (actionLabel) {
        <button class="btn btn-primary" (click)="actionFn && actionFn()">
          <span class="material-icons">add</span>
          {{ actionLabel }}
        </button>
      }
    </div>
  `,
  styles: [`
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 64px 32px;
      text-align: center;
      gap: 12px;
    }
    .icon-wrap {
      width: 72px; height: 72px;
      background: var(--color-background);
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      border: 1px solid var(--color-border);
      .material-icons { font-size: 32px; color: var(--color-text-muted); }
    }
    h3 { font-size: 16px; color: var(--color-text-primary); margin: 0; }
    p  { font-size: 14px; color: var(--color-text-muted); margin: 0; max-width: 320px; }
  `]
})
export class EmptyStateComponent {
  @Input() icon    = 'inbox';
  @Input() title   = 'No data found';
  @Input() message = 'Nothing to display here yet.';
  @Input() actionLabel?: string;
  @Input() actionFn?: () => void;
}
