import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { ReviewDto, UserBadgeDto } from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { UsersService } from '../../core/services/users.service';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { ProposeSwapDialogComponent } from '../../shared/components/propose-swap-dialog/propose-swap-dialog.component';
import { RatingStarsComponent } from '../../shared/components/rating-stars/rating-stars.component';

const REVIEWS_PAGE_SIZE = 5;

@Component({
  selector: 'app-user-profile',
  imports: [DatePipe, RouterLink, AppShellComponent, LoadingSpinnerComponent, RatingStarsComponent, ProposeSwapDialogComponent],
  template: `
    <app-shell>
      <a class="icon-btn light" topbar-actions routerLink="/home" aria-label="Back to home">
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 12H5m7-7-7 7 7 7"/></svg>
      </a>

      @if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else {
        <section class="hero">
          <div class="hero-top">
            <span class="avatar">{{ initials() }}</span>
            <div class="hero-name">
              <h2>{{ displayName() }}
                <svg width="15" height="15" viewBox="0 0 24 24" fill="#2fbf71" stroke="#fff" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2l2.4 2.4 3.4-.5 1 3.3 3 1.6-1.4 3.2 1.4 3.2-3 1.6-1 3.3-3.4-.5L12 22l-2.4-2.4-3.4.5-1-3.3-3-1.6 1.4-3.2L2.2 8.8l3-1.6 1-3.3 3.4.5z"/><path d="m9 12 2 2 4-4"/></svg>
              </h2>
              @if (title()) { <p class="hero-title">{{ title() }}</p> }
            </div>
          </div>
          <div class="hero-stats">
            <span class="stat">
              <svg class="star" width="13" height="13" viewBox="0 0 24 24" fill="currentColor" stroke="none"><path d="m12 2 3.1 6.3 6.9 1-5 4.9 1.2 6.9-6.2-3.3-6.2 3.3L7 14.2 2 9.3l6.9-1z"/></svg>
              {{ rating().toFixed(1) }} ({{ reviewsCount() }})
            </span>
            @if (location()) {
              <span class="stat">
                <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0z"/><circle cx="12" cy="10" r="3"/></svg>
                {{ location() }}
              </span>
            }
            <span class="stat">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M8 3 4 7l4 4"/><path d="M4 7h16"/><path d="m16 21 4-4-4-4"/><path d="M20 17H4"/></svg>
              Swaps
            </span>
          </div>
        </section>

        <div class="tabs">
          <button class="tab" [class.active]="tab() === 'about'" (click)="tab.set('about')">About Me</button>
          <button class="tab" [class.active]="tab() === 'reviews'" (click)="tab.set('reviews')">Reviews ({{ reviewsCount() }})</button>
        </div>

        @if (tab() === 'about') {
          <section class="card">
            <p class="about-line">
              Passionate about sharing skills and learning from others through meaningful skill exchanges.
            </p>

            <div class="skill-columns">
              <div class="skill-col">
                <span class="col-label">Teaches</span>
                <div class="col-chips">
                  @for (skill of skills(); track skill) {
                    <span class="chip-static purple">{{ skill }}</span>
                  }
                  @if (!skills().length) { <span class="chip-static">—</span> }
                </div>
              </div>
              @if (wanted().length) {
                <span class="swap-sep">
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M8 3 4 7l4 4"/><path d="M4 7h16"/><path d="m16 21 4-4-4-4"/><path d="M20 17H4"/></svg>
                </span>
                <div class="skill-col">
                  <span class="col-label">Wants to learn</span>
                  <div class="col-chips">
                    @for (skill of wanted(); track skill) {
                      <span class="chip-static">{{ skill }}</span>
                    }
                  </div>
                </div>
              }
            </div>
          </section>

          <section class="card">
            <h3>Badges</h3>
            @if (badgesLoading()) {
              <div class="state-box tight"><app-spinner [size]="22" /></div>
            } @else if (!badges().length) {
              <p class="empty-line">No badges yet.</p>
            } @else {
              <div class="badge-grid">
                @for (badge of badges(); track badge.badgeId) {
                  <div class="badge-item" [title]="badge.description">
                    <span class="badge-icon">
                      @if (badge.iconUrl) {
                        <img [src]="badge.iconUrl" alt="" />
                      } @else {
                        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="9" r="6"/><path d="m8.5 14-1.5 7 5-3 5 3-1.5-7"/></svg>
                      }
                    </span>
                    <b>{{ badge.name }}</b>
                    <small>×{{ badge.awardCount }}</small>
                  </div>
                }
              </div>
            }
          </section>
        } @else {
          <section class="card">
            @if (reviewsLoading() && !reviews().length) {
              <div class="state-box tight"><app-spinner [size]="22" /></div>
            } @else if (!reviews().length) {
              <p class="empty-line">No reviews yet.</p>
            } @else {
              <div class="review-list">
                @for (review of reviews(); track review.id) {
                  <article class="review">
                    <div class="review-head">
                      <b>{{ review.reviewerFirstName }} {{ review.reviewerLastName }}</b>
                      <app-rating-stars [rating]="review.rating" />
                    </div>
                    @if (review.comment) { <p class="review-comment">{{ review.comment }}</p> }
                    <small class="review-date">{{ review.createdAtUtc | date: 'MMM d, yyyy' }}</small>
                  </article>
                }
              </div>
              @if (hasMoreReviews()) {
                <button class="btn btn-outline btn-sm more-btn" [disabled]="reviewsLoading()" (click)="loadMoreReviews()">
                  @if (reviewsLoading()) { <app-spinner [size]="16" /> } @else { Load more }
                </button>
              }
            }
          </section>
        }

        <button class="btn btn-primary btn-cta" (click)="proposeSwap()">Propose Swap</button>
      }

      @if (proposeOpen()) {
        <app-propose-swap-dialog
          [receiverId]="userId()"
          [receiverName]="displayName()"
          [theirSkillNames]="skills()"
          (closed)="onProposeClosed($event)"
        />
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .icon-btn { width: 38px; height: 38px; display: inline-flex; align-items: center; justify-content: center; background: var(--input-bg); color: var(--text); border-radius: 12px; }
    .icon-btn.light { background: rgba(255, 255, 255, 0.25); color: #fff; margin-bottom: -46px; position: relative; z-index: 2; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }
    .state-box.tight { padding: 16px; }
    .card { padding: 20px; margin-bottom: 16px; }
    .card h3 { font-size: 15.5px; font-weight: 700; margin: 0 0 8px; }

    .hero {
      background: linear-gradient(160deg, #8f7bf5 0%, #7c5cfc 55%, #6a4df0 100%);
      border-radius: 0 0 28px 28px; margin: -8px -20px 18px; padding: 24px 20px 18px; color: #fff;
    }
    .hero-top { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 10px; }
    .avatar {
      width: 92px; height: 92px; border-radius: 50%; background: #fff; color: var(--primary);
      display: inline-flex; align-items: center; justify-content: center; font-size: 30px; font-weight: 700;
      border: 4px solid rgba(255, 255, 255, 0.4);
    }
    .hero-name h2 { display: flex; align-items: center; gap: 6px; font-size: 20px; font-weight: 700; margin: 0; justify-content: center; }
    .hero-title { font-size: 13px; opacity: 0.9; margin: 2px 0 0; }
    .hero-stats { display: flex; justify-content: center; gap: 14px; margin-top: 16px; flex-wrap: wrap; }
    .stat { display: inline-flex; align-items: center; gap: 5px; font-size: 12px; font-weight: 600; background: rgba(255, 255, 255, 0.18); padding: 6px 12px; border-radius: 999px; }
    .stat .star { color: #ffd76e; }

    .tabs { display: flex; gap: 8px; margin-bottom: 14px; }
    .tab {
      flex: 1; background: var(--input-bg); border: none; border-radius: var(--radius-sm);
      padding: 11px; font-size: 13px; font-weight: 600; color: var(--text-secondary); cursor: pointer;
    }
    .tab.active { background: var(--primary-light); color: var(--primary); }

    .about-line { font-size: 13px; color: var(--text-secondary); line-height: 1.6; margin: 0 0 14px; }
    .skill-columns { display: flex; align-items: stretch; gap: 10px; }
    .skill-col { flex: 1; min-width: 0; }
    .col-label { display: block; font-size: 10px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px; }
    .col-chips { display: flex; flex-wrap: wrap; gap: 5px; }
    .col-chips .chip-static { font-size: 10.5px; padding: 4px 9px; }
    .swap-sep { display: flex; align-items: center; color: var(--primary); flex-shrink: 0; }

    .btn-cta { width: 100%; padding: 13px; font-size: 14.5px; margin-bottom: 10px; }

    .badge-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 12px; margin-top: 10px; }
    .badge-item { display: flex; flex-direction: column; align-items: center; text-align: center; gap: 4px; font-size: 11.5px; }
    .badge-item b { font-weight: 600; }
    .badge-item small { color: var(--text-muted); }
    .badge-icon { width: 46px; height: 46px; border-radius: 50%; background: var(--primary-light); color: var(--primary); display: flex; align-items: center; justify-content: center; }
    .badge-icon img { width: 26px; height: 26px; object-fit: contain; }

    .review-list { display: flex; flex-direction: column; gap: 14px; }
    .review { border-bottom: 1px solid var(--border); padding-bottom: 12px; }
    .review:last-child { border-bottom: none; padding-bottom: 0; }
    .review-head { display: flex; align-items: center; justify-content: space-between; gap: 8px; font-size: 13.5px; }
    .review-comment { font-size: 13px; color: var(--text-secondary); line-height: 1.55; margin: 6px 0; }
    .review-date { font-size: 11px; color: var(--text-muted); }
    .empty-line { font-size: 12.5px; color: var(--text-muted); margin: 4px 0; }
    .more-btn { margin: 14px auto 0; display: flex; }
  `,
})
export class UserProfileComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly usersService = inject(UsersService);

  protected readonly loading = signal(true);
  protected readonly userId = signal('');
  protected readonly displayName = signal('');
  protected readonly title = signal('');
  protected readonly location = signal('');
  protected readonly rating = signal(0);
  protected readonly reviewsCount = signal(0);
  protected readonly skills = signal<string[]>([]);
  protected readonly wanted = signal<string[]>([]);
  protected readonly tab = signal<'about' | 'reviews'>('about');

  protected readonly badges = signal<UserBadgeDto[]>([]);
  protected readonly badgesLoading = signal(false);
  protected readonly reviews = signal<ReviewDto[]>([]);
  protected readonly reviewsLoading = signal(false);
  private reviewsPage = 0;
  protected readonly hasMoreReviews = signal(false);

  protected readonly proposeOpen = signal(false);

  protected readonly initials = computed(() =>
    this.displayName()
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0]?.toUpperCase() ?? '')
      .join('')
  );

  async ngOnInit(): Promise<void> {
    const params = this.route.snapshot.paramMap;
    const query = this.route.snapshot.queryParamMap;
    this.userId.set(params.get('id') ?? '');
    this.displayName.set(query.get('name') ?? 'SkillSwap member');
    this.title.set(query.get('title') ?? '');
    this.location.set([query.get('city'), query.get('country')].filter(Boolean).join(', '));
    this.rating.set(Number(query.get('rating') ?? 0) || 0);
    this.reviewsCount.set(Number(query.get('reviews') ?? 0) || 0);
    this.skills.set((query.get('skills') ?? '').split('|').map((s) => s.trim()).filter(Boolean));
    this.wanted.set((query.get('wanted') ?? '').split('|').map((s) => s.trim()).filter(Boolean));
    this.loading.set(false);

    const id = this.userId();
    if (id) {
      void this.loadBadges(id);
      void this.loadReviews(id, true);
    }
  }

  protected proposeSwap(): void {
    if (!this.auth.isAuthenticated()) {
      void this.router.navigate(['/auth/sign-in'], {
        queryParams: { redirect: `/users/${this.userId()}` },
      });
      return;
    }
    this.proposeOpen.set(true);
  }

  protected onProposeClosed(proposed: boolean): void {
    this.proposeOpen.set(false);
    if (proposed) void this.router.navigateByUrl('/swaps');
  }

  private async loadBadges(userId: string): Promise<void> {
    this.badgesLoading.set(true);
    try {
      this.badges.set(await firstValueFrom(this.usersService.getUserBadges(userId)));
    } catch {
      // Badges are a bonus — don't block the page.
    } finally {
      this.badgesLoading.set(false);
    }
  }

  private async loadReviews(userId: string, reset: boolean): Promise<void> {
    if (this.reviewsLoading()) return;
    this.reviewsLoading.set(true);
    const page = reset ? 1 : this.reviewsPage + 1;
    try {
      const result = await firstValueFrom(
        this.usersService.getUserReviews(userId, page, REVIEWS_PAGE_SIZE)
      );
      this.reviews.update((list) => (reset ? result.items : [...list, ...result.items]));
      this.reviewsPage = result.pageNumber;
      this.hasMoreReviews.set(result.pageNumber < result.totalPages);
      this.reviewsCount.set(result.totalCount);
    } catch {
      // Reviews failing shouldn't block the profile.
    } finally {
      this.reviewsLoading.set(false);
    }
  }

  protected loadMoreReviews(): void {
    const id = this.userId();
    if (id) void this.loadReviews(id, false);
  }
}
