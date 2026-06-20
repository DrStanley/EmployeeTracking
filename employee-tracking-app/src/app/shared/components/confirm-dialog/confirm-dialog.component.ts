import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmLabel?: string;
  confirmColor?: 'primary' | 'warn' | 'accent';
  cancelLabel?: string;
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule],
  template: `
    <div class="dialog-wrap">
      <div class="dialog-header">
        <span class="material-icons icon">warning_amber</span>
        <h2>{{ data.title }}</h2>
      </div>
      <mat-dialog-content>
        <p>{{ data.message }}</p>
      </mat-dialog-content>
      <mat-dialog-actions align="end">
        <button mat-button (click)="close(false)">
          {{ data.cancelLabel || 'Cancel' }}
        </button>
        <button mat-flat-button [color]="data.confirmColor || 'warn'" (click)="close(true)">
          {{ data.confirmLabel || 'Confirm' }}
        </button>
      </mat-dialog-actions>
    </div>
  `,
  styles: [`
    .dialog-wrap { padding: 8px; min-width: 360px; max-width: 480px; }
    .dialog-header {
      display: flex; align-items: center; gap: 12px;
      padding: 16px 24px 8px;
      .icon { color: var(--color-warning); font-size: 28px; }
      h2   { font-size: 18px; font-weight: 600; color: var(--color-text-primary); margin: 0; }
    }
    mat-dialog-content p { color: var(--color-text-secondary); font-size: 14px; margin: 0; }
    mat-dialog-actions { padding: 16px 24px; gap: 8px; }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}
  close(result: boolean): void { this.dialogRef.close(result); }
}
