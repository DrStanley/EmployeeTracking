import { Component, Input, TemplateRef, ContentChild, signal, computed } from '@angular/core';
import { CommonModule, NgTemplateOutlet } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { EmptyStateComponent } from '../empty-state/empty-state.component';
import { LoadingSkeletonComponent } from '../loading-skeleton/loading-skeleton.component';

export interface ColumnConfig<T> {
  key: string;
  label: string;
  sortable?: boolean;
  width?: string;
  accessor?: (row: T) => unknown;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  imports: [CommonModule, NgTemplateOutlet, MatIconModule, EmptyStateComponent, LoadingSkeletonComponent],
  templateUrl: './data-table.component.html',
  styleUrl: './data-table.component.scss'
})
export class DataTableComponent<T extends Record<string, any>> {
  @Input() columns: ColumnConfig<T>[] = [];
  @Input() data: T[] = [];
  @Input() loading = false;
  @Input() emptyTitle = 'No records found';
  @Input() emptyMessage = 'There is nothing to show here yet.';
  @Input() rowTemplate?: TemplateRef<{ $implicit: T }>;
  @Input() pageSize = 10;

  @ContentChild('rowTemplate') rowTpl?: TemplateRef<any>;
  @ContentChild('cellTemplate') cellTpl?: TemplateRef<any>;

  sortKey = signal<string | null>(null);
  sortDir = signal<'asc' | 'desc'>('asc');
  page = signal(0);

  sortedData = computed(() => {
    const key = this.sortKey();
    const dir = this.sortDir();
    if (!key) return this.data;
    return [...this.data].sort((a, b) => {
      const av = a[key];
      const bv = b[key];
      if (av < bv) return dir === 'asc' ? -1 : 1;
      if (av > bv) return dir === 'asc' ? 1 : -1;
      return 0;
    });
  });

  pagedData = computed(() => {
    const start = this.page() * this.pageSize;
    return this.sortedData().slice(start, start + this.pageSize);
  });

  totalPages = computed(() => Math.ceil(this.data.length / this.pageSize));

  toggleSort(col: ColumnConfig<T>): void {
    if (!col.sortable) return;
    if (this.sortKey() === col.key) {
      this.sortDir.set(this.sortDir() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortKey.set(col.key);
      this.sortDir.set('asc');
    }
  }

  nextPage(): void {
    if (this.page() < this.totalPages() - 1) this.page.update(p => p + 1);
  }

  prevPage(): void {
    if (this.page() > 0) this.page.update(p => p - 1);
  }

  cellValue(row: T, col: ColumnConfig<T>): unknown {
    return col.accessor ? col.accessor(row) : row[col.key];
  }
}
