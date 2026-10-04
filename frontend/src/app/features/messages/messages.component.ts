import { Component, OnDestroy, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink, RouterOutlet } from '@angular/router';
import { Subscription, filter, firstValueFrom } from 'rxjs';

import { SwapRequestDto, SwapRequestStatus } from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { SwapRequestsService } from '../../core/services/swap-requests.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

interface ConversationRow {
  swapId: string;
  conversationId: string;
  otherName: string;
  skillsLine: string;
  status: number;
}

@Component({
  selector: 'app-messages',
  imports: [RouterLink, RouterOutlet, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <div class="msg-layout" [class.chat-open]="chatOpen()">
        <aside class="msg-list">
          <h2 class="page-title">Messages</h2>
          <p class="page-sub">Chats with your accepted swap partners.</p>

          @if (error() && !rows().length) {
            <div class="state-box">
              <p>{{ error() }}</p>
              <button class="btn btn-soft btn-sm" (click)="reload()">Try again</button>
            </div>
          } @else if (loading()) {
            <div class="state-box"><app-spinner [size]="32" /></div>
          } @else if (!rows().length) {
            <div class="state-box">
              <div class="empty-icon">
                <svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
              </div>
              <p>No conversations yet. Accept a swap request to start chatting.</p>
              <a class="btn btn-primary btn-sm" routerLink="/swaps">Go to my swaps</a>
            </div>
          } @else {
            <div class="list">
              @for (row of rows(); track row.conversationId) {
                <a class="convo-row card" [routerLink]="['/messages', row.swapId]" routerLinkActive="active">
                  <span class="avatar">{{ initials(row.otherName) }}</span>
                  <div class="convo-main">
                    <div class="convo-head">
                      <b>{{ row.otherName }}</b>
                      <span class="status" [class.completed]="row.status === Status.Completed">{{ row.status === Status.Completed ? 'Completed' : 'Active' }}</span>
                    </div>
                    <p class="skills">{{ row.skillsLine }}</p>
                  </div>
                  <svg class="chevron" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m9 18 6-6-6-6"/></svg>
                </a>
              }
            </div>
          }
        </aside>

        <section class="msg-chat">
          <router-outlet />
          @if (!chatOpen()) {
            <div class="no-chat">
              <svg width="34" height="34" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
              <p>Select a conversation to start chatting.</p>
            </div>
          }
        </section>
      </div>
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .page-title { font-size: 21px; font-weight: 700; margin: 4px 0 2px; }
    .page-sub { font-size: 13px; color: var(--text-secondary); margin: 0 0 16px; }
    .list { display: flex; flex-direction: column; gap: 12px; }
    .convo-row { display: flex; align-items: center; gap: 12px; padding: 14px 16px; text-decoration: none; color: inherit; transition: box-shadow 0.2s ease, transform 0.2s ease; }
    .convo-row:hover { box-shadow: 0 8px 24px rgba(31, 31, 61, 0.1); transform: translateY(-1px); }
    .avatar { width: 46px; height: 46px; border-radius: 50%; background: var(--gradient); color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: 15px; font-weight: 700; flex-shrink: 0; }
    .convo-main { flex: 1; min-width: 0; }
    .convo-head { display: flex; align-items: center; justify-content: space-between; gap: 8px; font-size: 14px; }
    .status { font-size: 10px; font-weight: 600; padding: 3px 9px; border-radius: 999px; background: var(--success-light); color: var(--success); }
    .status.completed { background: var(--primary-light); color: var(--primary); }
    .skills { font-size: 12px; color: var(--text-secondary); margin: 4px 0 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .chevron { color: var(--text-muted); flex-shrink: 0; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }
    .empty-icon { width: 76px; height: 76px; border-radius: 50%; background: var(--primary-light); color: var(--primary); display: flex; align-items: center; justify-content: center; }
    .no-chat { display: none; }

    .msg-layout.chat-open .msg-list { display: none; }
    .msg-layout.chat-open .msg-chat { display: block; }

    @media (min-width: 900px) {
      .msg-layout { display: grid; grid-template-columns: 340px 1fr; gap: 24px; align-items: start; }
      .msg-layout .msg-list { display: block; }
      .msg-layout.chat-open .msg-list { display: block; }
      .msg-chat {
        display: block;
        position: sticky;
        top: 74px;
        height: calc(100dvh - 130px);
      }
      .no-chat {
        display: flex; flex-direction: column; align-items: center; justify-content: center;
        gap: 12px; height: 100%; min-height: 320px; text-align: center;
        color: var(--text-muted); font-size: 14px;
        border: 1.5px dashed var(--border); border-radius: var(--radius-lg);
      }
      .convo-row.active { border-color: var(--primary); box-shadow: 0 0 0 1.5px var(--primary); }
    }
  `,
})
export class MessagesComponent implements OnInit, OnDestroy {
  private readonly auth = inject(AuthService);
  private readonly swapRequests = inject(SwapRequestsService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  protected readonly Status = SwapRequestStatus;

  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly rows = signal<ConversationRow[]>([]);
  protected readonly chatOpen = signal(false);
  private routerEvents: Subscription | null = null;

  protected initials(name: string): string {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0]?.toUpperCase() ?? '')
      .join('');
  }

  async ngOnInit(): Promise<void> {
    this.refreshChatOpen();
    this.routerEvents = this.router.events
      .pipe(filter((e) => e instanceof NavigationEnd))
      .subscribe(() => this.refreshChatOpen());
    await this.reload();
  }

  ngOnDestroy(): void {
    this.routerEvents?.unsubscribe();
  }

  private refreshChatOpen(): void {
    const open = this.route.firstChild !== null;
    const wasOpen = this.chatOpen();
    this.chatOpen.set(open);
    if (wasOpen && !open) void this.reload();
  }

  protected async reload(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const [accepted, completed] = await Promise.all([
        firstValueFrom(this.swapRequests.list(SwapRequestStatus.Accepted, 1, 50)),
        firstValueFrom(this.swapRequests.list(SwapRequestStatus.Completed, 1, 50)),
      ]);
      const myId = this.auth.user()?.userId;
      const swaps = [...accepted.items, ...completed.items].filter(
        (s): s is SwapRequestDto & { conversationId: string } =>
          s.conversationId !== null && s.conversationId !== undefined
      );
      this.rows.set(
        swaps
          .sort((a, b) => Date.parse(b.updatedAtUtc ?? b.createdAtUtc) - Date.parse(a.updatedAtUtc ?? a.createdAtUtc))
          .map((s) => ({
            swapId: s.id,
            conversationId: s.conversationId as string,
            otherName:
              s.requesterId === myId
                ? `${s.receiverFirstName} ${s.receiverLastName}`
                : `${s.requesterFirstName} ${s.requesterLastName}`,
            skillsLine: `${s.offeredSkill.skillName} ↔ ${s.requestedSkill.skillName}`,
            status: s.status,
          }))
      );
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }
}
