import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-page-header',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="page-header">
      <div class="header-left">
        @if (icon) {
          <div class="icon-wrap">
            <span class="material-icons">{{ icon }}</span>
          </div>
        }
        <div>
          <h1>{{ title }}</h1>
          @if (subtitle) { <p class="subtitle">{{ subtitle }}</p> }
        </div>
      </div>
      <div class="header-actions">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      margin-bottom: 32px;
      flex-wrap: wrap;
      gap: 16px;
    }
    .header-left {
      display: flex;
      align-items: center;
      gap: 16px;
    }
    .icon-wrap {
      width: 48px; height: 48px;
      background: var(--color-primary-light);
      border-radius: 12px;
      display: flex; align-items: center; justify-content: center;
      .material-icons { color: var(--color-primary); font-size: 24px; }
    }
    h1 { font-size: 24px; font-weight: 700; color: var(--color-text-primary); margin: 0; }
    .subtitle { font-size: 14px; color: var(--color-text-muted); margin: 4px 0 0; }
    .header-actions { display: flex; align-items: center; gap: 8px; }
  `]
})
export class PageHeaderComponent {
  @Input() title    = '';
  @Input() subtitle = '';
  @Input() icon     = '';
}
