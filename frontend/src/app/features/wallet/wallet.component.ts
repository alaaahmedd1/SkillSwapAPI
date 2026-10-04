import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { WalletBalanceDto, WalletTransactionDto } from '../../core/models/domain.models';
import { WalletService } from '../../core/services/wallet.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { InsufficientCreditsComponent } from './insufficient-credits.component';
import { TopUpDialogComponent } from './top-up-dialog.component';

function formatHours(minutes: number): string {
  const sign = minutes < 0 ? '-' : '';
  const abs = Math.abs(minutes);
  const h = Math.floor(abs / 60);
  const m = abs % 60;
  return m ? `${sign}${h}h ${m}m` : `${sign}${h}h`;
}

@Component({
  selector: 'app-wallet',
  imports: [
    DatePipe,
    RouterLink,
    AppShellComponent,
    LoadingSpinnerComponent,
    TopUpDialogComponent,
  ],
  template: `
    <app-shell>
      <h2 class="page-title">Time <span>Wallet</span></h2>
      <p class="page-sub">Your time-credit balance for teaching and learning.</p>

      @if (error() && !balance()) {
        <div class="state-box">
          <p>{{ error() }}</p>
          <button class="btn btn-soft btn-sm" (click)="reload()">Try again</button>
        </div>
      } @else if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (balance(); as b) {
        <section class="card balance-card">
          <div class="ring-wrap">
            <div class="ring" [style.--pct]="ringPct()">
              <div class="ring-inner">
                <b class="ring-value">{{ formatBalance() }}</b>
                <small>Available balance</small>
              </div>
            </div>
            <p class="worth">
              Worth {{ freeSessions() }} free learning session{{ freeSessions() === 1 ? '' : 's' }}
            </p>
          </div>

          <div class="tiles">
            <div class="tile">
              <span class="tile-head earned">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 19V5m-7 7 7-7 7 7"/></svg>
                Earned
              </span>
              <b class="tile-value earned">+{{ formatHours(b.totalEarnedMinutes) }}</b>
              <small>total earned</small>
            </div>
            <div class="tile">
              <span class="tile-head spent">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 5v14m7-7-7 7-7-7"/></svg>
                Spent
              </span>
              <b class="tile-value spent">-{{ formatHours(b.totalSpentMinutes) }}</b>
              <small>skills learned</small>
            </div>
          </div>

          <div class="actions">
            <button class="btn btn-primary" (click)="openTopUp()">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 8v8m-4-4h8"/></svg>
              Top-Up Credits
            </button>
            <a class="btn btn-outline" routerLink="/profile">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 3v18M3 12h18"/></svg>
              Teach &amp; Earn
            </a>
          </div>
        </section>

        <section class="card">
          <div class="section-head">
            <h3>Recent Time Swaps</h3>
            <a class="text-link" routerLink="/wallet/transactions">See All</a>
          </div>

          @if (recentLoading()) {
            <div class="state-box tight"><app-spinner [size]="22" /></div>
          } @else if (!recent().length) {
            <p class="empty-line">No transactions yet — teach a skill or top up to get started.</p>
          } @else {
            <div class="tx-list">
              @for (tx of recent(); track tx.id) {
                <a class="tx-row" [routerLink]="['/wallet/transactions', tx.id]">
                  <span class="tx-icon" [class.out]="tx.amountMinutes < 0">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M7 17 17 7M17 7H8m9 0v9"/></svg>
                  </span>
                  <div class="tx-main">
                    <b>{{ tx.title }}</b>
                    <small>{{ tx.createdAtUtc | date: 'MMM d, h:mm a' }}</small>
                  </div>
                  <b class="tx-amount" [class.out]="tx.amountMinutes < 0">
                    {{ tx.amountMinutes < 0 ? '' : '+' }}{{ formatHours(tx.amountMinutes) }}
                  </b>
                </a>
              }
            </div>
          }
        </section>
      }

      @if (topUpOpen()) {
        <app-top-up-dialog [requiredMinutes]="0" (closed)="onTopUpClosed()" />
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .page-title { font-size: 21px; font-weight: 700; margin: 4px 0 2px; }
    .page-title span { background: var(--gradient); -webkit-background-clip: text; background-clip: text; color: transparent; }
    .page-sub { font-size: 13px; color: var(--text-secondary); margin: 0 0 16px; }

    .balance-card { padding: 26px 20px; display: flex; flex-direction: column; gap: 18px; }
    .actions { display: flex; flex-direction: column; gap: 10px; }
    .ring-wrap { display: flex; flex-direction: column; align-items: center; gap: 10px; }
    .ring {
      --pct: 0.5;
      width: 168px; height: 168px; border-radius: 50%;
      background: conic-gradient(var(--primary) calc(var(--pct) * 100%), var(--primary-light) 0);
      display: flex; align-items: center; justify-content: center;
    }
    .ring-inner {
      width: 138px; height: 138px; border-radius: 50%; background: #fff;
      display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 3px; text-align: center;
    }
    .ring-value { font-size: 24px; font-weight: 700; }
    .ring-inner small { font-size: 11px; color: var(--text-muted); }
    .worth { font-size: 11.5px; color: var(--text-muted); margin: 0; }

    .tiles { display: grid; grid-template-columns: 1fr 1fr; gap: 12px; }
    .tile {
      background: var(--primary-soft); border-radius: var(--radius-md);
      padding: 14px; display: flex; flex-direction: column; gap: 3px; align-items: flex-start;
    }
    .tile-head { display: inline-flex; align-items: center; gap: 5px; font-size: 11.5px; font-weight: 600; color: var(--text-muted); }
    .tile-head.earned { color: var(--success); }
    .tile-head.spent { color: var(--danger); }
    .tile-value { font-size: 18px; font-weight: 700; }
    .tile-value.earned { color: var(--success); }
    .tile-value.spent { color: var(--danger); }
    .tile small { font-size: 10.5px; color: var(--text-muted); }

    .section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 10px; }
    .section-head h3 { font-size: 15.5px; font-weight: 700; margin: 0; }

    .tx-list { display: flex; flex-direction: column; }
    .tx-row {
      display: flex; align-items: center; gap: 12px; padding: 12px 2px;
      border-bottom: 1px solid var(--border); color: inherit;
    }
    .tx-row:last-child { border-bottom: none; }
    .tx-icon {
      width: 36px; height: 36px; border-radius: 50%; background: var(--success-light); color: var(--success);
      display: inline-flex; align-items: center; justify-content: center; flex-shrink: 0;
    }
    .tx-icon.out { background: var(--danger-light); color: var(--danger); }
    .tx-main { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 2px; }
    .tx-main b { font-size: 13px; font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .tx-main small { font-size: 11px; color: var(--text-muted); }
    .tx-amount { font-size: 13px; font-weight: 700; color: var(--success); }
    .tx-amount.out { color: var(--danger); }

    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }
    .state-box.tight { padding: 16px; }
    .empty-line { font-size: 12.5px; color: var(--text-muted); margin: 4px 0; }

    @media (min-width: 900px) {
      .page-title { font-size: 26px; }
      .balance-card { max-width: 560px; }
      .actions { flex-direction: row; }
      .actions .btn { width: auto; }
      .tx-row { border-radius: var(--radius-sm); padding: 12px 10px; transition: background 0.15s ease; }
      .tx-row:hover { background: var(--primary-soft); }
      .text-link:hover { text-decoration: underline; }
    }
  `,
})
export class WalletComponent implements OnInit {
  private readonly wallet = inject(WalletService);

