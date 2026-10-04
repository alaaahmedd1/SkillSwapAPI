import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';
import { SkillCatalogItemDto } from '../../core/models/api.models';
import {
  ProficiencyLevel,
  ProfileDto,
  ReviewDto,
  SkillType,
  UserBadgeDto,
  UserSkillDto,
  WalletBalanceDto,
} from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';
import { UsersService } from '../../core/services/users.service';
import { WalletService } from '../../core/services/wallet.service';
import { ToastService } from '../../core/services/toast.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';
import { RatingStarsComponent } from '../../shared/components/rating-stars/rating-stars.component';

const PROFICIENCY_LABELS: Record<number, string> = {
  [ProficiencyLevel.Beginner]: 'Beginner',
  [ProficiencyLevel.Intermediate]: 'Intermediate',
  [ProficiencyLevel.Expert]: 'Expert',
};

const REVIEWS_PAGE_SIZE = 5;

@Component({
  selector: 'app-profile',
  imports: [DatePipe, RouterLink, AppShellComponent, LoadingSpinnerComponent, RatingStarsComponent],
  template: `
    <app-shell>
      @if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (loadError()) {
        <div class="state-box">
          <p>{{ loadError() }}</p>
          <button class="btn btn-soft btn-sm" (click)="reload()">Try again</button>
        </div>
      } @else if (profile(); as p) {
        <div class="profile-layout">
          <div class="profile-col">
            <section class="card header-card">
              <div class="head-row">
                <span class="avatar">{{ initials(p.firstName, p.lastName) }}</span>
                <div class="head-main">
                  <div class="name-row">
                    <h2>{{ p.firstName }} {{ p.lastName }}</h2>
                    <svg width="15" height="15" viewBox="0 0 24 24" fill="#2fbf71" stroke="#fff" stroke-width="1.4" stroke-linecap="round" stroke-linejoin="round"><path d="M12 2l2.4 2.4 3.4-.5 1 3.3 3 1.6-1.4 3.2 1.4 3.2-3 1.6-1 3.3-3.4-.5L12 22l-2.4-2.4-3.4.5-1-3.3-3-1.6 1.4-3.2L2.2 8.8l3-1.6 1-3.3 3.4.5z"/><path d="m9 12 2 2 4-4"/></svg>
                  </div>
                  @if (p.title) { <span class="title-pill">{{ p.title }}</span> }
                  <p class="loc">
                    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 10c0 6-8 12-8 12s-8-6-8-12a8 8 0 0 1 16 0z"/><circle cx="12" cy="10" r="3"/></svg>
                    {{ locationLabel(p) }}
                  </p>
                  <app-rating-stars [rating]="p.averageRating" [count]="p.totalReviewsCount" />
                </div>
              </div>
              <a class="btn btn-primary btn-edit" routerLink="/profile/edit">Edit Profile &amp; Availability</a>
            </section>

            @if (balance(); as b) {
            <section class="card ledger-card">
              <div class="ledger-head">
                <span class="ledger-icon">
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/></svg>
                </span>
                <b class="ledger-value">{{ hoursLabel(b.balanceMinutes) }}</b>
                <span class="ledger-label">Hours Earned</span>
                <a class="ledger-link" routerLink="/wallet">Ledger →</a>
              </div>
              <p class="ledger-sub">Spend hours to learn — earn them back by teaching.</p>
            </section>
            }
          </div>

          <div class="profile-col">
            <section class="card">
              <div class="section-head">
                <h3>Skill Inventory</h3>
                <button class="btn btn-soft btn-sm" (click)="toggleAddSkill()">
                  {{ addingSkill() ? 'Close' : '+ Add' }}
                </button>
              </div>

              <div class="tabs">
                <button class="tab" [class.active]="skillTab() === 'teach'" (click)="skillTab.set('teach')">
                  Can Teach <span class="tab-count">{{ offeredSkills().length }}</span>
                </button>
                <button class="tab" [class.active]="skillTab() === 'learn'" (click)="skillTab.set('learn')">
                  Wants to Learn <span class="tab-count">{{ seekingSkills().length }}</span>
                </button>
              </div>

              @if (addingSkill()) {
                <div class="add-skill">
                  <div class="field">
                    <label for="skill-pick">Skill</label>
                    <select id="skill-pick" class="input" [value]="newSkillId()" (change)="newSkillId.set($any($event.target).value)">
                      <option value="" disabled>Select a skill</option>
                      @for (skill of addableSkills(); track skill.id) {
                        <option [value]="skill.id">{{ skill.name }}</option>
                      }
                    </select>
                  </div>
                  <div class="grid-2">
                    <div class="field">
                      <label for="skill-type">I want to</label>
                      <select id="skill-type" class="input" [value]="newType()" (change)="newType.set(+$any($event.target).value)">
                        <option [value]="SkillType.Offered">Teach (Offered)</option>
                        <option [value]="SkillType.Seeking">Learn (Seeking)</option>
                      </select>
                    </div>
                    <div class="field">
                      <label for="skill-level">Level</label>
                      <select id="skill-level" class="input" [value]="newProficiency()" (change)="newProficiency.set(+$any($event.target).value)">
                        <option [value]="ProficiencyLevel.Beginner">Beginner</option>
                        <option [value]="ProficiencyLevel.Intermediate">Intermediate</option>
                        <option [value]="ProficiencyLevel.Expert">Expert</option>
                      </select>
                    </div>
                  </div>
                  <div class="field">
                    <label for="skill-years">Years of experience <span class="opt">(optional)</span></label>
                    <input id="skill-years" type="number" min="0" max="60" class="input" placeholder="e.g. 3" [value]="newYears()" (input)="newYears.set($any($event.target).value)" />
                  </div>
                  @if (skillError()) { <p class="field-error">{{ skillError() }}</p> }
                  <button class="btn btn-primary btn-sm" [disabled]="savingSkill() || !newSkillId()" (click)="addSkill()">
                    @if (savingSkill()) { <app-spinner [size]="16" /> } @else { Add Skill }
                  </button>
                </div>
              }

              @if (activeSkills().length) {
                <div class="skill-list">
                  @for (skill of activeSkills(); track skill.id) {
                    <div class="skill-row">
                      <div class="skill-main">
                        <b>{{ skill.skillName }}</b>
                        <span class="skill-meta">
                          <span class="lvl lvl-{{ skill.proficiencyLevel }}">{{ levelLabel(skill.proficiencyLevel) }}</span>
                          @if (skill.yearsOfExperience !== null) { <small>{{ skill.yearsOfExperience }}y exp</small> }
                        </span>
                      </div>
                      <button class="remove" (click)="removeSkill(skill)" [disabled]="removingSkillId() === skill.id" aria-label="Remove {{ skill.skillName }}">
                        <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><path d="M18 6 6 18M6 6l12 12"/></svg>
                      </button>
                    </div>
                  }
                </div>
              } @else {
                <p class="empty-line">
                  {{ skillTab() === 'teach' ? 'No teaching skills yet — add what you can teach.' : 'No learning goals yet — add what you want to learn.' }}
                </p>
              }
            </section>

            <section class="card">
              <div class="section-head">
                <h3>Recognized Mastery &amp; Badges</h3>
              </div>
              @if (badgesLoading()) {
                <div class="state-box tight"><app-spinner [size]="22" /></div>
              } @else if (!badges().length) {
                <p class="empty-line">No badges yet — complete swaps to earn them.</p>
              } @else {
                <div class="badge-chips">
                  @for (badge of badges(); track badge.badgeId) {
                    <span class="badge-chip" [title]="badge.description">
                      @if (badge.iconUrl) {
                        <img [src]="badge.iconUrl" alt="" />
                      } @else {
                        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="9" r="6"/><path d="m8.5 14-1.5 7 5-3 5 3-1.5-7"/></svg>
                      }
                      {{ badge.name }}
                    </span>
                  }
                </div>
              }
            </section>

            <section class="card">
              <div class="section-head">
                <h3>Peer Reviews</h3>
              </div>
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
          </div>
        </div>

        <button class="btn btn-danger-outline btn-signout" (click)="logout()">Sign Out</button>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }
    .state-box.tight { padding: 16px; }
    .card { padding: 20px; margin-bottom: 16px; }
    .card h3 { font-size: 15.5px; font-weight: 700; margin: 0; }

    .head-row { display: flex; gap: 14px; align-items: flex-start; }
    .avatar { width: 64px; height: 64px; border-radius: 50%; background: var(--gradient); color: #fff; display: inline-flex; align-items: center; justify-content: center; font-size: 22px; font-weight: 700; flex-shrink: 0; }
    .head-main { flex: 1; min-width: 0; }
    .name-row { display: flex; align-items: center; gap: 6px; }
    .name-row h2 { font-size: 18px; font-weight: 700; margin: 0; }
    .title-pill { display: inline-flex; margin-top: 4px; font-size: 11.5px; font-weight: 600; color: var(--primary); background: var(--primary-light); padding: 3px 10px; border-radius: 999px; }
    .loc { display: flex; align-items: center; gap: 4px; font-size: 12px; color: var(--text-secondary); margin: 6px 0; }
    .btn-edit { margin-top: 16px; padding: 12px; font-size: 13.5px; }

    .ledger-card { padding: 16px 18px; }
    .ledger-head { display: flex; align-items: center; gap: 8px; }
    .ledger-icon { width: 32px; height: 32px; border-radius: 10px; background: var(--primary-light); color: var(--primary); display: inline-flex; align-items: center; justify-content: center; }
    .ledger-value { font-size: 17px; font-weight: 700; }
    .ledger-label { font-size: 12px; color: var(--text-secondary); flex: 1; }
    .ledger-link { font-size: 12px; font-weight: 600; }
    .ledger-sub { font-size: 11.5px; color: var(--text-muted); margin: 8px 0 0; }

    .section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 8px; }
    .tabs { display: flex; gap: 8px; margin-bottom: 12px; }
    .tab {
      flex: 1; display: inline-flex; align-items: center; justify-content: center; gap: 6px;
      background: var(--input-bg); border: none; border-radius: var(--radius-sm);
      padding: 10px; font-size: 12.5px; font-weight: 600; color: var(--text-secondary); cursor: pointer;
    }
    .tab.active { background: var(--primary-light); color: var(--primary); }
    .tab-count { font-size: 10.5px; background: var(--primary); color: #fff; border-radius: 999px; padding: 1px 7px; }

    .add-skill { background: var(--primary-soft); border-radius: var(--radius-md); padding: 14px; margin: 10px 0 4px; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 0 10px; }
    .opt { font-weight: 400; color: var(--text-muted); }

    .skill-list { display: flex; flex-direction: column; gap: 8px; margin-top: 6px; }
    .skill-row { display: flex; align-items: center; gap: 8px; background: var(--input-bg); border-radius: var(--radius-sm); padding: 11px 12px; }
    .skill-main { flex: 1; display: flex; align-items: center; justify-content: space-between; gap: 8px; min-width: 0; }
    .skill-main b { font-size: 13px; font-weight: 600; }
    .skill-meta { display: inline-flex; align-items: center; gap: 8px; }
    .skill-meta small { font-size: 11px; color: var(--text-muted); }
    .lvl { font-size: 10.5px; font-weight: 600; padding: 3px 9px; border-radius: 999px; background: var(--primary-light); color: var(--primary); }
    .lvl-1 { background: #fff6e5; color: var(--warning); }
    .lvl-3 { background: var(--success-light); color: var(--success); }
    .remove { background: none; border: none; color: var(--text-muted); cursor: pointer; padding: 4px; border-radius: 6px; display: inline-flex; }
    .remove:hover { color: var(--danger); }
    .empty-line { font-size: 12.5px; color: var(--text-muted); margin: 4px 0; }

    .badge-chips { display: flex; flex-wrap: wrap; gap: 8px; }
    .badge-chip {
      display: inline-flex; align-items: center; gap: 6px; font-size: 12px; font-weight: 600;
      background: var(--primary-light); color: var(--primary); border-radius: 999px; padding: 7px 13px;
    }
    .badge-chip img { width: 16px; height: 16px; object-fit: contain; }

    .review-list { display: flex; flex-direction: column; gap: 14px; margin-top: 14px; }
    .review { border-bottom: 1px solid var(--border); padding-bottom: 12px; }
    .review:last-child { border-bottom: none; padding-bottom: 0; }
    .review-head { display: flex; align-items: center; justify-content: space-between; gap: 8px; font-size: 13.5px; }
    .review-comment { font-size: 13px; color: var(--text-secondary); line-height: 1.55; margin: 6px 0; }
    .review-date { font-size: 11px; color: var(--text-muted); }
    .more-btn { margin: 14px auto 0; display: flex; }

    @media (min-width: 900px) {
      .profile-layout { display: grid; grid-template-columns: minmax(0, 0.9fr) minmax(0, 1.4fr); gap: 20px; align-items: start; }
      .profile-col { display: flex; flex-direction: column; gap: 16px; min-width: 0; }
      .profile-col .card { margin-bottom: 0; }
      .btn-edit { width: auto; align-self: flex-start; padding: 12px 22px; }
      .btn-signout { width: auto; }
      .name-row h2 { font-size: 20px; }
      .ledger-link:hover { text-decoration: underline; }
      .skill-row { transition: background 0.15s ease; }
      .skill-row:hover { background: var(--primary-soft); }
      .badge-chip { transition: filter 0.15s ease; }
      .badge-chip:hover { filter: brightness(0.96); }
    }
  `,
})
export class ProfileComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly profileService = inject(ProfileService);
  private readonly usersService = inject(UsersService);
  private readonly walletService = inject(WalletService);
  private readonly toast = inject(ToastService);

  protected readonly SkillType = SkillType;
  protected readonly ProficiencyLevel = ProficiencyLevel;

  protected readonly profile = signal<ProfileDto | null>(null);
  protected readonly balance = signal<WalletBalanceDto | null>(null);
  protected readonly loading = signal(true);
  protected readonly loadError = signal('');

  protected readonly skillTab = signal<'teach' | 'learn'>('teach');

  protected readonly catalog = signal<{ id: string; name: string }[]>([]);
  protected readonly addingSkill = signal(false);
  protected readonly newSkillId = signal('');
  protected readonly newType = signal<number>(SkillType.Offered);
  protected readonly newProficiency = signal<number>(ProficiencyLevel.Intermediate);
  protected readonly newYears = signal<string>('');
  protected readonly savingSkill = signal(false);
  protected readonly removingSkillId = signal<string | null>(null);
  protected readonly skillError = signal('');

  protected readonly badges = signal<UserBadgeDto[]>([]);
  protected readonly badgesLoading = signal(false);
  protected readonly reviews = signal<ReviewDto[]>([]);
  protected readonly reviewsLoading = signal(false);
  private reviewsPage = 0;
  protected readonly hasMoreReviews = signal(false);

  protected readonly offeredSkills = computed(
    () => (this.profile()?.userSkills ?? []).filter((s) => s.type === SkillType.Offered)
  );
  protected readonly seekingSkills = computed(
    () => (this.profile()?.userSkills ?? []).filter((s) => s.type === SkillType.Seeking)
  );
  protected readonly activeSkills = computed(() =>
    this.skillTab() === 'teach' ? this.offeredSkills() : this.seekingSkills()
  );

  protected readonly addableSkills = computed(() => {
    const taken = new Set((this.profile()?.userSkills ?? []).map((s) => s.skillId));
    return this.catalog().filter((s) => !taken.has(s.id));
  });

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  async reload(): Promise<void> {
    this.loading.set(true);
    this.loadError.set('');
    try {
      const [profile, catalog] = await Promise.all([
        firstValueFrom(this.profileService.getProfile()),
        this.loadCatalog(),
      ]);
      this.profile.set(profile);
      void this.loadBadges(profile.userId);
      void this.loadReviews(profile.userId, true);
      void this.loadBalance();
    } catch (err) {
      this.loadError.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  private async loadBalance(): Promise<void> {
    try {
      this.balance.set(await firstValueFrom(this.walletService.getBalance()));
    } catch {
      // Ledger is a nicety — the profile still renders without it.
    }
  }

  private async loadCatalog(): Promise<void> {
    try {
      const catalog = await firstValueFrom(
        this.http.get<SkillCatalogItemDto[]>(`${environment.apiUrl}/api/v1/skills`)
      );
      const seen = new Set<string>();
      this.catalog.set(
        catalog
          .flatMap((c) => c.skills)
          .filter((s) => {
            if (seen.has(s.id)) return false;
            seen.add(s.id);
            return true;
          })
      );
    } catch {
      // Catalog failure only blocks adding skills; profile still renders.
    }
  }

  protected levelLabel(level: number): string {
    return PROFICIENCY_LABELS[level] ?? '';
  }

  protected hoursLabel(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return m ? `${h}.5` : `${h}`;
  }

  protected initials(first: string, last: string): string {
    return `${first[0] ?? ''}${last[0] ?? ''}`.toUpperCase();
  }

  protected locationLabel(p: ProfileDto): string {
    return [p.city, p.country].filter(Boolean).join(', ') || 'Location not set';
  }

  // ── Skills ──────────────────────────────────────────────────────────────
  protected toggleAddSkill(): void {
    this.addingSkill.update((v) => !v);
    this.skillError.set('');
  }

  protected async addSkill(): Promise<void> {
    const yearsRaw = this.newYears().trim();
    const years = yearsRaw === '' ? null : Number(yearsRaw);
    if (years !== null && (!Number.isInteger(years) || years < 0 || years > 60)) {
      this.skillError.set('Years of experience must be a whole number between 0 and 60.');
      return;
    }
    this.savingSkill.set(true);
    this.skillError.set('');
    try {
      const added = await firstValueFrom(
        this.profileService.addSkill({
          skillId: this.newSkillId(),
          type: this.newType(),
          proficiencyLevel: this.newProficiency(),
          yearsOfExperience: years,
        })
      );
      this.profile.update((p) => (p ? { ...p, userSkills: [...p.userSkills, added] } : p));
      this.skillTab.set(added.type === SkillType.Offered ? 'teach' : 'learn');
      this.newSkillId.set(this.addableSkills()[0]?.id ?? '');
      this.newYears.set('');
      this.addingSkill.set(false);
      this.toast.success(`${added.skillName} added.`);
    } catch (err) {
      this.skillError.set(extractApiError(err).message);
    } finally {
      this.savingSkill.set(false);
    }
  }

  protected async removeSkill(skill: UserSkillDto): Promise<void> {
    this.removingSkillId.set(skill.id);
    try {
      await firstValueFrom(this.profileService.removeSkill(skill.id));
      this.profile.update((p) =>
        p ? { ...p, userSkills: p.userSkills.filter((s) => s.id !== skill.id) } : p
      );
      this.toast.success(`${skill.skillName} removed.`);
    } catch (err) {
      this.toast.error(extractApiError(err).message);
    } finally {
      this.removingSkillId.set(null);
    }
  }

  // ── Badges & reviews ────────────────────────────────────────────────────
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
    } catch {
      // Reviews failing shouldn't break the profile.
    } finally {
      this.reviewsLoading.set(false);
    }
  }

  protected loadMoreReviews(): void {
    const p = this.profile();
    if (p) void this.loadReviews(p.userId, false);
  }

  protected logout(): void {
    this.auth.logout();
  }
}
