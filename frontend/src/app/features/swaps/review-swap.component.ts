import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SwapRequestDetailsDto } from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { SwapRequestsService } from '../../core/services/swap-requests.service';
import { ToastService } from '../../core/services/toast.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-review-swap',
  imports: [FormsModule, RouterLink, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <a class="icon-btn" topbar-actions [routerLink]="['/swaps', swapId()]" aria-label="Back to swap">
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
        <h2 class="page-title">Final Session <span>Review</span></h2>

        <section class="card">
          <div class="session-row">
            <span class="avatar">{{ initials(partnerName()) }}</span>
            <div>
              <b>Session with {{ partnerName() }}</b>
              <small>{{ swapTitle() }}</small>
            </div>
          </div>

          <div class="stars-row">
            @for (star of [1, 2, 3, 4, 5]; track star) {
              <button class="star" type="button" (click)="rating.set(star)" [attr.aria-label]="star + ' star' + (star > 1 ? 's' : '')">
                <svg width="30" height="30" viewBox="0 0 24 24" [attr.fill]="star <= rating() ? 'var(--star)' : 'none'" [attr.stroke]="star <= rating() ? 'var(--star)' : 'var(--text-muted)'" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round"><path d="m12 2 3.1 6.3 6.9 1-5 4.9 1.2 6.9-6.2-3.3-6.2 3.3L7 14.2 2 9.3l6.9-1z"/></svg>
              </button>
            }
          </div>
          <p class="rating-note">Rate your experience. {{ rating() }}/5</p>
        </section>

        <section class="card">
          <h3 class="card-title">Gift a Badge</h3>
          <p class="card-sub">Recognize what made this exchange special (optional).</p>
          <div class="badge-grid">
            @for (badge of badgeOptions; track badge.name) {
              <button class="badge-tile" [class.selected]="selectedBadge() === badge.name" (click)="toggleBadge(badge.name)">
                <span class="badge-emoji">{{ badge.emoji }}</span>
                <small>{{ badge.name }}</small>
              </button>
            }
          </div>
        </section>

        <section class="card">
          <h3 class="card-title">Your Feedback</h3>
          <textarea
            class="input feedback"
            rows="4"
            maxlength="500"
            placeholder="Write a short feedback…"
            [(ngModel)]="comment"
          ></textarea>
        </section>

        @if (error2()) { <p class="field-error">{{ error2() }}</p> }
        <div class="actions">
          <button class="btn btn-soft" [routerLink]="['/swaps', swapId()]">Cancel</button>
          <button class="btn btn-primary" [disabled]="submitting() || rating() === 0" (click)="submit()">
            @if (submitting()) { <app-spinner [size]="16" /> } @else { Submit Review }
          </button>
        </div>
      }

      @if (submitted()) {
        <div class="overlay">
          <div class="modal">
            <span class="modal-icon">
              <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="var(--warning)" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><path d="m12 2 3.1 6.3 6.9 1-5 4.9 1.2 6.9-6.2-3.3-6.2 3.3L7 14.2 2 9.3l6.9-1z"/></svg>
            </span>
            <h3>Feedback Sent!</h3>
            <p class="modal-sub">Thank you for supporting the community learning. Your rating and badge have been delivered to your tutor.</p>
            <button class="btn btn-primary" (click)="goHome()">Back to Home / Wallet</button>
            <button class="btn btn-soft" (click)="goSwap()">Back to Session</button>
          </div>
        </div>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .icon-btn { width: 38px; height: 38px; display: inline-flex; align-items: center; justify-content: center; background: var(--input-bg); color: var(--text); border-radius: 12px; }
    .page-title { font-size: 21px; font-weight: 700; margin: 4px 0 14px; }
    .page-title span { background: var(--gradient); -webkit-background-clip: text; background-clip: text; color: transparent; }

    .card { padding: 20px; margin-bottom: 16px; }
    .card-title { font-size: 14.5px; font-weight: 700; margin: 0 0 4px; }
    .card-sub { font-size: 12px; color: var(--text-secondary); margin: 0 0 12px; }

    .session-row { display: flex; align-items: center; gap: 12px; margin-bottom: 16px; }
    .avatar { width: 44px; height: 44px; border-radius: 50%; background: var(--gradient); color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: 15px; font-weight: 700; flex-shrink: 0; }
    .session-row b { display: block; font-size: 14px; }
    .session-row small { font-size: 11.5px; color: var(--text-muted); }

    .stars-row { display: flex; justify-content: center; gap: 8px; padding: 4px 0; }
    .star { background: none; border: none; cursor: pointer; padding: 2px; }
    .rating-note { text-align: center; font-size: 12px; color: var(--text-secondary); margin: 8px 0 0; }

    .badge-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
    .badge-tile {
      display: flex; flex-direction: column; align-items: center; gap: 6px;
      border: 1.5px solid transparent; border-radius: var(--radius-md); padding: 16px 10px;
      color: #fff; cursor: pointer; font-family: inherit;
    }
    .badge-tile:nth-child(1) { background: #4d9ffb; }
    .badge-tile:nth-child(2) { background: #ffa53c; }
    .badge-tile:nth-child(3) { background: #4fd1c5; }
    .badge-tile:nth-child(4) { background: #ff7a6e; }
    .badge-tile.selected { border-color: var(--text); box-shadow: 0 0 0 3px rgba(31, 31, 61, 0.15); }
    .badge-emoji { font-size: 24px; }
    .badge-tile small { font-size: 11.5px; font-weight: 600; }

    textarea.feedback { resize: vertical; font-family: inherit; }

    .actions { display: flex; gap: 10px; margin-bottom: 14px; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }

    .overlay { position: fixed; inset: 0; z-index: 100; background: rgba(31, 31, 61, 0.45); display: flex; align-items: center; justify-content: center; padding: 24px; }
    .modal {
      width: 100%; max-width: 380px; background: #fff; border-radius: 24px; padding: 28px 22px;
      display: flex; flex-direction: column; gap: 10px; text-align: center; animation: pop 0.22s ease both;
    }
    @keyframes pop { from { transform: scale(0.95); opacity: 0; } to { transform: none; opacity: 1; } }
    .modal-icon { width: 64px; height: 64px; border-radius: 50%; background: #fff6e5; display: flex; align-items: center; justify-content: center; margin: 0 auto; }
    .modal h3 { font-size: 16px; font-weight: 700; margin: 0; }
    .modal-sub { font-size: 12.5px; color: var(--text-secondary); line-height: 1.6; margin: 0; }
  `,
})
export class ReviewSwapComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly swapRequests = inject(SwapRequestsService);
  private readonly toast = inject(ToastService);

  // Gift-a-badge is a visual selection only — the backend has no badge-gifting endpoint yet.
  protected readonly badgeOptions = [
    { name: 'Best Tutor', emoji: '🎓' },
    { name: 'Great Energy', emoji: '⏰' },
    { name: 'Problem Solver', emoji: '💡' },
    { name: 'Super Patient', emoji: '🌟' },
  ];

  protected readonly swapId = signal('');
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly error2 = signal('');
  protected readonly submitting = signal(false);
  protected readonly submitted = signal(false);
  protected readonly rating = signal(0);
  protected readonly selectedBadge = signal('');
  protected comment = '';

  private swap: SwapRequestDetailsDto | null = null;

  async ngOnInit(): Promise<void> {
    this.swapId.set(this.route.snapshot.paramMap.get('id') ?? '');
    await this.reload();
  }

  protected partnerName(): string {
    const swap = this.swap;
    if (!swap) return 'your partner';
    const me = this.auth.user()?.userId;
    return swap.requesterId === me
      ? `${swap.receiverFirstName} ${swap.receiverLastName}`.trim()
      : `${swap.requesterFirstName} ${swap.requesterLastName}`.trim();
  }

  protected partnerId(): string {
    const swap = this.swap;
    if (!swap) return '';
    const me = this.auth.user()?.userId;
    return swap.requesterId === me ? swap.receiverId : swap.requesterId;
  }

  protected swapTitle(): string {
    return this.swap?.offeredSkill?.skillName || 'Skill exchange';
  }

  protected toggleBadge(name: string): void {
    this.selectedBadge.set(this.selectedBadge() === name ? '' : name);
  }

  protected initials(name: string): string {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0]?.toUpperCase() ?? '')
      .join('');
  }

  protected async submit(): Promise<void> {
    if (!this.swap || this.submitting()) return;
    this.submitting.set(true);
    this.error2.set('');
    try {
      await firstValueFrom(
        this.http.post(`${environment.apiUrl}/api/v1/reviews`, {
          swapRequestId: this.swapId(),
          revieweeId: this.partnerId(),
          rating: this.rating(),
          comment: this.comment.trim() || null,
        })
      );
      this.toast.success('Review submitted. Thank you!');
      this.submitted.set(true);
    } catch (err) {
      this.error2.set(extractApiError(err).message);
    } finally {
      this.submitting.set(false);
    }
  }

  protected goHome(): void {
    void this.router.navigateByUrl('/home');
  }

  protected goSwap(): void {
    void this.router.navigateByUrl(`/swaps/${this.swapId()}`);
  }

  protected async reload(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.swap = await firstValueFrom(this.swapRequests.details(this.swapId()));
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }
}
