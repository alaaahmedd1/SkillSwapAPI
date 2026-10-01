import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, effect, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { MessageDto, SwapRequestStatus } from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { ChatHubService } from '../../core/services/chat-hub.service';
import { ConversationsService } from '../../core/services/conversations.service';
import { SwapRequestsService } from '../../core/services/swap-requests.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

const HISTORY_PAGE_SIZE = 50;

@Component({
  selector: 'app-chat-room',
  imports: [DatePipe, RouterLink, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <a class="icon-btn" topbar-actions routerLink="/messages" aria-label="Back to messages">
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 12H5m7-7-7 7 7 7"/></svg>
      </a>

      @if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (error()) {
        <div class="state-box">
          <p>{{ error() }}</p>
          <button class="btn btn-soft btn-sm" (click)="reload()">Try again</button>
        </div>
      } @else {
        <header class="chat-head">
          <div class="peer">
            <span class="avatar">{{ peerInitials() }}</span>
            <div>
              <b>{{ peerName() }}</b>
              <p class="sub">{{ skillsLine() }}</p>
            </div>
          </div>
          @if (meetingLink()) {
            <a class="btn btn-soft btn-sm" [routerLink]="meetingLink()">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M23 7l-7 5 7 5V7z"/><rect x="1" y="5" width="15" height="14" rx="2"/></svg>
              Meeting
            </a>
          }
        </header>

        @if (hubError()) {
          <p class="hub-error">Live connection issue — {{ hubError() }}. Messages may be delayed.</p>
        }

        <div class="thread" #thread>
          @for (message of messages(); track message.id) {
            <div class="bubble-row" [class.mine]="message.senderId === myId()">
              <div class="bubble">
                <p>{{ message.content }}</p>
                <small>{{ message.sentAtUtc | date: 'MMM d, h:mm a' }}</small>
              </div>
            </div>
          }
          @if (sending()) {
            <div class="bubble-row mine pending"><div class="bubble"><p>{{ draft() }}</p></div></div>
          }
        </div>

        <form class="composer" (submit)="send($event)">
          <input
            class="input"
            placeholder="Type a message…"
            [value]="draft()"
            (input)="draft.set($any($event.target).value)"
            [disabled]="sending()"
            aria-label="Message"
          />
          <button class="send-btn" type="submit" [disabled]="sending() || !draft().trim()" aria-label="Send">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m22 2-7 20-4-9-9-4z"/><path d="M22 2 11 13"/></svg>
          </button>
        </form>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .icon-btn { width: 38px; height: 38px; display: inline-flex; align-items: center; justify-content: center; background: var(--input-bg); color: var(--text); border-radius: 12px; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }

    .chat-head { display: flex; align-items: center; justify-content: space-between; gap: 10px; padding-bottom: 12px; border-bottom: 1px solid var(--border); margin-bottom: 12px; }
    .peer { display: flex; align-items: center; gap: 10px; min-width: 0; }
    .avatar { width: 42px; height: 42px; border-radius: 50%; background: var(--gradient); color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: 14px; font-weight: 700; flex-shrink: 0; }
    .peer b { font-size: 14.5px; }
    .sub { font-size: 11.5px; color: var(--text-muted); margin: 2px 0 0; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .chat-head .btn { display: inline-flex; align-items: center; gap: 5px; flex-shrink: 0; }

    .hub-error { font-size: 12px; color: var(--warning); background: #fff6e5; border-radius: var(--radius-sm); padding: 8px 12px; margin: 0 0 10px; }

    .thread { display: flex; flex-direction: column; gap: 8px; min-height: 40vh; padding-bottom: 8px; }
    .bubble-row { display: flex; }
    .bubble-row.mine { justify-content: flex-end; }
    .bubble { max-width: 78%; background: var(--input-bg); border-radius: 16px 16px 16px 4px; padding: 10px 14px; }
    .bubble p { font-size: 13.5px; line-height: 1.5; margin: 0; white-space: pre-wrap; word-break: break-word; }
    .bubble small { display: block; font-size: 10px; color: var(--text-muted); margin-top: 4px; }
    .bubble-row.mine .bubble { background: var(--primary); border-radius: 16px 16px 4px 16px; }
    .bubble-row.mine .bubble p { color: #fff; }
    .bubble-row.mine .bubble small { color: rgba(255, 255, 255, 0.75); }
    .bubble-row.pending { opacity: 0.6; }

    .composer { display: flex; gap: 8px; align-items: center; position: sticky; bottom: calc(var(--nav-height) + 10px); background: var(--bg); padding: 8px 0; }
    .composer .input { border-radius: 999px; }
    .send-btn { width: 44px; height: 44px; border-radius: 50%; border: none; background: var(--gradient); color: #fff; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; flex-shrink: 0; box-shadow: var(--shadow-btn); }
    .send-btn:disabled { opacity: 0.5; cursor: not-allowed; }
  `,
})
export class ChatRoomComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly swapRequests = inject(SwapRequestsService);
  private readonly conversations = inject(ConversationsService);
  private readonly hub = inject(ChatHubService);

  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly hubError = signal('');
  protected readonly messages = signal<MessageDto[]>([]);
  protected readonly draft = signal('');
  protected readonly sending = signal(false);
  protected readonly peerName = signal('');
  protected readonly skillsLine = signal('');
  protected readonly swapStatus = signal(0);
  private conversationId = '';

  protected readonly Status = SwapRequestStatus;
  private hubSubscription: { unsubscribe(): void } | null = null;

  protected readonly myId = computed(() => this.auth.user()?.userId ?? '');
  protected readonly peerInitials = computed(() =>
    this.peerName()
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0]?.toUpperCase() ?? '')
      .join('')
  );
  protected readonly meetingLink = computed(() => {
    const swapId = this.route.snapshot.paramMap.get('swapId');
    return this.swapStatus() === SwapRequestStatus.Accepted && swapId ? ['/sessions', swapId] : null;
  });

  constructor() {
    effect(() => {
      // Scroll the thread to the newest message whenever the list changes.
      this.messages();
      queueMicrotask(() => {
        const thread = document.querySelector('.thread');
        thread?.scrollTo({ top: thread.scrollHeight });
      });
    });
  }

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  protected async reload(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const swapId = this.route.snapshot.paramMap.get('swapId') ?? '';
      const swap = await firstValueFrom(this.swapRequests.details(swapId));
      if (!swap.conversationId) {
        this.error.set('No conversation exists for this swap yet.');
        this.loading.set(false);
        return;
      }
      this.conversationId = swap.conversationId;
      this.swapStatus.set(swap.status);
      const myId = this.myId();
      this.peerName.set(
        swap.requesterId === myId
          ? `${swap.receiverFirstName} ${swap.receiverLastName}`
          : `${swap.requesterFirstName} ${swap.requesterLastName}`
      );
      this.skillsLine.set(`${swap.offeredSkill.skillName} ↔ ${swap.requestedSkill.skillName}`);

      const history = await firstValueFrom(
        this.conversations.getMessages(this.conversationId, 1, HISTORY_PAGE_SIZE)
      );
      this.messages.set([...history.items].reverse());
      await this.connectHub();
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  private async connectHub(): Promise<void> {
    try {
      await this.hub.connect();
      await this.hub.joinConversation(this.conversationId);
      this.hubError.set('');
      this.hubSubscription ??= this.hub.messages$.subscribe((message) => {
        if (message.conversationId === this.conversationId) {
          this.messages.update((list) => [...list, message]);
        }
      });
    } catch (err) {
      this.hubError.set(err instanceof Error ? err.message : 'could not connect');
    }
  }

  protected async send(event: Event): Promise<void> {
    event.preventDefault();
    const content = this.draft().trim();
    if (!content || this.sending()) return;
    this.sending.set(true);
    try {
      await this.hub.sendMessage(this.conversationId, content);
      this.draft.set('');
    } catch (err) {
      this.hubError.set(extractApiError(err).message);
    } finally {
      this.sending.set(false);
    }
  }

  ngOnDestroy(): void {
    this.hubSubscription?.unsubscribe();
    this.hubSubscription = null;
    void this.hub.leaveConversation(this.conversationId).catch(() => undefined);
    this.hub.disconnect().catch(() => undefined);
  }
}
