import { Component, input } from '@angular/core';

@Component({
  selector: 'app-spinner',
  imports: [],
  template: `
    <span class="spinner" [style.width.px]="size()" [style.height.px]="size()"></span>
  `,
  styles: `
    .spinner {
      display: inline-block;
      border-radius: 50%;
      border: 3px solid var(--primary-light);
      border-top-color: var(--primary);
      animation: spin 0.8s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `,
})
export class LoadingSpinnerComponent {
  readonly size = input(28);
}
