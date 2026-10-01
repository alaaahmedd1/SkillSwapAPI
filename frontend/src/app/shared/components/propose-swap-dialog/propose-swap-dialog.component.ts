import { Component, OnInit, computed, inject, input, output, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../../environments/environment';
import { SkillCatalogItemDto } from '../../../core/models/api.models';
import { SkillType, UserSkillDto } from '../../../core/models/domain.models';
import { extractApiError } from '../../../core/utils/api-error';
import { ProfileService } from '../../../core/services/profile.service';
import { SwapRequestsService } from '../../../core/services/swap-requests.service';
import { LoadingSpinnerComponent } from '../loading-spinner/loading-spinner.component';

@Component({
  selector: 'app-propose-swap-dialog',
  imports: [LoadingSpinnerComponent],
  template: `
    <div class="backdrop" (click)="onBackdropClick($event)">
      <div class="dialog-card" role="dialog" aria-modal="true">
        @if (success()) {
          <img class="dialog-img" src="/assets/illustrations/email-check.png" alt="" />
          <h2>Swap Request Sent</h2>
          <p>Your proposal is on its way to {{ receiverName() }}. You'll be notified when they respond.</p>
          <button class="btn btn-primary" (click)="close(true)">View My Swaps</button>
        } @else {
          <h2>Propose Swap</h2>
          <p class="with">with <b>{{ receiverName() }}</b></p>

          @if (loading()) {
            <div class="dialog-state"><app-spinner [size]="30" /></div>
          } @else if (loadError()) {
            <p class="dialog-error">{{ loadError() }}</p>
            <button class="btn btn-soft" (click)="close(false)">Close</button>
          } @else if (!offeredSkills().length) {
            <p class="dialog-error">
              You need at least one <b>offered</b> skill on your profile before proposing a swap.
            </p>
            <button class="btn btn-soft" (click)="close(false)">Close</button>
          } @else {
            <div class="field">
              <label for="offered-skill">You'll teach</label>
              <select id="offered-skill" class="input" [value]="offeredSkillId()" (change)="offeredSkillId.set($any($event.target).value)">
                @for (skill of offeredSkills(); track skill.skillId) {
                  <option [value]="skill.skillId">{{ skill.skillName }}</option>
                }
              </select>
            </div>

            <div class="field">
              <label for="requested-skill">You want to learn</label>
              <select id="requested-skill" class="input" [value]="requestedSkillId()" (change)="requestedSkillId.set($any($event.target).value)">
                @for (skill of requestedOptions(); track skill.skillId) {
                  <option [value]="skill.skillId">{{ skill.skillName }}</option>
                }
              </select>
            </div>

            <div class="field">
              <label for="schedule-details">Schedule details <span class="opt">(optional)</span></label>
              <textarea
                id="schedule-details"
                class="input textarea"
                rows="2"
                placeholder="e.g. Weekdays after 6pm, GMT+2"
                [value]="scheduleDetails()"
                (input)="scheduleDetails.set($any($event.target).value)"
              ></textarea>
            </div>

            @if (submitError()) {
              <p class="dialog-error">{{ submitError() }}</p>
            }

            <button class="btn btn-primary" [disabled]="submitting() || !offeredSkillId() || !requestedSkillId()" (click)="submit()">
              @if (submitting()) { <app-spinner [size]="18" /> } @else { Send Request }
            </button>
            <button class="btn btn-ghost" [disabled]="submitting()" (click)="close(false)">Cancel</button>
          }
        }
      </div>
    </div>
  `,
  styles: `
    :host { display: contents; }
    .backdrop {
      position: fixed; inset: 0; z-index: 900;
      background: rgba(31, 31, 61, 0.45);
      backdrop-filter: blur(4px);
      display: flex; align-items: flex-end; justify-content: center;
      animation: fade-in 0.2s ease both;
    }
    .dialog-card {
      width: 100%; max-width: 480px;
      background: #fff; border-radius: 24px 24px 0 0;
      padding: 26px 22px calc(22px + env(safe-area-inset-bottom, 0px));
      box-shadow: 0 -12px 48px rgba(0, 0, 0, 0.18);
      animation: sheet-in 0.28s cubic-bezier(0.32, 0.9, 0.4, 1) both;
      h2 { font-size: 19px; font-weight: 700; margin: 0 0 4px; }
      .with { font-size: 13px; color: var(--text-secondary); margin: 0 0 18px; }
      .btn-ghost { background: none; color: var(--text-secondary); margin-top: 8px; }
    }
    .dialog-state { display: flex; justify-content: center; padding: 30px 0; }
    .dialog-error { font-size: 13px; color: var(--danger); line-height: 1.55; margin: 4px 0 14px; }
    .dialog-img { width: 130px; height: 130px; object-fit: contain; margin: 0 auto 8px; }
    .opt { font-weight: 400; color: var(--text-muted); }
    .textarea { resize: none; line-height: 1.5; }
    @keyframes fade-in { from { opacity: 0; } to { opacity: 1; } }
    @keyframes sheet-in { from { transform: translateY(60px); opacity: 0.6; } to { transform: translateY(0); opacity: 1; } }
  `,
})
export class ProposeSwapDialogComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly profileService = inject(ProfileService);
  private readonly swapRequests = inject(SwapRequestsService);

  readonly receiverId = input.required<string>();
  readonly receiverName = input.required<string>();
  /** Skill names the other user offers (matched against the catalog). */
  readonly theirSkillNames = input<string[]>([]);
  readonly closed = output<boolean>(); // true = a swap was created

  protected readonly loading = signal(true);
  protected readonly loadError = signal('');
  protected readonly offeredSkills = signal<UserSkillDto[]>([]);
  protected readonly catalog = signal<{ skillId: string; skillName: string }[]>([]);
  protected readonly offeredSkillId = signal('');
  protected readonly requestedSkillId = signal('');
  protected readonly scheduleDetails = signal('');
  protected readonly submitting = signal(false);
  protected readonly submitError = signal('');
  protected readonly success = signal(false);

  protected readonly requestedOptions = computed(() => {
    const catalog = this.catalog();
    const names = this.theirSkillNames().map((n) => n.trim().toLowerCase()).filter(Boolean);
    if (!names.length) return catalog;
    const matched = catalog.filter((s) => names.some((n) => s.skillName.trim().toLowerCase() === n || s.skillName.trim().toLowerCase().includes(n)));
    return matched.length ? matched : catalog;
  });

  async ngOnInit(): Promise<void> {
    try {
      const [profile, catalog] = await Promise.all([
        firstValueFrom(this.profileService.getProfile()),
        firstValueFrom(this.http.get<SkillCatalogItemDto[]>(`${environment.apiUrl}/api/v1/skills`)),
      ]);
      const offered = profile.userSkills.filter((s) => s.type === SkillType.Offered);
      const seen = new Set<string>();
      const flat = catalog
        .flatMap((c) => c.skills)
        .filter((s) => {
          if (seen.has(s.id)) return false;
          seen.add(s.id);
          return true;
        })
        .map((s) => ({ skillId: s.id, skillName: s.name }));
      this.offeredSkills.set(offered);
      this.catalog.set(flat);
      this.offeredSkillId.set(offered[0]?.skillId ?? '');
      const requested = this.requestedOptions();
      this.requestedSkillId.set(requested[0]?.skillId ?? '');
    } catch (err) {
      this.loadError.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected async submit(): Promise<void> {
    if (this.submitting()) return;
    this.submitting.set(true);
    this.submitError.set('');
    try {
      await firstValueFrom(
        this.swapRequests.create({
          receiverId: this.receiverId(),
          offeredSkillId: this.offeredSkillId(),
          requestedSkillId: this.requestedSkillId(),
          proposedScheduleDetails: this.scheduleDetails().trim() || null,
        })
      );
      this.success.set(true);
    } catch (err) {
      this.submitError.set(extractApiError(err).message);
    } finally {
      this.submitting.set(false);
    }
  }

  protected close(proposed: boolean): void {
    this.closed.emit(proposed);
  }

  protected onBackdropClick(event: MouseEvent): void {
    if (event.target === event.currentTarget && !this.submitting()) {
      this.closed.emit(false);
    }
  }
}
