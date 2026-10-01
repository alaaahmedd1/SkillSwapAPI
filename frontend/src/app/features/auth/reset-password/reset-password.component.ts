import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { ApiRequestError } from '../../../core/utils/api-error';
import { IllustrationDialogComponent } from '../../../shared/components/illustration-dialog/illustration-dialog.component';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { AUTH_PAGE_STYLES } from '../auth-page.styles';

type FieldKey = 'newPassword' | 'confirmPassword';

const SERVER_FIELD_MAP: Record<string, FieldKey> = {
  NewPassword: 'newPassword',
  ConfirmPassword: 'confirmPassword',
};

@Component({
  selector: 'app-reset-password',
  imports: [IllustrationDialogComponent, LoadingSpinnerComponent],
  template: `
    <div class="app-frame auth-page page-enter">
      <header class="auth-header">
        <button type="button" class="back" (click)="goBack()" aria-label="Go back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
        </button>
      </header>

      <h1>Reset Password</h1>
      <p class="sub">Create a new password for <b>{{ email() }}</b>.</p>

      <form (submit)="$event.preventDefault(); submit()" novalidate>
        <div class="field">
          <label for="newPassword">New Password</label>
          <div class="input-wrap">
            <input id="newPassword" [type]="showPassword() ? 'text' : 'password'" class="input has-trailing" [class.input-invalid]="hasError('newPassword')" [value]="newPassword()" (input)="setField('newPassword', $event)" placeholder="Create a new password" autocomplete="new-password" />
            <button type="button" class="trailing-btn" (click)="showPassword.set(!showPassword())" [attr.aria-label]="showPassword() ? 'Hide password' : 'Show password'">
              @if (showPassword()) {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c6.5 0 10 8 10 8a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.5 13.5 0 0 0 2 12s3.5 8 10 8a9.74 9.74 0 0 0 5.39-1.61"/><path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/><path d="m2 2 20 20"/></svg>
              } @else {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>
              }
            </button>
          </div>
          @if (hasError('newPassword')) { <span class="field-error">{{ firstError('newPassword') }}</span> }
          @if (newPassword()) {
            <ul class="rules">
              @for (rule of passwordRules(); track rule.label) {
                <li [class.ok]="rule.ok">
                  <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>
                  {{ rule.label }}
                </li>
              }
            </ul>
          }
        </div>

        <div class="field">
          <label for="confirmPassword">Confirm Password</label>
          <div class="input-wrap">
            <input id="confirmPassword" [type]="showConfirm() ? 'text' : 'password'" class="input has-trailing" [class.input-invalid]="hasError('confirmPassword')" [value]="confirmPassword()" (input)="setField('confirmPassword', $event)" placeholder="Confirm your new password" autocomplete="new-password" />
            <button type="button" class="trailing-btn" (click)="showConfirm.set(!showConfirm())" [attr.aria-label]="showConfirm() ? 'Hide password' : 'Show password'">
              @if (showConfirm()) {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c6.5 0 10 8 10 8a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.5 13.5 0 0 0 2 12s3.5 8 10 8a9.74 9.74 0 0 0 5.39-1.61"/><path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/><path d="m2 2 20 20"/></svg>
              } @else {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>
              }
            </button>
          </div>
          @if (hasError('confirmPassword')) { <span class="field-error">{{ firstError('confirmPassword') }}</span> }
        </div>

        <button class="btn btn-primary" [disabled]="submitting()">
          @if (submitting()) { <app-spinner [size]="20" /> } @else { Reset Password }
        </button>
      </form>

      @if (showDialog()) {
        <app-illustration-dialog
          image="/assets/illustrations/success-party.png"
          title="Password Reset!"
          text="Your password has been changed successfully. Sign in with your new password."
          buttonText="Sign In"
          (closed)="onDialogClosed()" />
      }
    </div>
  `,
  styles: [
    AUTH_PAGE_STYLES,
    `
      .sub b { color: var(--text); font-weight: 600; word-break: break-all; }
      .rules { list-style: none; margin: 10px 0 0; padding: 0; display: grid; grid-template-columns: 1fr 1fr; gap: 6px 10px; }
      .rules li { display: flex; align-items: center; gap: 6px; font-size: 11.5px; color: var(--text-muted); }
      .rules li svg { opacity: 0.4; flex-shrink: 0; }
      .rules li.ok { color: var(--success); }
      .rules li.ok svg { opacity: 1; }
    `,
  ],
})
export class ResetPasswordComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  email = signal('');
  otp = signal('');
  newPassword = signal('');
  confirmPassword = signal('');
  showPassword = signal(false);
  showConfirm = signal(false);
  submitting = signal(false);
  showDialog = signal(false);
  errors = signal<Record<string, string[]>>({});

  passwordRules = computed(() => {
    const p = this.newPassword();
    return [
      { label: 'At least 8 characters', ok: p.length >= 8 },
      { label: 'One uppercase letter', ok: /[A-Z]/.test(p) },
      { label: 'One number', ok: /[0-9]/.test(p) },
      { label: 'One special character', ok: /[^a-zA-Z0-9]/.test(p) },
    ];
  });

  constructor() {
    const params = this.route.snapshot.queryParamMap;
    const email = params.get('email');
    const otp = params.get('otp');
    if (!email || !otp) {
      this.router.navigateByUrl('/welcome', { replaceUrl: true });
      return;
    }
    this.email.set(email);
    this.otp.set(otp);
  }

  setField(key: FieldKey, event: Event): void {
    this[key].set((event.target as HTMLInputElement).value);
    this.errors.update((e) => {
      if (!(key in e)) return e;
      const next = { ...e };
      delete next[key];
      return next;
    });
  }

  hasError(key: FieldKey): boolean {
    return !!this.errors()[key]?.length;
  }

  firstError(key: FieldKey): string {
    return this.errors()[key]?.[0] ?? '';
  }

  goBack(): void {
    history.back();
  }

  async submit(): Promise<void> {
    if (this.submitting()) return;
    const errors: Record<string, string[]> = {};
    if (this.passwordRules().some((r) => !r.ok)) errors['newPassword'] = ['Password does not meet all requirements.'];
    if (!this.confirmPassword()) errors['confirmPassword'] = ['Please confirm your password.'];
    else if (this.confirmPassword() !== this.newPassword()) errors['confirmPassword'] = ['Passwords do not match.'];
    this.errors.set(errors);
    if (Object.keys(errors).length) return;

    this.submitting.set(true);
    try {
      await firstValueFrom(
        this.auth.resetPassword(this.email(), this.otp(), this.newPassword(), this.confirmPassword())
      );
      this.showDialog.set(true);
    } catch (err) {
      if (err instanceof ApiRequestError && Object.keys(err.fieldErrors).length) {
        const merged: Record<string, string[]> = {};
        for (const [code, msgs] of Object.entries(err.fieldErrors)) {
          const key = SERVER_FIELD_MAP[code] ?? code;
          merged[key] = [...(merged[key] ?? []), ...msgs];
        }
        this.errors.set(merged);
      } else {
        this.toast.error(err instanceof Error ? err.message : 'Something went wrong. Please try again.');
      }
    } finally {
      this.submitting.set(false);
    }
  }

  onDialogClosed(): void {
    this.router.navigate(['/auth/sign-in'], { queryParams: { email: this.email() } });
  }
}
