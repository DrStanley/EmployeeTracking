import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loading-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    @switch (type) {
      @case ('card') {
        <div class="skeleton-card">
          <div class="skel skel-title"></div>
          <div class="skel skel-line"></div>
          <div class="skel skel-line short"></div>
        </div>
      }
      @case ('table') {
        <div class="skeleton-table">
          <div class="skel skel-row header"></div>
          @for (i of rows; track i) {
            <div class="skel skel-row"></div>
          }
        </div>
      }
      @case ('list') {
        <div class="skeleton-list">
          @for (i of rows; track i) {
            <div class="skel-list-item">
              <div class="skel skel-avatar"></div>
              <div class="skel-lines">
                <div class="skel skel-line"></div>
                <div class="skel skel-line short"></div>
              </div>
            </div>
          }
        </div>
      }
      @default {
        <div class="skeleton-text">
          <div class="skel skel-line"></div>
          <div class="skel skel-line"></div>
          <div class="skel skel-line short"></div>
        </div>
      }
    }
  `,
  styles: [`
    .skel {
      background: linear-gradient(90deg, var(--color-border) 25%, var(--color-background) 50%, var(--color-border) 75%);
      background-size: 1000px 100%;
      animation: shimmer 1.5s infinite;
      border-radius: 6px;
    }
    @keyframes shimmer {
      0%   { background-position: -1000px 0; }
      100% { background-position:  1000px 0; }
    }
    .skel-title  { height: 24px; width: 40%; margin-bottom: 16px; }
    .skel-line   { height: 16px; width: 100%; margin-bottom: 10px; }
    .skel-line.short { width: 60%; }
    .skeleton-card { padding: 24px; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: 12px; }
    .skel-row    { height: 48px; margin-bottom: 4px; border-radius: 4px; }
    .skel-row.header { height: 44px; opacity: 0.6; }
    .skel-list-item { display: flex; gap: 12px; align-items: center; padding: 12px 0; border-bottom: 1px solid var(--color-border); }
    .skel-avatar { width: 40px; height: 40px; border-radius: 50%; flex-shrink: 0; }
    .skel-lines  { flex: 1; display: flex; flex-direction: column; gap: 8px; }
  `]
})
export class LoadingSkeletonComponent {
  @Input() type: 'card' | 'table' | 'list' | 'text' = 'text';
  @Input() count = 5;
  get rows(): number[] { return Array.from({ length: this.count }, (_, i) => i); }
}
