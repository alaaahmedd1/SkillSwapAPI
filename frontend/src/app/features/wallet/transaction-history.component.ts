import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { WalletTransactionDto } from '../../core/models/domain.models';
import { WALLET_FILTER, WalletService } from '../../core/services/wallet.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-transaction-history',
  imports: [DatePipe, RouterLink, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <a class="icon-btn" topbar-actions routerLink="/wallet" aria-label="Back to wallet">
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 12H5m7-7-7 7 7 7"/></svg>
      </a>

      <h2 class="page-title">Transaction <span>History</span></h2>
      <p class="page-sub">Every hour you earned, spent or purchased.</p>

      <div class="filters">
        @for (f of filters; track f.value) {
          <button class="chip" [class.active]="filter() === f.value" (click)="setFilter(f.value)">{{ f.label }}</button>
        }
      </div>

      @if (error() && !items().length) {
        <div class="state-box">
          <p>{{ error() }}</p>
          <button class="btn btn-soft btn-sm" (click)="reload()">Try again</button>
        </div>
      } @else if (loading() && !items().length) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (!items().length) {
        <div class="empty-card card">
          <span class="empty-icon">
            <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></svg>
          </span>
          <h3>No {{ filterLabel() }} Hours Yet</h3>
          <p>You haven't {{ filterLabel() === 'Earned' ? 'earned' : filterLabel() === 'Spent' ? 'spent' : 'made' }} any time credits yet. Share your skills to start earning credits!</p>
          <a class="btn btn-primary btn-sm" routerLink="/profile">+ Teach &amp; Earn Your First Credit</a>
        </div>
      } @else {
        <div class="list">
          @for (tx of items(); track tx.id) {
            <a class="tx-row card" [routerLink]="['/wallet/transactions', tx.id]">
              <span class="tx-icon" [class.out]="tx.amountMinutes < 0">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M7 17 17 7M17 7H8m9 0v9"/></svg>
              </span>
              <div class="tx-main">
                <b>{{ tx.title }}</b>
                <small>{{ tx.createdAtUtc | date: "EEE, h:mm a" }}</small>
              </div>
              <b class="tx-amount" [class.out]="tx.amountMinutes < 0">
                {{ tx.amountMinutes < 0 ? '' : '+' }}{{ formatHours(tx.amountMinutes) }}
              </b>
            </a>
          }
        </div>

        <div class="more">
          @if (hasMore()) {
            <button class="btn btn-outline btn-sm" [disabled]="loading()" (click)="loadMore()">
              @if (loading()) { <app-spinner [size]="16" /> } @else { Load more }
            </button>
          }
        </div>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .icon-btn { width: 38px; height: 38px; display: inline-flex; align-items: center; justify-content: center; background: var(--input-bg); color: var(--text); border-radius: 12px; }
    .page-title { font-size: 21px; font-weight: 700; margin: 4px 0 2px; }
    .page-title span { background: var(--gradient); -webkit-background-clip: text; background-clip: text; color: transparent; }
    .page-sub { font-size: 13px; color: var(--text-secondary); margin: 0 0 16px; }

    .filters { display: flex; flex-direction: column; gap: 10px; margin-bottom: 16px; }
    .filters .chip { justify-content: center; padding: 11px; }

    .list { display: flex; flex-direction: column; gap: 10px; }
    .tx-row { display: flex; align-items: center; gap: 12px; padding: 14px 16px; color: inherit; }
    .tx-icon {
      width: 38px; height: 38px; border-radius: 50%; background: var(--success-light); color: var(--success);
      display: inline-flex; align-items: center; justify-content: center; flex-shrink: 0;
    }
    .tx-icon.out { background: var(--danger-light); color: var(--danger); }
    .tx-main { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 2px; }
    .tx-main b { font-size: 13.5px; font-weight: 600; }
    .tx-main small { font-size: 11px; color: var(--text-muted); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .tx-amount { font-size: 13.5px; font-weight: 700; color: var(--success); flex-shrink: 0; }
    .tx-amount.out { color: var(--danger); }

    .empty-card { display: flex; flex-direction: column; align-items: center; gap: 8px; text-align: center; padding: 44px 24px; }
    .empty-icon { width: 72px; height: 72px; border-radius: 50%; background: var(--primary-light); color: var(--primary); display: flex; align-items: center; justify-content: center; margin-bottom: 6px; }
    .empty-card h3 { font-size: 16px; font-weight: 700; margin: 0; }
    .empty-card p { font-size: 12.5px; color: var(--text-secondary); margin: 0 0 10px; line-height: 1.6; }

    .more { display: flex; justify-content: center; margin-top: 18px; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }
  `,
})
export class TransactionHistoryComponent implements OnInit {
  private readonly wallet = inject(WalletService);

  protected readonly filters = [
    { value: WALLET_FILTER.All, label: 'All' },
    { value: WALLET_FILTER.Earned, label: 'Earned' },
    { value: WALLET_FILTER.Spent, label: 'Spent' },
  ];

  protected readonly items = signal<WalletTransactionDto[]>([]);
  protected readonly filter = signal<number>(WALLET_FILTER.All);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  private page = 1;
  private totalPages = 0;

  protected readonly hasMore = signal(false);

  protected filterLabel(): string {
    return this.filters.find((f) => f.value === this.filter())?.label ?? 'All';
  }

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  protected formatHours(minutes: number): string {
    const h = Math.floor(Math.abs(minutes) / 60);
    const m = Math.abs(minutes) % 60;
    return m ? `${h}h ${m}m` : `${h}.0h`;
  }

  protected setFilter(value: number): void {
    if (this.filter() === value) return;
    this.filter.set(value);
    void this.reload();
  }

  protected reload(): void {
    void this.load(true);
  }

  protected loadMore(): void {
    void this.load(false);
  }

  private async load(reset: boolean): Promise<void> {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set('');
    const page = reset ? 1 : this.page + 1;
    try {
      const result = await firstValueFrom(
        this.wallet.getTransactions(this.filter(), page, PAGE_SIZE)
      );
      this.items.update((list) => (reset ? result.items : [...list, ...result.items]));
      this.page = result.pageNumber;
      this.totalPages = result.totalPages;
      this.hasMore.set(result.pageNumber < result.totalPages);
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }
}
