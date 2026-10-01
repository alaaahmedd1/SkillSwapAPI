import { Component, computed, input } from '@angular/core';

@Component({
  selector: 'app-rating-stars',
  imports: [],
  template: `
    <span class="stars" [attr.aria-label]="rating() + ' out of 5 stars'">
      @for (filled of stars(); track $index) {
        <svg viewBox="0 0 24 24" width="13" height="13" [class.filled]="filled">
          <path d="M12 2.6l2.83 5.74 6.34.92-4.59 4.47 1.08 6.31L12 17.26l-5.66 2.78 1.08-6.31L2.83 9.26l6.34-.92z" />
        </svg>
      }
      @if (count() !== null) {
        <span class="count">{{ count() }} review{{ count() === 1 ? '' : 's' }}</span>
      }
    </span>
  `,
  styles: `
    :host { display: inline-flex; }
    .stars { display: inline-flex; align-items: center; gap: 2px; }
    svg { fill: var(--border); }
    svg.filled { fill: var(--star); }
    .count { font-size: 11.5px; color: var(--text-muted); margin-left: 5px; }
  `,
})
export class RatingStarsComponent {
  readonly rating = input(0);
  readonly count = input<number | null>(null);

  protected readonly stars = computed(() => {
    const rounded = Math.round(this.rating());
    return Array.from({ length: 5 }, (_, i) => i < rounded);
  });
}
