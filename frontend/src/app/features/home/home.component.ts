import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult, PublicListingDto, SkillCatalogItemDto } from '../../core/models/api.models';
import { AuthService } from '../../core/services/auth.service';
import { ToastService } from '../../core/services/toast.service';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { ProposeSwapDialogComponent } from '../../shared/components/propose-swap-dialog/propose-swap-dialog.component';

const PAGE_SIZE = 10;

@Component({
  selector: 'app-home',
  imports: [RouterLink, AppShellComponent, LoadingSpinnerComponent, ProposeSwapDialogComponent],
  template: `
    <app-shell>
      <a class="icon-btn" topbar-actions routerLink="/explore" aria-label="Search skills">
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"/><path d="m21 21-4.3-4.3"/></svg>
      </a>
      @if (isAuthenticated()) {
        <a class="icon-btn" topbar-actions routerLink="/swaps" aria-label="My swaps">
          <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M8 3 4 7l4 4"/><path d="M4 7h16"/><path d="m16 21 4-4-4-4"/><path d="M20 17H4"/></svg>
        </a>
      }

      <div class="greeting">
        <div>
          <h2 class="hello">Hi, {{ firstName() }}</h2>
          <p class="hello-sub">Learn something new, share what you know.</p>
        </div>
      </div>

      @if (isAuthenticated()) {
        <div class="match-banner">
          <p class="match-title">We found people who match your skills</p>
          <p class="match-sub">Based on what you teach and want to learn</p>
          <a class="match-btn" routerLink="/explore">View Match</a>
        </div>
      }

      <div class="chips-row">
        <button class="chip" [class.active]="!selectedSkill()" (click)="selectSkill(null)">All</button>
        @for (skill of skills(); track skill.id) {
          <button class="chip" [class.active]="selectedSkill() === skill.name" (click)="selectSkill(skill.name)">{{ skill.name }}</button>
        }
      </div>

      @if (error() && !items().length) {
        <div class="state-box">
          <p>{{ error() }}</p>
          <button class="btn btn-soft btn-sm" (click)="retry()">Try again</button>
        </div>
      } @else if (loading() && !items().length) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (!items().length) {
        <div class="state-box">
          <img src="/assets/illustrations/onboarding-matching.png" alt="" class="state-art" />
          <p>No swaps found{{ selectedSkill() ? ' for “' + selectedSkill() + '”' : ' yet' }}.</p>
        </div>
      }

      <div class="feed">
        @for (listing of items(); track listing.id) {
          <article class="swap-card card">
            <button class="swap-head as-link" (click)="openProfile(listing)">
              <span class="avatar">{{ initials(listing.displayName) }}</span>
              <div class="who">
                <h3>{{ listing.displayName }}</h3>
                <p class="meta">
                  <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0z"/><circle cx="12" cy="10" r="3"/></svg>
                  {{ locationLabel(listing) }}
                  <span class="dot">·</span>
                  <svg class="star" width="12" height="12" viewBox="0 0 24 24" fill="currentColor" stroke="none"><path d="m12 2 3.1 6.3 6.9 1-5 4.9 1.2 6.9-6.2-3.3-6.2 3.3L7 14.2 2 9.3l6.9-1z"/></svg>
                  {{ listing.averageRating.toFixed(1) }} ({{ listing.totalReviewsCount }})
                </p>
              </div>
            </button>

            <div class="skill-columns">
              <div class="skill-col">
                <span class="col-label">Teaches</span>
                <div class="col-chips">
                  @for (s of listing.skillsOffered; track s) {
                    <span class="chip-static purple">{{ s }}</span>
                  }
                </div>
              </div>
              <span class="swap-sep">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M8 3 4 7l4 4"/><path d="M4 7h16"/><path d="m16 21 4-4-4-4"/><path d="M20 17H4"/></svg>
              </span>
              <div class="skill-col">
                <span class="col-label">Wants to learn</span>
                <div class="col-chips">
                  @if (listing.skillsWanted.length) {
                    @for (s of listing.skillsWanted; track s) {
                      <span class="chip-static">{{ s }}</span>
                    }
                  } @else {
                    <span class="chip-static">Open to offers</span>
                  }
                </div>
              </div>
            </div>

            <div class="card-actions">
              <button class="btn btn-primary btn-propose" (click)="proposeSwap(listing)">Propose Swap</button>
              <button class="chat-btn" (click)="goToMessages()" aria-label="Messages">
                <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
              </button>
            </div>
          </article>
        }
      </div>

      @if (proposeFor(); as target) {
        <app-propose-swap-dialog
          [receiverId]="target.id"
          [receiverName]="target.displayName"
          [theirSkillNames]="target.skillsOffered"
          (closed)="onProposeClosed($event)"
        />
      }

      @if (items().length) {
        <div class="more">
          @if (hasMore()) {
            <button class="btn btn-outline btn-sm" [disabled]="loading()" (click)="loadMore()">
              @if (loading()) { <app-spinner [size]="16" /> } @else { Load more }
            </button>
          } @else {
            <span class="end-note">You've seen all swaps</span>
          }
        </div>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .icon-btn { width: 38px; height: 38px; display: inline-flex; align-items: center; justify-content: center; background: var(--input-bg); color: var(--text); border-radius: 12px; }

    .greeting { margin: 4px 0 12px; }
    .hello { font-size: 21px; font-weight: 700; margin: 0; }
    .hello-sub { font-size: 12.5px; color: var(--text-secondary); margin: 2px 0 0; }

    .match-banner {
      background: var(--gradient); border-radius: var(--radius-lg);
      padding: 18px; margin-bottom: 16px; color: #fff;
      box-shadow: var(--shadow-btn);
    }
    .match-title { font-size: 14.5px; font-weight: 700; margin: 0; }
    .match-sub { font-size: 11.5px; opacity: 0.85; margin: 3px 0 12px; }
    .match-btn {
      display: inline-flex; background: #fff; color: var(--primary);
      font-size: 12.5px; font-weight: 700; padding: 9px 18px; border-radius: 999px;
    }

    .chips-row { display: flex; gap: 8px; overflow-x: auto; padding: 2px 2px 14px; margin: 0 -2px; scrollbar-width: none; }
    .chips-row::-webkit-scrollbar { display: none; }

    .feed { display: flex; flex-direction: column; gap: 16px; }
    .swap-card { padding: 18px; }
    .swap-head { display: flex; gap: 12px; align-items: flex-start; }
    .swap-head.as-link { width: 100%; background: none; border: none; padding: 0; text-align: left; cursor: pointer; font-family: inherit; }
    .avatar { width: 46px; height: 46px; border-radius: 50%; background: var(--gradient); color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: 15px; font-weight: 700; flex-shrink: 0; }
    .who { flex: 1; min-width: 0; }
    .who h3 { font-size: 15px; font-weight: 600; margin: 0; }
    .who .meta { display: flex; align-items: center; gap: 4px; font-size: 11.5px; color: var(--text-muted); margin: 3px 0 0; flex-wrap: wrap; }
    .who .meta .star { color: var(--star); }
    .who .meta .dot { margin: 0 2px; }

    .skill-columns { display: flex; align-items: stretch; gap: 10px; margin: 14px 0; }
    .skill-col { flex: 1; min-width: 0; }
    .col-label { display: block; font-size: 10px; font-weight: 700; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 6px; }
    .col-chips { display: flex; flex-wrap: wrap; gap: 5px; }
    .col-chips .chip-static { font-size: 10.5px; padding: 4px 9px; }
    .swap-sep { display: flex; align-items: center; color: var(--primary); flex-shrink: 0; }

    .card-actions { display: flex; gap: 10px; }
    .btn-propose { flex: 1; padding: 12px; font-size: 14px; }
    .chat-btn {
      width: 46px; border-radius: var(--radius-md); border: 1.5px solid var(--border);
      background: #fff; color: var(--primary); display: inline-flex; align-items: center; justify-content: center;
      cursor: pointer;
    }

    .more { display: flex; justify-content: center; margin-top: 20px; }
    .end-note { font-size: 12.5px; color: var(--text-muted); }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }
    .state-art { width: 150px; height: 150px; object-fit: cover; border-radius: var(--radius-lg); }
  `,
})
export class HomeComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  items = signal<PublicListingDto[]>([]);
  skills = signal<{ id: string; name: string }[]>([]);
  selectedSkill = signal<string | null>(null);
  pageNumber = signal(1);
  totalPages = signal(0);
  loading = signal(false);
  error = signal('');
  proposeFor = signal<PublicListingDto | null>(null);

  protected readonly isAuthenticated = this.auth.isAuthenticated;
  protected readonly firstName = computed(() => this.auth.user()?.firstName ?? 'there');

  hasMore = computed(() => this.pageNumber() < this.totalPages());

  async ngOnInit(): Promise<void> {
    const skillParam = this.route.snapshot.queryParamMap.get('skill');
    if (skillParam) this.selectedSkill.set(skillParam);
    this.loadSkills();
    await this.loadFeed(true);
  }

  async loadSkills(): Promise<void> {
    try {
      const catalog = await firstValueFrom(
        this.http.get<SkillCatalogItemDto[]>(`${environment.apiUrl}/api/v1/skills`)
      );
      const seen = new Set<string>();
      const flat = catalog.flatMap((c) => c.skills).filter((s) => {
        if (seen.has(s.id)) return false;
        seen.add(s.id);
        return true;
      });
      this.skills.set(flat);
    } catch {
      // Chips are a filter nicety — the feed still works without them.
    }
  }

  selectSkill(skill: string | null): void {
    if (this.selectedSkill() === skill) return;
    this.selectedSkill.set(skill);
    void this.loadFeed(true);
  }

  retry(): void {
    void this.loadFeed(true);
  }

  loadMore(): void {
    void this.loadFeed(false);
  }

  proposeSwap(listing: PublicListingDto): void {
    if (!this.auth.isAuthenticated()) {
      this.toast.info('Sign in to propose a swap.');
      void this.router.navigate(['/auth/sign-in'], { queryParams: { redirect: '/home' } });
      return;
    }
    this.proposeFor.set(listing);
  }

  goToMessages(): void {
    void this.router.navigateByUrl(this.isAuthenticated() ? '/messages' : '/auth/sign-in');
  }

  onProposeClosed(proposed: boolean): void {
    this.proposeFor.set(null);
    if (proposed) void this.router.navigateByUrl('/swaps');
  }

  openProfile(listing: PublicListingDto): void {
    void this.router.navigate(['/users', listing.id], {
      queryParams: {
        name: listing.displayName,
        title: listing.title,
        skills: listing.skillsOffered.join('|'),
        wanted: listing.skillsWanted.join('|'),
        rating: listing.averageRating,
        reviews: listing.totalReviewsCount,
        city: listing.city,
        country: listing.country,
      },
    });
  }

  initials(name: string): string {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((p) => p[0]?.toUpperCase() ?? '')
      .join('');
  }

  locationLabel(listing: PublicListingDto): string {
    return [listing.city, listing.country].filter(Boolean).join(', ') || 'Anywhere';
  }

  private async loadFeed(reset: boolean): Promise<void> {
    if (this.loading()) return;
    this.loading.set(true);
    this.error.set('');
    const page = reset ? 1 : this.pageNumber() + 1;
    try {
      const params: Record<string, string | number> = { pageNumber: page, pageSize: PAGE_SIZE };
      const skill = this.selectedSkill();
      if (skill) params['skillFilter'] = skill;
      const result = await firstValueFrom(
        this.http.get<PagedResult<PublicListingDto>>(`${environment.apiUrl}/api/GuestFeed`, { params })
      );
      this.items.update((list) => (reset ? result.items : [...list, ...result.items]));
      this.pageNumber.set(result.pageNumber);
      this.totalPages.set(result.totalPages);
    } catch (err) {
      this.error.set(err instanceof Error ? err.message : 'Could not load swaps.');
    } finally {
      this.loading.set(false);
    }
  }
}
