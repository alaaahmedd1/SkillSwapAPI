import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-illustration-dialog',
  imports: [],
  template: `
    <div class="backdrop" (click)="onBackdropClick($event)">
      <div class="dialog-card" role="dialog" aria-modal="true">
        <img class="dialog-img" [src]="image()" [alt]="title()" />
        <h2>{{ title() }}</h2>
        <p>{{ text() }}</p>
        <button class="btn btn-primary" (click)="closed.emit()">{{ buttonText() }}</button>
      </div>
    </div>
  `,
  styles: `
    .backdrop {
      position: fixed;
      inset: 0;
      z-index: 900;
      background: rgba(31, 31, 61, 0.45);
      backdrop-filter: blur(4px);
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 28px;
      animation: fade-in 0.2s ease both;
    }

    .dialog-card {
      width: 100%;
      max-width: 320px;
      background: #fff;
      border-radius: 24px;
      padding: 30px 24px 24px;
      text-align: center;
      box-shadow: 0 24px 60px rgba(0, 0, 0, 0.25);
      animation: pop-in 0.28s cubic-bezier(0.34, 1.4, 0.64, 1) both;

      h2 {
        font-size: 20px;
        font-weight: 700;
        margin: 4px 0 10px;
      }

      p {
        font-size: 13.5px;
        color: var(--text-secondary);
        line-height: 1.6;
        margin: 0 0 22px;
      }
    }

    .dialog-img {
      width: 150px;
      height: 150px;
      object-fit: contain;
      margin: 0 auto 6px;
    }

    @keyframes fade-in {
      from { opacity: 0; }
      to { opacity: 1; }
    }

    @keyframes pop-in {
      from { opacity: 0; transform: scale(0.86) translateY(14px); }
      to { opacity: 1; transform: scale(1) translateY(0); }
    }
  `,
})
export class IllustrationDialogComponent {
  readonly image = input.required<string>();
  readonly title = input.required<string>();
  readonly text = input.required<string>();
  readonly buttonText = input.required<string>();
  readonly dismissOnBackdrop = input(true);
  readonly closed = output<void>();

  protected onBackdropClick(event: MouseEvent): void {
    if (this.dismissOnBackdrop() && event.target === event.currentTarget) {
      this.closed.emit();
    }
  }
}
