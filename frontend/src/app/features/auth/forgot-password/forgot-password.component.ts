import { Component, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { IllustrationDialogComponent } from '../../../shared/components/illustration-dialog/illustration-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { AUTH_PAGE_STYLES } from '../auth-page.styles';

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

@Component({
  selector: 'app-forgot-password',
  imports: [RouterLink, IllustrationDialogComponent, LoadingSpinnerComponent],
  template: `
    <div class="app-frame auth-page page-enter">
      <header class="auth-header">
        <button type="button" class="back" (click)="goBack()" aria-label="Go back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
        </button>
      </header>

      <h1>Forgot Password</h1>
      <p class="sub">Enter your email and we'll send you a 6-digit code to reset your password.</p>

      <form (submit)="$event.preventDefault(); submit()" novalidate>
        <div class="field">
          <label for="email">Email</label>
          <div class="input-wrap">
            <input id="email" type="email" class="input" [class.input-invalid]="!!error()" [value]="email()" (input)="onInput($event)" placeholder="Enter your email" autocomplete="email" />
          </div>
          @if (error()) { <span class="field-error">{{ error() }}</span> }
        </div>

        <button class="btn btn-primary" [disabled]="submitting()">
          @if (submitting()) { <app-spinner [size]="20" /> } @else { Send Code }
        </button>
      </form>

      <p class="switch">Remembered it? <a routerLink="/auth/sign-in">Sign In</a></p>

      @if (showDialog()) {
        <app-illustration-dialog
          image="/assets/illustrations/email-check.png"
          title="Check Your Email"
          [text]="'If an account exists for ' + email() + ', a password reset code is on its way.'"
          buttonText="Continue"
          (closed)="onDialogClosed()" />
      }
    </div>
  `,
  styles: [AUTH_PAGE_STYLES],
})
export class ForgotPasswordComponent {
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  email = signal('');
  error = signal('');
  submitting = signal(false);
  showDialog = signal(false);

  onInput(event: Event): void {
    this.email.set((event.target as HTMLInputElement).value);
    this.error.set('');
  }

  goBack(): void {
    history.back();
  }

  async submit(): Promise<void> {
    if (this.submitting()) return;
    if (!this.email().trim()) {
      this.error.set('Email address is required.');
      return;
    }
    if (!EMAIL_RE.test(this.email().trim())) {
      this.error.set('A valid email address is required.');
      return;
    }

    this.submitting.set(true);
    try {
      await firstValueFrom(this.auth.forgotPassword(this.email().trim()));
      this.showDialog.set(true);
    } catch (err) {
      this.toast.error(err instanceof Error ? err.message : 'Something went wrong. Please try again.');
    } finally {
      this.submitting.set(false);
    }
  }

  onDialogClosed(): void {
    this.router.navigate(['/auth/verify-otp'], {
      queryParams: { email: this.email().trim(), purpose: 'reset' },
    });
  }
}
