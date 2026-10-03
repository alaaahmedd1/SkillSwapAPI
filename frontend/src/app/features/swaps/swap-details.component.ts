import { DatePipe } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import {
  ProposalStatus,
  SwapRequestDetailsDto,
  SwapRequestStatus,
} from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { SwapRequestsService } from '../../core/services/swap-requests.service';
import { ToastService } from '../../core/services/toast.service';
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

const DURATIONS = [30, 60, 120];

function toTimeOnly(value: string): string {
  return value.length === 5 ? `${value}:00` : value;
}

@Component({
  selector: 'app-swap-details',
  imports: [DatePipe, RouterLink, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <a class="icon-btn" topbar-actions [routerLink]="['/swaps']" aria-label="Back to swaps">
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 12H5m7-7-7 7 7 7"/></svg>
      </a>

      @if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (error()) {
        <div class="state-box">
          <p>{{ error() }}</p>
          <button class="btn btn-soft btn-sm" (click)="reload()">Try again</button>
        </div>
      } @else if (swap(); as s) {
        <div class="details-layout">
        <section class="card head-card">
          <div class="row-between">
            <h2>{{ otherName(s) }}</h2>
            <span class="status" [class]="statusMeta(s.status).className">{{ statusMeta(s.status).label }}</span>
          </div>
          <span class="direction" [class.incoming]="!isMine(s)">{{ isMine(s) ? 'Outgoing request' : 'Incoming request' }}</span>
          <div class="skill-swap">
            <div class="skill-box">
              <small>You give</small>
              <b>{{ s.offeredSkill.skillName }}</b>
              <span class="cat">{{ s.offeredSkill.categoryName }}</span>
            </div>
            <span class="swap-arrow">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M8 3 4 7l4 4"/><path d="M4 7h16"/><path d="m16 21 4-4-4-4"/><path d="M20 17H4"/></svg>
            </span>
            <div class="skill-box">
              <small>You get</small>
              <b>{{ s.requestedSkill.skillName }}</b>
              <span class="cat">{{ s.requestedSkill.categoryName }}</span>
            </div>
          </div>
          @if (s.proposedScheduleDetails) {
            <p class="schedule-note"><b>Schedule note:</b> {{ s.proposedScheduleDetails }}</p>
          }
          <small class="date">Requested {{ s.createdAtUtc | date: 'MMM d, yyyy' }}</small>

          @if (actionError()) { <p class="field-error">{{ actionError() }}</p> }

          <div class="actions">
            @if (s.status === Status.Pending && !isMine(s)) {
              <button class="btn btn-primary btn-sm" [disabled]="acting()" (click)="act('accept')">Accept</button>
              <button class="btn btn-danger-outline btn-sm" [disabled]="acting()" (click)="act('reject')">Reject</button>
            }
            @if (s.status === Status.Pending && isMine(s)) {
              <button class="btn btn-danger-outline btn-sm" [disabled]="acting()" (click)="act('cancel')">Cancel Request</button>
            }
            @if (s.status === Status.Accepted) {
              <a class="btn btn-primary btn-sm" [routerLink]="['/swaps', s.id, 'schedule']">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg>
                Schedule Session
              </a>
              <a class="btn btn-outline btn-sm" [routerLink]="['/messages', s.id]">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
                Open Chat
              </a>
              <a class="btn btn-soft btn-sm" [routerLink]="['/sessions', s.id]">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M23 7l-7 5 7 5V7z"/><rect x="1" y="5" width="15" height="14" rx="2"/></svg>
                Join Meeting
              </a>
              @if (myConfirmed(s)) {
                <span class="confirm-chip">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>
                  Waiting for {{ otherFirstName(s) }} to confirm
                </span>
              } @else {
                <button class="btn btn-outline btn-sm" [disabled]="acting()" (click)="act('complete')">
                  {{ otherConfirmed(s) ? 'Confirm Completion' : 'Mark Completed' }}
                </button>
              }
            }
            @if (s.status === Status.Completed) {
              <a class="btn btn-primary btn-sm" [routerLink]="['/swaps', s.id, 'review']">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m12 2 3.1 6.3 6.9 1-5 4.9 1.2 6.9-6.2-3.3-6.2 3.3L7 14.2 2 9.3l6.9-1z"/></svg>
                Leave a Review
              </a>
              <a class="btn btn-soft btn-sm" [routerLink]="['/messages', s.id]">
                <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>
                Open Chat
              </a>
            }
          </div>
        </section>

        @if (s.status === Status.Pending || s.status === Status.Accepted) {
          <section class="card">
            <div class="section-head">
              <h3>Session Proposals</h3>
              @if (!showProposalForm()) {
                <button class="btn btn-soft btn-sm" (click)="toggleProposalForm()">+ Propose time</button>
              }
            </div>

            @if (showProposalForm()) {
              <div class="proposal-form">
                <div class="field">
                  <label for="prop-date">Date</label>
                  <input id="prop-date" type="date" class="input" [min]="today()" [value]="propDate()" (change)="propDate.set($any($event.target).value)" />
                </div>
                <div class="grid-2">
                  <div class="field">
                    <label for="prop-time">Start time</label>
                    <input id="prop-time" type="time" class="input" [value]="propTime()" (change)="propTime.set($any($event.target).value)" />
                  </div>
                  <div class="field">
                    <label for="prop-duration">Duration</label>
                    <select id="prop-duration" class="input" [value]="propDuration()" (change)="propDuration.set(+$any($event.target).value)">
                      @for (d of durations; track d) {
                        <option [value]="d">{{ d }} min</option>
                      }
                    </select>
                  </div>
                </div>
                <p class="end-note">Ends at {{ computedEndTime() }}</p>
                @if (proposalError()) { <p class="field-error">{{ proposalError() }}</p> }
                <div class="name-actions">
                  <button class="btn btn-primary btn-sm" [disabled]="savingProposal()" (click)="createProposal()">
                    @if (savingProposal()) { <app-spinner [size]="16" /> } @else { Send Proposal }
                  </button>
                  <button class="btn btn-soft btn-sm" [disabled]="savingProposal()" (click)="toggleProposalForm()">Cancel</button>
                </div>
              </div>
            }

            @if (!s.sessionProposals.length && !showProposalForm()) {
              <p class="empty-line">No time proposals yet.</p>
            }
            <div class="proposal-list">
              @for (proposal of s.sessionProposals; track proposal.id) {
                <div class="proposal">
                  <div class="proposal-head">
                    <b>{{ proposal.scheduledDate | date: 'EEE, MMM d' }} · {{ proposal.startTime.slice(0, 5) }}–{{ proposal.endTime.slice(0, 5) }}</b>
                    <span class="prop-status" [class]="propStatusClass(proposal.status)">{{ propStatusLabel(proposal.status) }}</span>
                  </div>
                  <small class="prop-by">
                    Proposed by {{ proposal.proposerId === myId() ? 'you' : otherFirstName(s) }} · {{ proposal.durationMinutes }} min
                  </small>
                  @if (proposal.status === Proposal.Proposed && proposal.proposerId !== myId()) {
                    <div class="prop-actions">
                      <button class="btn btn-primary btn-sm" [disabled]="acting()" (click)="actOnProposal(proposal.id, 'accept')">Accept</button>
                      <button class="btn btn-danger-outline btn-sm" [disabled]="acting()" (click)="actOnProposal(proposal.id, 'reject')">Reject</button>
                    </div>
                  }
                </div>
              }
            </div>
          </section>
        }
        </div>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .icon-btn { width: 38px; height: 38px; display: inline-flex; align-items: center; justify-content: center; background: var(--input-bg); color: var(--text); border-radius: 12px; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }
    .card { padding: 20px; margin-bottom: 16px; }
    .card h3 { font-size: 15.5px; font-weight: 700; margin: 0; }

    .row-between { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
    .row-between h2 { font-size: 17px; font-weight: 700; margin: 0; }
    .status { font-size: 11px; font-weight: 600; padding: 4px 10px; border-radius: 999px; flex-shrink: 0; }
    .status.pending { background: #fff6e5; color: var(--warning); }
    .status.accepted { background: var(--success-light); color: var(--success); }
    .status.completed { background: var(--primary-light); color: var(--primary); }
    .status.rejected { background: var(--danger-light); color: var(--danger); }
    .direction { font-size: 11px; font-weight: 600; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.4px; }
    .direction.incoming { color: var(--primary); }

    .skill-swap { display: flex; align-items: center; gap: 10px; margin: 16px 0; }
    .skill-box { flex: 1; background: var(--input-bg); border-radius: var(--radius-sm); padding: 12px; display: flex; flex-direction: column; gap: 2px; }
    .skill-box small { font-size: 10.5px; color: var(--text-muted); text-transform: uppercase; letter-spacing: 0.4px; }
    .skill-box b { font-size: 13.5px; font-weight: 600; }
    .skill-box .cat { font-size: 11px; color: var(--text-secondary); }
    .swap-arrow { color: var(--primary); flex-shrink: 0; }

    .schedule-note { font-size: 12.5px; color: var(--text-secondary); background: var(--primary-soft); border-radius: var(--radius-sm); padding: 10px 12px; margin: 0 0 10px; }
    .date { font-size: 11px; color: var(--text-muted); }
    .actions { display: flex; flex-wrap: wrap; gap: 10px; margin-top: 16px; }
    .actions .btn { display: inline-flex; align-items: center; gap: 6px; }
    .confirm-chip { display: inline-flex; align-items: center; gap: 6px; font-size: 12px; font-weight: 600; color: var(--warning); background: #fff6e5; border-radius: 999px; padding: 7px 13px; }

    .section-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 6px; }
    .proposal-form { background: var(--primary-soft); border-radius: var(--radius-md); padding: 14px; margin: 10px 0; }
    .grid-2 { display: grid; grid-template-columns: 1fr 1fr; gap: 0 10px; }
    .end-note { font-size: 12px; color: var(--text-secondary); margin: 0 0 12px; }
    .name-actions { display: flex; gap: 10px; }
    .empty-line { font-size: 12.5px; color: var(--text-muted); margin: 4px 0; }

    .proposal-list { display: flex; flex-direction: column; gap: 12px; margin-top: 10px; }
    .proposal { border: 1px solid var(--border); border-radius: var(--radius-sm); padding: 12px; }
    .proposal-head { display: flex; align-items: center; justify-content: space-between; gap: 8px; font-size: 13px; }
    .prop-status { font-size: 10.5px; font-weight: 600; padding: 3px 9px; border-radius: 999px; background: var(--input-bg); color: var(--text-secondary); }
    .prop-status.proposed { background: #fff6e5; color: var(--warning); }
    .prop-status.accepted { background: var(--success-light); color: var(--success); }
    .prop-status.rejected { background: var(--danger-light); color: var(--danger); }
    .prop-by { font-size: 11px; color: var(--text-muted); display: block; margin-top: 4px; }
    .prop-actions { display: flex; gap: 8px; margin-top: 10px; }

    @media (min-width: 900px) {
      .details-layout { display: grid; grid-template-columns: minmax(0, 1.25fr) minmax(0, 1fr); gap: 20px; align-items: start; }
      .icon-btn:hover { background: var(--primary-light); color: var(--primary); }
    }
  `,
})
export class SwapDetailsComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly swapRequests = inject(SwapRequestsService);
  private readonly toast = inject(ToastService);

  protected readonly Status = SwapRequestStatus;
  protected readonly Proposal = ProposalStatus;
  protected readonly durations = DURATIONS;

  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly swap = signal<SwapRequestDetailsDto | null>(null);
  protected readonly acting = signal(false);
  protected readonly actionError = signal('');

  protected readonly showProposalForm = signal(false);
  protected readonly propDate = signal('');
  protected readonly propTime = signal('18:00');
  protected readonly propDuration = signal(60);
  protected readonly savingProposal = signal(false);
  protected readonly proposalError = signal('');

  protected readonly myId = computed(() => this.auth.user()?.userId ?? '');

  async ngOnInit(): Promise<void> {
    await this.reload();
  }

  protected async reload(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const id = this.route.snapshot.paramMap.get('id') ?? '';
      this.swap.set(await firstValueFrom(this.swapRequests.details(id)));
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected isMine(s: SwapRequestDetailsDto): boolean {
    return s.requesterId === this.myId();
  }

  protected otherName(s: SwapRequestDetailsDto): string {
    return this.isMine(s)
      ? `${s.receiverFirstName} ${s.receiverLastName}`
      : `${s.requesterFirstName} ${s.requesterLastName}`;
  }

  protected otherFirstName(s: SwapRequestDetailsDto): string {
    return this.isMine(s) ? s.receiverFirstName : s.requesterFirstName;
  }

  protected myConfirmed(s: SwapRequestDetailsDto): boolean {
    return this.isMine(s) ? s.isRequesterConfirmed : s.isReceiverConfirmed;
  }

  protected otherConfirmed(s: SwapRequestDetailsDto): boolean {
    return this.isMine(s) ? s.isReceiverConfirmed : s.isRequesterConfirmed;
  }

  protected statusMeta(status: number): { label: string; className: string } {
    return STATUS_META[status] ?? { label: 'Unknown', className: 'pending' };
  }

  protected propStatusLabel(status: number): string {
    return status === ProposalStatus.Proposed ? 'Awaiting reply' : status === ProposalStatus.Accepted ? 'Accepted' : 'Declined';
  }

  protected propStatusClass(status: number): string {
    return status === ProposalStatus.Proposed ? 'proposed' : status === ProposalStatus.Accepted ? 'accepted' : 'rejected';
  }

  protected today(): string {
    return new Date().toISOString().slice(0, 10);
  }

  protected computedEndTime(): string {
    return this.addMinutes(this.propTime(), this.propDuration());
  }

  private addMinutes(time: string, minutes: number): string {
    const [h, m] = time.split(':').map(Number);
    const total = (h * 60 + m + minutes) % (24 * 60);
    const hh = String(Math.floor(total / 60)).padStart(2, '0');
    const mm = String(total % 60).padStart(2, '0');
    return `${hh}:${mm}`;
  }

  protected async act(action: 'accept' | 'reject' | 'cancel' | 'complete'): Promise<void> {
    const s = this.swap();
    if (!s || this.acting()) return;
    this.acting.set(true);
    this.actionError.set('');
    try {
      await firstValueFrom(this.swapRequests[action](s.id));
      await this.reload();
      if (action === 'complete') {
        const updated = this.swap();
        this.toast.success(
          updated?.status === SwapRequestStatus.Completed
            ? 'Swap marked as completed.'
            : `Completion confirmed — waiting for ${updated ? this.otherFirstName(updated) : 'the other party'} to confirm.`
        );
      } else {
        this.toast.success(action === 'accept' ? 'Swap accepted.' : 'Swap updated.');
      }
    } catch (err) {
      this.actionError.set(extractApiError(err).message);
    } finally {
      this.acting.set(false);
    }
  }

  protected toggleProposalForm(): void {
    this.showProposalForm.update((v) => !v);
    this.proposalError.set('');
    if (this.showProposalForm() && !this.propDate()) {
      this.propDate.set(this.today());
    }
  }

  protected async createProposal(): Promise<void> {
    const s = this.swap();
    if (!s || this.savingProposal()) return;
    if (!this.propDate() || !this.propTime()) {
      this.proposalError.set('Pick a date and start time.');
      return;
    }
    this.savingProposal.set(true);
    this.proposalError.set('');
    try {
      await firstValueFrom(
        this.swapRequests.createProposal(s.id, {
          scheduledDate: this.propDate(),
          startTime: toTimeOnly(this.propTime()),
          endTime: toTimeOnly(this.addMinutes(this.propTime(), this.propDuration())),
          durationMinutes: this.propDuration(),
        })
      );
      this.toast.success('Proposal sent.');
      this.showProposalForm.set(false);
      await this.reload();
    } catch (err) {
      this.proposalError.set(extractApiError(err).message);
    } finally {
      this.savingProposal.set(false);
    }
  }

  protected async actOnProposal(proposalId: string, action: 'accept' | 'reject'): Promise<void> {
    const s = this.swap();
    if (!s || this.acting()) return;
    this.acting.set(true);
    try {
      await firstValueFrom(
        action === 'accept'
          ? this.swapRequests.acceptProposal(s.id, proposalId)
          : this.swapRequests.rejectProposal(s.id, proposalId)
      );
      this.toast.success(action === 'accept' ? 'Proposal accepted.' : 'Proposal declined.');
      await this.reload();
    } catch (err) {
      this.toast.error(extractApiError(err).message);
    } finally {
      this.acting.set(false);
    }
  }
}
