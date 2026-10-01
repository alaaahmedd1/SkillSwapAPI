import { Component, EventEmitter, Output, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-insufficient-credits',
  template: `
    <div class="overlay" (click)="close()">
      <div class="modal" (click)="$event.stopPropagation()">
        <span class="icon">
          <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M5 22h14"/><path d="M5 2h14"/><path d="M17 22v-4.172a2 2 0 0 0-.586-1.414L12 12l-4.414 4.414A2 2 0 0 0 7 17.828V22"/><path d="M7 2v4.172a2 2 0 0 0 .586 1.414L12 12l4.414-4.414A2 2 0 0 0 17 6.172V2"/></svg>
        </span>
        <h3>Insufficient Credits</h3>
        <p class="msg">
          You need {{ requiredLabel() }} to book this session, but you only have
          <b class="danger">{{ balanceLabel() }}</b> left in your Time Wallet.
        </p>

        <div class="rows">
          <div class="row"><span>Required session time</span><b>{{ requiredLabel() }}</b></div>
          <div class="row"><span>Your current balance</span><b class="danger">{{ balanceLabel() }}</b></div>
          @if (missing() > 0) {
            <div class="row"><span>Missing time</span><b class="warn">{{ missingLabel() }}</b></div>
          }
        </div>

        <button class="btn btn-primary" (click)="topUp.emit()">Top-Up Credits Now</button>
        <a class="btn btn-outline" routerLink="/profile">Teach &amp; Earn Hours for Free</a>
        <button class="cancel" (click)="close()">Cancel and go back</button>
      </div>
    </div>
  `,
  styles: `
    :host { position: fixed; inset: 0; z-index: 100; }
    .overlay {
      position: absolute; inset: 0; background: rgba(31, 31, 61, 0.45);
      display: flex; align-items: center; justify-content: center; padding: 24px;
    }
    .modal {
      width: 100%; max-width: 420px; background: #fff; border-radius: 24px;
      padding: 28px 22px; text-align: center;
      display: flex; flex-direction: column; gap: 8px;
      animation: pop 0.22s ease both;
    }
    @keyframes pop { from { transform: scale(0.95); opacity: 0; } to { transform: none; opacity: 1; } }
    .icon {
      width: 68px; height: 68px; border-radius: 50%; background: #fff7e8; color: var(--warning);
      display: flex; align-items: center; justify-content: center; margin: 0 auto 4px;
    }
    h3 { font-size: 17px; font-weight: 700; margin: 0; }
    .msg { font-size: 12.5px; color: var(--text-secondary); line-height: 1.6; margin: 0 0 8px; }
    .danger { color: var(--danger); }
    .warn { color: var(--warning); }

    .rows { background: var(--primary-soft); border-radius: var(--radius-md); padding: 4px 14px; margin-bottom: 12px; }
    .row { display: flex; align-items: center; justify-content: space-between; gap: 10px; padding: 10px 0; font-size: 12.5px; border-bottom: 1px solid var(--border); }
    .row:last-child { border-bottom: none; }
    .row span { color: var(--text-secondary); }
    .row b { font-weight: 600; }

    .btn + .btn { margin-top: 10px; }
    .cancel { background: none; border: none; color: var(--text-muted); font-size: 12.5px; cursor: pointer; margin-top: 12px; font-family: inherit; }
  `,
  imports: [RouterLink],
})
export class InsufficientCreditsComponent {
  readonly requiredMinutes = input.required<number>();
  readonly balanceMinutes = input(0);

  @Output() closed = new EventEmitter<void>();
  @Output() topUp = new EventEmitter<void>();

  protected readonly missing = computed(() => this.requiredMinutes() - this.balanceMinutes());

  protected requiredLabel(): string {
    return label(this.requiredMinutes());
  }

  protected balanceLabel(): string {
    return label(this.balanceMinutes());
  }

  protected missingLabel(): string {
    return label(Math.max(0, this.missing()));
  }

  protected close(): void {
    this.closed.emit();
  }
}

function label(minutes: number): string {
  const h = Math.floor(minutes / 60);
  const m = minutes % 60;
  if (h && m) return `${h}h ${m}m`;
  if (h) return `${h}h`;
  return `${m}m`;
}
