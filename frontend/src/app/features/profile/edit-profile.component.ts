import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AvailabilitySlotDto, ProfileDto } from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { ProfileService } from '../../core/services/profile.service';
import { ToastService } from '../../core/services/toast.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

interface DayRow {
  dayOfWeek: number; // 0 = Sunday … 6 = Saturday
  name: string;
  blocks: Record<number, boolean>; // 1 Morning, 2 Afternoon, 3 Evening
}

const DAYS: { dayOfWeek: number; name: string }[] = [
  { dayOfWeek: 0, name: 'Sun' },
  { dayOfWeek: 1, name: 'Mon' },
  { dayOfWeek: 2, name: 'Tue' },
  { dayOfWeek: 3, name: 'Wed' },
  { dayOfWeek: 4, name: 'Thu' },
  { dayOfWeek: 5, name: 'Fri' },
  { dayOfWeek: 6, name: 'Sat' },
];

const BLOCKS = [
  { value: 1, label: 'Morning', hint: '7:00 AM – 12:00 PM' },
  { value: 2, label: 'Afternoon', hint: '12:00 PM – 6:00 PM' },
  { value: 3, label: 'Evening', hint: '6:00 PM – 10:00 PM' },
];

@Component({
  selector: 'app-edit-profile',
  imports: [RouterLink, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <a class="icon-btn" topbar-actions routerLink="/profile" aria-label="Back to profile">
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 12H5m7-7-7 7 7 7"/></svg>
      </a>

      <h2 class="page-title">Edit Profile &amp; <span>Availability</span></h2>
      <p class="page-sub">Update your public teacher profile and customize your open exchange hours.</p>

      @if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (loadError()) {
        <div class="state-box">
          <p>{{ loadError() }}</p>
          <button class="btn btn-soft btn-sm" (click)="reload()">Try again</button>
        </div>
      } @else {
        <section class="card">
          <h3 class="card-title">Personal Information <small>PUBLIC DETAILS</small></h3>

          <div class="field">
            <label for="edit-first">Full Name</label>
            <div class="name-grid">
              <input id="edit-first" class="input" placeholder="First name" [value]="firstName()" (input)="firstName.set($any($event.target).value)" />
              <input id="edit-last" class="input" placeholder="Last name" [value]="lastName()" (input)="lastName.set($any($event.target).value)" />
            </div>
          </div>

          <div class="field">
            <label for="edit-title">Primary Role</label>
            <input id="edit-title" class="input" placeholder="e.g. Junior UX Designer" [value]="title()" (input)="title.set($any($event.target).value)" />
          </div>

          <div class="field">
            <label for="edit-city">Location &amp; Timezone</label>
            <div class="name-grid">
              <input id="edit-city" class="input" placeholder="City" [value]="city()" (input)="city.set($any($event.target).value)" />
              <input id="edit-country" class="input" placeholder="Country" [value]="country()" (input)="country.set($any($event.target).value)" />
            </div>
          </div>

          <div class="field">
            <label for="edit-tz">Timezone</label>
            <input id="edit-tz" class="input" placeholder="e.g. UTC+2" [value]="timeZone()" (input)="timeZone.set($any($event.target).value)" />
          </div>

          <div class="field">
            <label for="edit-bio">Exchange Bio <small class="opt">max 200</small></label>
            <textarea id="edit-bio" class="input bio" rows="4" maxlength="200" placeholder="What you teach, what you're learning, and how you like to exchange skills…" [value]="bio()" (input)="bio.set($any($event.target).value)"></textarea>
          </div>
        </section>

        <section class="card">
          <div class="week-head">
            <h3 class="card-title no-margin">Weekly Swap Schedule</h3>
            <span class="active-pill">{{ activeCount() }} active slots</span>
          </div>
          <p class="week-sub">Tap slots when you're open for 1-on-1. Time-block exchanges.</p>

          <div class="schedule">
            <div class="schedule-row header">
              <span></span>
              @for (block of blocks; track block.value) {
                <span class="block-label" [title]="block.hint">{{ block.label.slice(0, 3) }}</span>
              }
            </div>
            @for (day of days(); track day.dayOfWeek) {
              <div class="schedule-row">
                <span class="day-name">{{ day.name }}</span>
                @for (block of blocks; track block.value) {
                  <button
                    class="slot"
                    [class.on]="day.blocks[block.value]"
                    (click)="toggleSlot(day, block.value)"
                    [attr.aria-label]="day.name + ' ' + block.label"
                  >
                    @if (day.blocks[block.value]) {
                      <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>
                    }
                  </button>
                }
              </div>
            }
          </div>
        </section>

        <section class="card">
          <h3 class="card-title">Exchange Rules</h3>

          <div class="rule">
            <div class="rule-text">
              <b>Open for Instant Swaps</b>
              <small>Allow teachers to book open sessions with you instantly without prior chat.</small>
            </div>
            <button class="switch" [class.on]="openForInstantSwaps()" (click)="openForInstantSwaps.set(!openForInstantSwaps())" role="switch" [attr.aria-checked]="openForInstantSwaps()">
              <span class="knob"></span>
            </button>
          </div>

          <div class="rule">
            <div class="rule-text">
              <b>Online Only Sessions</b>
              <small>All sessions happen remotely over video chat.</small>
            </div>
            <button class="switch" [class.on]="onlineOnly()" (click)="onlineOnly.set(!onlineOnly())" role="switch" [attr.aria-checked]="onlineOnly()">
              <span class="knob"></span>
            </button>
          </div>

          <div class="rule">
            <div class="rule-text">
              <b>Auto-match Barter Requests</b>
              <small>Automatically surface matching swap partners for your skills.</small>
            </div>
            <button class="switch" [class.on]="autoMatch()" (click)="autoMatch.set(!autoMatch())" role="switch" [attr.aria-checked]="autoMatch()">
              <span class="knob"></span>
            </button>
          </div>
        </section>

        @if (error()) { <p class="field-error">{{ error() }}</p> }
        <div class="actions">
          <button class="btn btn-soft" routerLink="/profile">Cancel</button>
          <button class="btn btn-primary" [disabled]="saving()" (click)="save()">
            @if (saving()) { <app-spinner [size]="16" /> } @else { Save Changes }
          </button>
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

    .card { padding: 20px; margin-bottom: 16px; }
    .card-title { font-size: 15.5px; font-weight: 700; margin: 0 0 14px; }
    .card-title small { font-size: 10px; font-weight: 600; color: var(--text-muted); letter-spacing: 0.5px; margin-left: 8px; }
    .card-title.no-margin { margin-bottom: 0; }

    .name-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
    .opt { font-weight: 400; color: var(--text-muted); }
    textarea.bio { resize: vertical; font-family: inherit; }

    .week-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px; }
    .active-pill { font-size: 11px; font-weight: 600; color: var(--primary); background: var(--primary-light); padding: 4px 10px; border-radius: 999px; }
    .week-sub { font-size: 12px; color: var(--text-secondary); margin: 0 0 14px; }

    .schedule { display: flex; flex-direction: column; gap: 8px; }
    .schedule-row { display: grid; grid-template-columns: 44px 1fr 1fr 1fr; gap: 8px; align-items: center; }
    .schedule-row.header { margin-bottom: 2px; }
    .block-label { font-size: 11px; font-weight: 600; color: var(--text-muted); text-align: center; }
    .day-name { font-size: 12.5px; font-weight: 600; color: var(--text-secondary); }
    .slot {
      height: 38px; border-radius: 11px; border: 1.5px solid var(--border); background: var(--input-bg);
      display: inline-flex; align-items: center; justify-content: center; color: #fff; cursor: pointer;
      transition: all 0.15s ease;
    }
    .slot.on { background: var(--primary); border-color: var(--primary); box-shadow: 0 4px 12px rgba(124, 92, 252, 0.3); }

    .rule { display: flex; align-items: flex-start; justify-content: space-between; gap: 14px; padding: 12px 0; border-bottom: 1px solid var(--border); }
    .rule:last-child { border-bottom: none; padding-bottom: 0; }
    .rule-text b { display: block; font-size: 13.5px; font-weight: 600; }
    .rule-text small { font-size: 11.5px; color: var(--text-secondary); line-height: 1.5; }

    .switch {
      width: 46px; height: 27px; border-radius: 999px; background: var(--border); border: none;
      position: relative; cursor: pointer; flex-shrink: 0; transition: background 0.18s ease;
    }
    .switch .knob {
      position: absolute; top: 3px; left: 3px; width: 21px; height: 21px; border-radius: 50%;
      background: #fff; box-shadow: 0 2px 6px rgba(0, 0, 0, 0.18); transition: transform 0.18s ease;
    }
    .switch.on { background: var(--primary); }
    .switch.on .knob { transform: translateX(19px); }

    .actions { display: flex; gap: 10px; margin-bottom: 14px; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }

    @media (min-width: 900px) {
      .page-title { font-size: 26px; }
      .card { max-width: 720px; }
      .actions { max-width: 720px; }
      .actions .btn { width: auto; flex: 0 0 auto; }
      .icon-btn { transition: background 0.15s ease, color 0.15s ease; }
      .icon-btn:hover { background: var(--primary-soft); color: var(--primary); }
      .slot:hover { border-color: var(--primary); }
    }
  `,
})
export class EditProfileComponent implements OnInit {
  private readonly profileService = inject(ProfileService);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly blocks = BLOCKS;
  protected readonly days = signal<DayRow[]>(DAYS.map((d) => ({ ...d, blocks: { 1: false, 2: false, 3: false } })));

  protected readonly loading = signal(true);
  protected readonly loadError = signal('');
  protected readonly saving = signal(false);
  protected readonly error = signal('');

  protected readonly firstName = signal('');
  protected readonly lastName = signal('');
  protected readonly title = signal('');
  protected readonly bio = signal('');
  protected readonly city = signal('');
  protected readonly country = signal('');
  protected readonly timeZone = signal('');
  protected readonly openForInstantSwaps = signal(true);
  protected readonly onlineOnly = signal(false);
  protected readonly autoMatch = signal(false);

  private profile: ProfileDto | null = null;

  protected readonly activeCount = signal(0);

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  protected toggleSlot(day: DayRow, block: number): void {
    day.blocks[block] = !day.blocks[block];
    this.days.update((rows) => [...rows]);
    this.activeCount.set(this.countActive());
  }

  protected async save(): Promise<void> {
    const first = this.firstName().trim();
    const last = this.lastName().trim();
    if (!first || !last) {
      this.error.set('Both first and last name are required.');
      return;
    }
    this.saving.set(true);
    this.error.set('');
    const slots: AvailabilitySlotDto[] = [];
    for (const day of this.days()) {
      for (const block of BLOCKS) {
        if (day.blocks[block.value]) slots.push({ dayOfWeek: day.dayOfWeek, timeBlock: block.value });
      }
    }
    try {
      await firstValueFrom(
        this.profileService.updateProfile({
          firstName: first,
          lastName: last,
          title: this.title().trim() || null,
          bio: this.bio().trim() || null,
          city: this.city().trim() || null,
          country: this.country().trim() || null,
          timeZone: this.timeZone().trim() || null,
          openForInstantSwaps: this.openForInstantSwaps(),
          onlineOnly: this.onlineOnly(),
          autoMatchBarterRequests: this.autoMatch(),
        })
      );
      await firstValueFrom(this.profileService.updateAvailability(slots));
      this.auth.applyNameUpdate(first, last);
      this.toast.success('Profile updated.');
      void this.router.navigateByUrl('/profile');
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.saving.set(false);
    }
  }

  private countActive(): number {
    return this.days().reduce(
      (sum, day) => sum + Object.values(day.blocks).filter(Boolean).length,
      0
    );
  }

  private async reload(): Promise<void> {
    this.loading.set(true);
    this.loadError.set('');
    try {
      const [profile, availability] = await Promise.all([
        firstValueFrom(this.profileService.getProfile()),
        firstValueFrom(this.profileService.getAvailability()).catch(() => [] as AvailabilitySlotDto[]),
      ]);
      this.profile = profile;
      this.firstName.set(profile.firstName);
      this.lastName.set(profile.lastName);
      this.title.set(profile.title ?? '');
      this.bio.set(profile.bio ?? '');
      this.city.set(profile.city ?? '');
      this.country.set(profile.country ?? '');
      this.timeZone.set(profile.timeZone ?? '');
      this.openForInstantSwaps.set(profile.openForInstantSwaps);
      this.onlineOnly.set(profile.onlineOnly);
      this.autoMatch.set(profile.autoMatchBarterRequests);

      this.days.set(
        DAYS.map((d) => ({
          ...d,
          blocks: Object.fromEntries(
            BLOCKS.map((b) => [
              b.value,
              availability.some((s) => s.dayOfWeek === d.dayOfWeek && s.timeBlock === b.value),
            ])
          ) as Record<number, boolean>,
        }))
      );
      this.activeCount.set(this.countActive());
    } catch (err) {
      this.loadError.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }
}
