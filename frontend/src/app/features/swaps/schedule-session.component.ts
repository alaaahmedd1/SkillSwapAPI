import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { SwapRequestDetailsDto } from '../../core/models/domain.models';
import { SwapRequestsService } from '../../core/services/swap-requests.service';
import { ToastService } from '../../core/services/toast.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

interface DayOption {
  iso: string; // yyyy-MM-dd
  weekday: string;
  dayNumber: string;
}

interface TimeOption {
  value: string; // HH:mm
  label: string;
  pm: boolean;
}

const TIME_SLOTS: TimeOption[] = [
  { value: '09:00', label: '9:00', pm: false },
  { value: '10:00', label: '10:00', pm: false },
  { value: '11:00', label: '11:00', pm: false },
  { value: '14:00', label: '2:00', pm: true },
  { value: '16:00', label: '4:00', pm: true },
  { value: '18:00', label: '6:00', pm: true },
];

const DURATIONS = [
  { minutes: 30, label: '30 Min' },
  { minutes: 60, label: '1 H' },
  { minutes: 120, label: '2 H' },
];

@Component({
  selector: 'app-schedule-session',
  imports: [RouterLink, AppShellComponent, LoadingSpinnerComponent],
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
        <h2 class="page-title">Schedule <span>Session</span></h2>
        <p class="page-sub">Pick a time that works for both of you — your partner confirms next.</p>

        <section class="card">
          <h3 class="card-title">Select day</h3>
          <div class="day-row">
            @for (day of days(); track day.iso) {
              <button class="day-chip" [class.active]="selectedDay() === day.iso" (click)="selectedDay.set(day.iso)">
                <b>{{ day.dayNumber }}</b>
                <small>{{ day.weekday }}</small>
              </button>
            }
          </div>

          <h3 class="card-title">Select time</h3>
          <div class="time-grid">
            @for (slot of timeSlots; track slot.value) {
              <button class="time-chip" [class.active]="selectedTime() === slot.value" (click)="selectedTime.set(slot.value)">
                {{ slot.label }} {{ slot.pm ? 'PM' : 'AM' }}
              </button>
            }
          </div>

          <h3 class="card-title">Select duration</h3>
          <div class="time-grid">
            @for (d of durations; track d.minutes) {
              <button class="time-chip" [class.active]="selectedDuration() === d.minutes" (click)="selectedDuration.set(d.minutes)">
                {{ d.label }}
              </button>
            }
          </div>
        </section>

        <section class="card summary-card">
          <div class="summary-head">
            <h3>Schedule Appointment</h3>
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20h9"/><path d="M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z"/></svg>
          </div>
          <div class="summary-box">
            <div class="row"><span>Date</span><b>{{ selectedDateLabel() }}</b></div>
            <div class="row"><span>Time</span><b>{{ timeLabel() }}</b></div>
            <div class="row"><span>Duration</span><b>{{ durationLabel() }}</b></div>
          </div>
        </section>

        @if (actionError()) { <p class="field-error">{{ actionError() }}</p> }
        <button class="btn btn-primary" [disabled]="sending()" (click)="sendProposal()">
          @if (sending()) { <app-spinner [size]="16" /> } @else { Send Proposal }
        </button>
      }

      @if (submitted()) {
        <div class="overlay">
          <div class="modal">
            <span class="modal-icon">
              <svg width="28" height="28" viewBox="0 0 24 24" fill="#2fbf71" stroke="none"><path d="m12 2 3.1 6.3 6.9 1-5 4.9 1.2 6.9-6.2-3.3-6.2 3.3L7 14.2 2 9.3l6.9-1z"/></svg>
            </span>
            <h3>Your Session has been submitted successfully</h3>
            <div class="summary-box">
              <div class="row"><span>Date</span><b>{{ selectedDateLabel() }}</b></div>
              <div class="row"><span>Time</span><b>{{ timeLabel() }}</b></div>
              <div class="row"><span>Duration</span><b>{{ durationLabel() }}</b></div>
            </div>
            <a class="btn btn-primary" [routerLink]="['/swaps', swapId()]">View My Proposal</a>
          </div>
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
    .card-title { font-size: 14px; font-weight: 700; margin: 0 0 10px; }

    .day-row { display: flex; gap: 8px; overflow-x: auto; padding-bottom: 4px; margin-bottom: 16px; scrollbar-width: none; }
    .day-row::-webkit-scrollbar { display: none; }
    .day-chip {
      min-width: 52px; display: flex; flex-direction: column; align-items: center; gap: 1px;
      border: 1.5px solid var(--border); background: #fff; border-radius: 12px; padding: 9px 10px;
      cursor: pointer; font-family: inherit;
    }
    .day-chip b { font-size: 15px; font-weight: 700; }
    .day-chip small { font-size: 10.5px; color: var(--text-muted); }
    .day-chip.active { background: var(--primary); border-color: var(--primary); color: #fff; }
    .day-chip.active small { color: rgba(255, 255, 255, 0.8); }

    .time-grid { display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 8px; margin-bottom: 16px; }
    .time-chip {
      border: 1.5px solid var(--border); background: #fff; border-radius: 12px;
      padding: 11px 6px; font-size: 12.5px; font-weight: 600; color: var(--text);
      cursor: pointer; font-family: inherit;
    }
    .time-chip.active { background: var(--primary); border-color: var(--primary); color: #fff; box-shadow: 0 4px 12px rgba(124, 92, 252, 0.3); }

    .summary-head { display: flex; align-items: center; justify-content: space-between; margin-bottom: 10px; }
    .summary-head h3 { font-size: 14.5px; font-weight: 700; margin: 0; color: var(--primary); }
    .summary-head svg { color: var(--text-muted); }

    .summary-box { background: #fff; border: 1.5px solid var(--primary); border-radius: var(--radius-md); padding: 4px 14px; }
    .row { display: flex; align-items: center; justify-content: space-between; gap: 10px; padding: 10px 0; font-size: 12.5px; border-bottom: 1px solid var(--border); }
    .row:last-child { border-bottom: none; }
    .row span { color: var(--text-secondary); }
    .row b { font-weight: 600; }

    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }

    @media (min-width: 900px) {
      .page-title { font-size: 26px; }
      .icon-btn:hover { background: var(--primary-light); color: var(--primary); }
      .card { max-width: 640px; }
      .day-row { flex-wrap: wrap; overflow-x: visible; scrollbar-width: auto; }
      .day-row::-webkit-scrollbar { display: block; }
      .day-chip { min-width: 60px; }
    }

    .overlay { position: fixed; inset: 0; z-index: 100; background: rgba(31, 31, 61, 0.45); display: flex; align-items: center; justify-content: center; padding: 24px; }
    .modal {
      width: 100%; max-width: 380px; background: #fff; border-radius: 24px; padding: 28px 22px;
      display: flex; flex-direction: column; gap: 12px; text-align: center; animation: pop 0.22s ease both;
    }
    @keyframes pop { from { transform: scale(0.95); opacity: 0; } to { transform: none; opacity: 1; } }
    .modal-icon { width: 64px; height: 64px; border-radius: 50%; background: var(--success-light); display: flex; align-items: center; justify-content: center; margin: 0 auto; }
    .modal h3 { font-size: 15.5px; font-weight: 700; margin: 0; }
  `,
})
export class ScheduleSessionComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly swapRequests = inject(SwapRequestsService);
  private readonly toast = inject(ToastService);

  protected readonly timeSlots = TIME_SLOTS;
  protected readonly durations = DURATIONS;

  protected readonly swapId = signal('');
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly sending = signal(false);
  protected readonly actionError = signal('');
  protected readonly submitted = signal(false);

  protected readonly days = signal<DayOption[]>([]);
  protected readonly selectedDay = signal('');
  protected readonly selectedTime = signal('18:00');
  protected readonly selectedDuration = signal(60);

  private swap: SwapRequestDetailsDto | null = null;

  protected readonly selectedDateLabel = computed(() => {
    const day = this.days().find((d) => d.iso === this.selectedDay());
    return day ? new Date(day.iso + 'T00:00:00').toLocaleDateString('en-US', {
      weekday: 'short', month: 'short', day: 'numeric',
    }) : '—';
  });

  protected readonly timeLabel = computed(() => {
    const slot = TIME_SLOTS.find((s) => s.value === this.selectedTime());
    return slot ? `${slot.label} ${slot.pm ? 'PM' : 'AM'} – ${this.endTimeLabel()}` : '—';
  });

  protected readonly durationLabel = computed(() => {
    const d = DURATIONS.find((x) => x.minutes === this.selectedDuration());
    return d ? d.label : '—';
  });

  async ngOnInit(): Promise<void> {
    this.swapId.set(this.route.snapshot.paramMap.get('id') ?? '');
    await this.reload();
  }

  protected async reload(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.swap = await firstValueFrom(this.swapRequests.details(this.swapId()));
      this.buildDays();
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected endTimeLabel(): string {
    const [h, m] = this.selectedTime().split(':').map(Number);
    const total = (h * 60 + m + this.selectedDuration()) % (24 * 60);
    const hh = Math.floor(total / 60);
    const mm = total % 60;
    const suffix = hh >= 12 ? 'PM' : 'AM';
    const h12 = hh % 12 === 0 ? 12 : hh % 12;
    return `${h12}:${String(mm).padStart(2, '0')} ${suffix}`;
  }

  protected async sendProposal(): Promise<void> {
    if (!this.swap || this.sending()) return;
    this.sending.set(true);
    this.actionError.set('');
    try {
      await firstValueFrom(
        this.swapRequests.createProposal(this.swapId(), {
          scheduledDate: this.selectedDay(),
          startTime: `${this.selectedTime()}:00`,
          endTime: `${this.endTimeIso()}:00`,
          durationMinutes: this.selectedDuration(),
        })
      );
      this.toast.success('Session proposal sent.');
      this.submitted.set(true);
    } catch (err) {
      const message = extractApiError(err).message;
      if (/insufficient|not enough/i.test(message)) {
        // Bubble up to the wallet alert pattern used across the app.
        this.actionError.set(message);
      } else {
        this.actionError.set(message);
      }
    } finally {
      this.sending.set(false);
    }
  }

  private endTimeIso(): string {
    const [h, m] = this.selectedTime().split(':').map(Number);
    const total = (h * 60 + m + this.selectedDuration()) % (24 * 60);
    return `${String(Math.floor(total / 60)).padStart(2, '0')}:${String(total % 60).padStart(2, '0')}`;
  }

  private buildDays(): void {
    const days: DayOption[] = [];
    const today = new Date();
    for (let i = 0; i < 7; i++) {
      const date = new Date(today);
      date.setDate(today.getDate() + i);
      const iso = date.toISOString().slice(0, 10);
      days.push({
        iso,
        weekday: date.toLocaleDateString('en-US', { weekday: 'short' }),
        dayNumber: String(date.getDate()),
      });
    }
    this.days.set(days);
    if (days.length) this.selectedDay.set(days[1]?.iso ?? days[0].iso);
  }
}
