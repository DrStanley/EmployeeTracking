import { Component, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ScreenReview, ScreenService } from '../../core/services/screen.service';
import { apiErrorMessage } from '../../core/http/api-error';
import { LoadingSkeletonComponent } from '../../shared/components/loading-skeleton/loading-skeleton.component';
import { EmptyStateComponent } from '../../shared/components/empty-state/empty-state.component';

@Component({
  selector: 'app-screen-reviews',
  standalone: true,
  imports: [CommonModule, LoadingSkeletonComponent, EmptyStateComponent],
  templateUrl: './screen-reviews.component.html',
  styleUrl: './screen-reviews.component.scss'
})
export class ScreenReviewsComponent implements OnInit {
  private readonly screenApi = inject(ScreenService);

  loading = signal(true);
  errorMsg = signal<string | null>(null);
  reviews = signal<ScreenReview[]>([]);

  ngOnInit(): void {
    this.screenApi.reviews().subscribe({
      next: reviews => {
        this.reviews.set(reviews);
        this.loading.set(false);
      },
      error: err => {
        this.errorMsg.set(apiErrorMessage(err, 'Could not load screen reviews.'));
        this.loading.set(false);
      }
    });
  }
}
