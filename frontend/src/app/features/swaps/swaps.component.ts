import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { SwapRequestDto, SwapRequestStatus } from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { SwapRequestsService } from '../../core/services/swap-requests.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

const STATUS_META: Record<number, { label: string; className: string }> = {
  [SwapRequestStatus.Pending]: { label: 'Pending', className: 'pending' },
  [SwapRequestStatus.Accepted]: { label: 'Accepted', className: 'accepted' },
  [SwapRequestStatus.Rejected]: { label: 'Rejected', className: 'rejected' },
  [SwapRequestStatus.Cancelled]: { label: 'Cancelled', className: 'rejected' },
  [SwapRequestStatus.Completed]: { label: 'Completed', className: 'completed' },
};

const TABS = [
  { label: 'All', status: 0 },
  { label: 'Pending', status: SwapRequestStatus.Pending },
  { label: 'Accepted', status: SwapRequestStatus.Accepted },
  { label: 'Completed', status: SwapRequestStatus.Completed },
];

@Component({
  selector: 'app-swaps',
  imports: [DatePipe, RouterLink, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <h2 class="page-title">My <span>Swaps</span></h2>
      <p class="page-sub">Track requests you sent and received.</p>

      <div class="chips-row">
        @for (tab of tabs; track tab.status) {
          <button class="chip" [class.active]="activeTab() === tab.status" (click)="selectTab(tab.status)">{{ tab.label }}</button>
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
        <div class="state-box">
          <img src="/assets/illustrations/onboarding-matching.png" alt="" class="state-art" />
          <p>No {{ tabLabel() }} swaps yet.</p>
          <a class="btn btn-primary btn-sm" routerLink="/home">Find people to swap with</a>
        </div>
      }

      <div class="list">
        @for (swap of items(); track swap.id) {
          <a class="swap-row card" [routerLink]="['/swaps', swap.id]">
            <div class="row-head">
              <b>{{ otherName(swap) }}</b>
              <span class="status" [class]="statusMeta(swap.status).className">{{ statusMeta(swap.status).label }}</span>
            </div>
            <span class="direction" [class.incoming]="!isMine(swap)">{{ isMine(swap) ? 'Outgoing' : 'Incoming' }}</span>
            <p class="skills-line">
              You give <b>{{ swap.offeredSkill.skillName }}</b> · You get <b>{{ swap.requestedSkill.skillName }}</b>
            </p>
            <small class="date">{{ swap.createdAtUtc | date: 'MMM d, yyyy' }}</small>
          </a>
        }
      </div>

      @if (items().length && hasMore()) {
        <div class="more">
          <button class="btn btn-outline btn-sm" [disabled]="loading()" (click)="loadMore()">
            @if (loading()) { <app-spinner [size]="16" /> } @else { Load more }
          </button>
        </div>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .page-title { font-size: 21px; font-weight: 700; margin: 4px 0 2px; }
    .page-title span { background: var(--gradient); -webkit-background-clip: text; background-clip: text; color: transparent; }
    .page-sub { font-size: 13px; color: var(--text-secondary); margin: 0 0 16px; }
    .chips-row { display: flex; gap: 8px; overflow-x: auto; padding: 2px 2px 14px; margin: 0 -2px; scrollbar-width: none; }
    .chips-row::-webkit-scrollbar { display: none; }
    .list { display: flex; flex-direction: column; gap: 12px; }
    .swap-row { display: block; padding: 16px; text-decoration: none; color: inherit; }
    .row-head { display: flex; align-items: center; justify-content: space-between; gap: 8px; font-size: 14.5px; }
    .status { font-size: 11px; font-weight: 600; padding: 4px 10px; border-radius: 999px; }
    .status.pending { background: #fff6e5; color: var(--warning); }
    .status.accepted { background: var(--success-light); color: var(--success); }
    .status.completed { background: var(--primary-light); color: var(--primary); }
    .status.rejected { background: var(--danger-light); color: var(--danger); }
    .direction { font-size: 11px; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.4px; }
    .direction.incoming { color: var(--primary); }
    .skills-line { font-size: 12.5px; color: var(--text-secondary); margin: 8px 0 6px; }
    .skills-line b { color: var(--text); font-weight: 600; }
    .date { font-size: 11px; color: var(--text-muted); }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }
    .state-art { width: 150px; height: 150px; object-fit: cover; border-radius: var(--radius-lg); }
    .more { display: flex; justify-content: center; margin-top: 18px; }
  `,
})
export class SwapsComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly swapRequests = inject(SwapRequestsService);

  protected readonly tabs = TABS;
  protected readonly activeTab = signal(0);
  protected readonly items = signal<SwapRequestDto[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  private pageNumber = 0;
  private totalPages = 0;

  protected readonly hasMore = computed(() => this.pageNumber < this.totalPages);

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  protected async selectTab(status: number): Promise<void> {
    if (this.activeTab() === status) return;
    this.activeTab.set(status);
    await this.reload();
  }

  protected async reload(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const status = this.activeTab();
      const result = await firstValueFrom(
        this.swapRequests.list(status || undefined, 1, 10)
      );
      this.items.set(result.items);
      this.pageNumber = result.pageNumber;
      this.totalPages = result.totalPages;
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected async loadMore(): Promise<void> {
    if (this.loading()) return;
    this.loading.set(true);
    try {
      const status = this.activeTab();
      const result = await firstValueFrom(
        this.swapRequests.list(status || undefined, this.pageNumber + 1, 10)
      );
      this.items.update((list) => [...list, ...result.items]);
      this.pageNumber = result.pageNumber;
      this.totalPages = result.totalPages;
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected isMine(swap: SwapRequestDto): boolean {
    return swap.requesterId === this.auth.user()?.userId;
  }

  protected otherName(swap: SwapRequestDto): string {
    return this.isMine(swap)
      ? `To ${swap.receiverFirstName} ${swap.receiverLastName}`
      : `From ${swap.requesterFirstName} ${swap.requesterLastName}`;
  }

  protected statusMeta(status: number): { label: string; className: string } {
    return STATUS_META[status] ?? { label: 'Unknown', className: 'pending' };
  }

  protected tabLabel(): string {
    return this.tabs.find((t) => t.status === this.activeTab())?.label.toLowerCase() ?? '';
  }
}
