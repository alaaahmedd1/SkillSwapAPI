import { Component, inject, OnDestroy, OnInit, signal, viewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { ApiRequestError } from '../../../core/utils/api-error';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { OtpInputComponent } from '../../../shared/components/otp-input/otp-input.component';
import { AUTH_PAGE_STYLES } from '../auth-page.styles';

const RESEND_SECONDS = 60;

@Component({
  selector: 'app-verify-otp',
  imports: [OtpInputComponent, LoadingSpinnerComponent],
  template: `
    <div class="app-frame auth-page page-enter">
      <header class="auth-header">
        <button type="button" class="back" (click)="goBack()" aria-label="Go back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
        </button>
      </header>

      <h1>Verification Code</h1>
      <p class="sub">
        Enter the 6-digit code we sent to <b>{{ email() }}</b>.
        @if (purpose() === 'register') { It activates your account. } @else { It lets you reset your password. }
      </p>

      <form (submit)="$event.preventDefault(); submit()" novalidate>
        <app-otp-input (valueChange)="onOtpChange($event)" />

        <div class="resend-row">
          <span>Didn't receive the code?</span>
          @if (resendIn() > 0) {
            <span class="countdown">Resend in {{ resendIn() }}s</span>
          } @else {
            <button type="button" class="text-link" [disabled]="resending()" (click)="resend()">
              {{ resending() ? 'Sending…' : 'Resend Code' }}
            </button>
          }
        </div>

        <button class="btn btn-primary" [disabled]="submitting()">
          @if (submitting()) { <app-spinner [size]="20" /> } @else { Continue }
        </button>
      </form>
    </div>
  `,
  styles: [
    AUTH_PAGE_STYLES,
    `
      form { display: flex; flex-direction: column; }
      .sub b { color: var(--text); font-weight: 600; word-break: break-all; }
      .resend-row { display: flex; align-items: center; justify-content: center; gap: 6px; margin: 26px 0 20px; font-size: 13px; color: var(--text-secondary); }
      .countdown { color: var(--text-muted); font-weight: 500; }
      .text-link:disabled { opacity: 0.6; cursor: default; }
    `,
  ],
})
export class VerifyOtpComponent implements OnInit, OnDestroy {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  private readonly otpInput = viewChild(OtpInputComponent);
  private timer: ReturnType<typeof setInterval> | null = null;

  email = signal('');
  purpose = signal<'register' | 'reset'>('register');
  otp = signal('');
  resendIn = signal(0);
  resending = signal(false);
  submitting = signal(false);

  ngOnInit(): void {
    const params = this.route.snapshot.queryParamMap;
    const email = params.get('email');
    if (!email) {
      this.router.navigateByUrl('/welcome', { replaceUrl: true });
      return;
    }
    this.email.set(email);
    this.purpose.set(params.get('purpose') === 'reset' ? 'reset' : 'register');
    this.startTimer();
  }

  ngOnDestroy(): void {
    this.stopTimer();
  }

  onOtpChange(value: string): void {
    this.otp.set(value);
  }

  goBack(): void {
    history.back();
  }

  async submit(): Promise<void> {
    if (this.submitting()) return;
    if (this.otp().length !== 6) {
      this.toast.error('Please enter the full 6-digit code.');
      return;
    }

    this.submitting.set(true);
    try {
      if (this.purpose() === 'register') {
        await firstValueFrom(this.auth.verifyOtp(this.email(), this.otp()));
        this.toast.success('Email verified! You can sign in now.');
        await this.router.navigate(['/auth/sign-in'], { queryParams: { email: this.email() } });
      } else {
        await this.router.navigate(['/auth/reset-password'], {
          queryParams: { email: this.email(), otp: this.otp() },
        });
      }
    } catch (err) {
      this.otpInput()?.clear();
      if (err instanceof ApiRequestError && err.fieldErrors['Otp']?.length) {
        this.toast.error(err.fieldErrors['Otp'][0]);
      } else {
        this.toast.error(err instanceof Error ? err.message : 'Verification failed. Please try again.');
      }
    } finally {
      this.submitting.set(false);
    }
  }

  async resend(): Promise<void> {
    if (this.resending() || this.resendIn() > 0) return;
    this.resending.set(true);
    try {
      await firstValueFrom(this.auth.resendOtp(this.email()));
      this.toast.success('A new code is on its way.');
      this.otpInput()?.clear();
      this.startTimer();
    } catch (err) {
      this.toast.error(err instanceof Error ? err.message : 'Could not resend the code.');
    } finally {
      this.resending.set(false);
    }
  }

  private startTimer(): void {
    this.stopTimer();
    this.resendIn.set(RESEND_SECONDS);
    this.timer = setInterval(() => {
      this.resendIn.update((s) => {
        if (s <= 1) {
          this.stopTimer();
          return 0;
        }
        return s - 1;
      });
    }, 1000);
  }

  private stopTimer(): void {
    if (this.timer) {
      clearInterval(this.timer);
      this.timer = null;
    }
  }
}