  protected readonly balance = signal<WalletBalanceDto | null>(null);
  protected readonly recent = signal<WalletTransactionDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly recentLoading = signal(false);
  protected readonly error = signal('');
  protected readonly topUpOpen = signal(false);

  protected readonly ringPct = computed(() => {
    const b = this.balance();
    if (!b) return 0.5;
    const total = b.totalEarnedMinutes;
    return total > 0 ? Math.max(0.06, Math.min(1, b.balanceMinutes / total)) : 0.5;
  });

  protected readonly freeSessions = computed(() => {
    const b = this.balance();
    return b ? Math.floor(b.balanceMinutes / 60) : 0;
  });

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  protected formatBalance(): string {
    const b = this.balance();
    return b ? formatHours(b.balanceMinutes) : '0h';
  }

  protected formatHours(minutes: number): string {
    return formatHours(minutes);
  }

  protected openTopUp(): void {
    this.topUpOpen.set(true);
  }

  protected onTopUpClosed(): void {
    this.topUpOpen.set(false);
    void this.reload();
  }

  private async reload(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    this.recentLoading.set(true);
    try {
      const [balance, recent] = await Promise.all([
        firstValueFrom(this.wallet.getBalance()),
        firstValueFrom(this.wallet.getTransactions(0, 1, 3)),
      ]);
      this.balance.set(balance);
      this.recent.set(recent.items);
      this.error.set('');
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
      this.recentLoading.set(false);
    }
  }
}
