import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { WalletTransactionDto } from '../../core/models/domain.models';
import { WalletService } from '../../core/services/wallet.service';
import { ToastService } from '../../core/services/toast.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-transaction-details',
  imports: [DatePipe, RouterLink, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <a class="icon-btn" topbar-actions routerLink="/wallet/transactions" aria-label="Back to history">
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 12H5m7-7-7 7 7 7"/></svg>
      </a>

      @if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (error()) {
        <div class="state-box">
          <p>{{ error() }}</p>
          <button class="btn btn-soft btn-sm" (click)="reload()">Try again</button>
        </div>
      } @else if (tx(); as t) {
        <section class="card head-card">
          <span class="pill" [class.out]="t.amountMinutes < 0">
            {{ signedLabel(t) }}
            {{ t.amountMinutes < 0 ? 'Spent' : t.transactionType === 'Purchased' ? 'Purchased' : 'Earned' }}
          </span>
          <h2>{{ t.title }}</h2>
          <p class="sub">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>
            {{ t.transactionType === 'Spent' ? 'Session completed & verified' : 'Status completed & verified' }}
          </p>
          <div class="balance-box">
            <span>Updated Wallet Balance</span>
            <b>{{ hoursLabel(t.runningBalanceMinutes) }} <small>hrs</small></b>
          </div>
        </section>

        @if (t.partnerUserId) {
          <section class="card partner-card">
            <span class="avatar">{{ initials(t) }}</span>
            <div class="partner-main">
              <b>Exchange Partner</b>
              <small>Verified member of this time swap</small>
            </div>
            <a class="btn btn-soft btn-sm" routerLink="/users/{{ t.partnerUserId }}">View</a>
          </section>
        }

        <section class="card">
          <h3 class="card-title">Exchange Summary</h3>
          <div class="rows">
            <div class="row"><span>Date &amp; Time</span><b>{{ t.createdAtUtc | date: 'MMM d, yyyy · h:mm a' }}</b></div>
            <div class="row"><span>Type</span><b>{{ t.transactionType }}</b></div>
            <div class="row"><span>Amount</span><b [class.out]="t.amountMinutes < 0">{{ signedLabel(t) }}</b></div>
            <div class="row"><span>Reference ID</span><b class="mono">{{ t.referenceCode }}</b></div>
            <div class="row"><span>Transaction ID</span><b class="mono small">{{ t.id }}</b></div>
          </div>

          <div class="note">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 16v-4m0-4h.01"/></svg>
            Time credits are verified by the SkillSwap community guarantee. This exchange cannot be modified once completed.
          </div>
        </section>

        @if (actionError()) { <p class="field-error">{{ actionError() }}</p> }

        <div class="actions">
          <button class="btn btn-primary" [disabled]="downloading()" (click)="downloadReceipt()">
            @if (downloading()) { <app-spinner [size]="16" /> } @else {
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><path d="M7 10l5 5 5-5"/><path d="M12 15V3"/></svg>
              Download PDF Receipt
            }
          </button>
          <button class="btn btn-outline" [disabled]="emailing()" (click)="emailReceipt()">
            @if (emailing()) { <app-spinner [size]="16" /> } @else { Email Receipt }
          </button>
        </div>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .icon-btn { width: 38px; height: 38px; display: inline-flex; align-items: center; justify-content: center; background: var(--input-bg); color: var(--text); border-radius: 12px; }

    .head-card { padding: 24px 20px; display: flex; flex-direction: column; align-items: center; text-align: center; gap: 8px; }
    .pill {
      display: inline-flex; font-size: 12px; font-weight: 700; padding: 6px 14px; border-radius: 999px;
      background: var(--success-light); color: var(--success);
    }
    .pill.out { background: var(--danger-light); color: var(--danger); }
    .head-card h2 { font-size: 17px; font-weight: 700; margin: 4px 0 0; }
    .sub { display: inline-flex; align-items: center; gap: 5px; font-size: 12px; color: var(--success); margin: 0; }
    .balance-box {
      margin-top: 10px; width: 100%; background: var(--primary-soft); border-radius: var(--radius-md);
      display: flex; align-items: center; justify-content: space-between; padding: 14px 16px;
    }
    .balance-box span { font-size: 12.5px; color: var(--text-secondary); }
    .balance-box b { font-size: 19px; font-weight: 700; color: var(--primary); }
    .balance-box small { font-size: 11px; color: var(--text-muted); font-weight: 500; }

    .partner-card { margin-top: 16px; padding: 16px; display: flex; align-items: center; gap: 12px; }
    .avatar { width: 42px; height: 42px; border-radius: 50%; background: var(--gradient); color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: 14px; font-weight: 700; }
    .partner-main { flex: 1; display: flex; flex-direction: column; gap: 2px; }
    .partner-main b { font-size: 13.5px; }
    .partner-main small { font-size: 11px; color: var(--text-muted); }

    .card { padding: 20px; margin-bottom: 16px; }
    .card-title { font-size: 15.5px; font-weight: 700; margin: 0 0 12px; }
    .rows { display: flex; flex-direction: column; }
    .row { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 10px 0; border-bottom: 1px solid var(--border); font-size: 13px; }
    .row:last-child { border-bottom: none; }
    .row span { color: var(--text-secondary); }
    .row b { font-weight: 600; text-align: right; }
    .row b.out { color: var(--danger); }
    .mono { font-family: ui-monospace, monospace; font-size: 12px; }
    .mono.small { font-size: 10.5px; word-break: break-all; }

    .note {
      display: flex; gap: 8px; margin-top: 14px; font-size: 11.5px; color: var(--text-secondary);
      background: var(--primary-soft); border-radius: var(--radius-sm); padding: 12px; line-height: 1.55;
    }
    .note svg { flex-shrink: 0; margin-top: 1px; color: var(--primary); }

    .actions { display: flex; flex-direction: column; gap: 10px; margin: 4px 0 12px; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }

    @media (min-width: 900px) {
      .card, .partner-card, .head-card { max-width: 640px; }
      .actions { flex-direction: row; }
      .actions .btn { width: auto; }
      .icon-btn { transition: background 0.15s ease, color 0.15s ease; }
      .icon-btn:hover { background: var(--primary-soft); color: var(--primary); }
    }
  `,
})
export class TransactionDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly wallet = inject(WalletService);
  private readonly toast = inject(ToastService);

  protected readonly tx = signal<WalletTransactionDto | null>(null);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly downloading = signal(false);
  protected readonly emailing = signal(false);
  protected readonly actionError = signal('');

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  protected signedLabel(t: WalletTransactionDto): string {
    const label = this.hoursLabel(Math.abs(t.amountMinutes));
    return t.amountMinutes < 0 ? `-${label}` : `+${label}`;
  }

  protected hoursLabel(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m ? `${h}h ${m}m` : `${h}.0`;
  }

  protected initials(t: WalletTransactionDto): string {
    return t.title
      .replace(/^(Session|Skill|with)\s+/i, '')
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0]?.toUpperCase() ?? '')
      .join('');
  }

  protected async reload(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const id = this.route.snapshot.paramMap.get('id') ?? '';
      this.tx.set(await firstValueFrom(this.wallet.getTransactionDetails(id)));
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected async downloadReceipt(): Promise<void> {
    const t = this.tx();
    if (!t || this.downloading()) return;
    this.downloading.set(true);
    this.actionError.set('');
    try {
      const blob = await firstValueFrom(this.wallet.getReceiptPdf(t.id));
      const url = URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `receipt-${t.referenceCode}.pdf`;
      a.click();
      URL.revokeObjectURL(url);
    } catch (err) {
      this.actionError.set(extractApiError(err).message);
    } finally {
      this.downloading.set(false);
    }
  }

  protected async emailReceipt(): Promise<void> {
    const t = this.tx();
    if (!t || this.emailing()) return;
    this.emailing.set(true);
    this.actionError.set('');
    try {
      await firstValueFrom(this.wallet.requestReceiptEmail(t.id));
      this.toast.success('Receipt emailed to you.');
    } catch (err) {
      this.actionError.set(extractApiError(err).message);
    } finally {
      this.emailing.set(false);
    }
  }
}
