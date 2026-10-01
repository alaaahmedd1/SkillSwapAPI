import { Component, inject } from '@angular/core';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-toast-host',
  imports: [],
  template: `
    <div class="toast-stack" aria-live="polite">
      @for (toast of toastService.toasts(); track toast.id) {
        <div class="toast toast-{{ toast.kind }}" (click)="toastService.dismiss(toast.id)">
          <span class="toast-icon">
            @if (toast.kind === 'success') {
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>
            } @else if (toast.kind === 'error') {
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><path d="M18 6 6 18M6 6l12 12"/></svg>
            } @else {
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4M12 8h.01"/></svg>
            }
          </span>
          <p>{{ toast.message }}</p>
        </div>
      }
    </div>
  `,
  styles: `
    .toast-stack {
      position: fixed;
      top: 18px;
      left: 50%;
      transform: translateX(-50%);
      z-index: 1000;
      display: flex;
      flex-direction: column;
      gap: 10px;
      width: min(92vw, 440px);
      pointer-events: none;
    }

    .toast {
      pointer-events: auto;
      display: flex;
      align-items: flex-start;
      gap: 10px;
      background: #1f1f3d;
      color: #fff;
      border-radius: 14px;
      padding: 13px 16px;
      font-size: 13.5px;
      line-height: 1.45;
      box-shadow: 0 12px 30px rgba(0, 0, 0, 0.25);
      cursor: pointer;
      animation: toast-in 0.28s ease both;

      p {
        margin: 0;
      }
    }

    .toast-success .toast-icon { color: #5ee6a0; }
    .toast-error .toast-icon { color: #ff8a8a; }
    .toast-info .toast-icon { color: #b7a6ff; }

    .toast-icon {
      flex-shrink: 0;
      display: inline-flex;
      margin-top: 1px;
    }

    @keyframes toast-in {
      from { opacity: 0; transform: translateY(-12px) scale(0.97); }
      to { opacity: 1; transform: translateY(0) scale(1); }
    }
  `,
})
export class ToastHostComponent {
  protected readonly toastService = inject(ToastService);
}
