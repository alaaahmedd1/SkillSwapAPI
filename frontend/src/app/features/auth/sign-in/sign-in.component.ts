import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { AuthService } from '../../../core/services/auth.service';
import { SocialSignInService } from '../../../core/services/social-signin.service';
import { ToastService } from '../../../core/services/toast.service';
import { ApiRequestError } from '../../../core/utils/api-error';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner/loading-spinner.component';
import { AUTH_PAGE_STYLES } from '../auth-page.styles';

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
type FieldKey = 'email' | 'password';

const SERVER_FIELD_MAP: Record<string, FieldKey> = {
  Email: 'email',
  Password: 'password',
};

@Component({
  selector: 'app-sign-in',
  imports: [RouterLink, LoadingSpinnerComponent],
  template: `
    <div class="app-frame auth-page page-enter">
      <header class="auth-header">
        <button type="button" class="back" (click)="goBack()" aria-label="Go back">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round"><path d="m15 18-6-6 6-6"/></svg>
        </button>
      </header>

      <h1>Welcome Back</h1>
      <p class="sub">Sign in to continue swapping skills.</p>

      <form (submit)="$event.preventDefault(); submit()" novalidate>
        <div class="field">
          <label for="email">Email</label>
          <div class="input-wrap">
            <input id="email" type="email" class="input" [class.input-invalid]="hasError('email')" [value]="email()" (input)="setField('email', $event)" placeholder="Enter your email" autocomplete="email" />
          </div>
          @if (hasError('email')) { <span class="field-error">{{ firstError('email') }}</span> }
        </div>

        <div class="field">
          <label for="password">Password</label>
          <div class="input-wrap">
            <input id="password" [type]="showPassword() ? 'text' : 'password'" class="input has-trailing" [class.input-invalid]="hasError('password')" [value]="password()" (input)="setField('password', $event)" placeholder="Enter your password" autocomplete="current-password" />
            <button type="button" class="trailing-btn" (click)="showPassword.set(!showPassword())" [attr.aria-label]="showPassword() ? 'Hide password' : 'Show password'">
              @if (showPassword()) {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c6.5 0 10 8 10 8a13.16 13.16 0 0 1-1.67 2.68"/><path d="M6.61 6.61A13.5 13.5 0 0 0 2 12s3.5 8 10 8a9.74 9.74 0 0 0 5.39-1.61"/><path d="M9.88 9.88a3 3 0 1 0 4.24 4.24"/><path d="m2 2 20 20"/></svg>
              } @else {
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7-10-7-10-7z"/><circle cx="12" cy="12" r="3"/></svg>
              }
            </button>
          </div>
          @if (hasError('password')) { <span class="field-error">{{ firstError('password') }}</span> }
        </div>

        <div class="form-row">
          <label class="checkbox-row">
            <input type="checkbox" [checked]="rememberMe()" (change)="rememberMe.set(!rememberMe())" />
            Remember me
          </label>
          <a class="text-link" routerLink="/auth/forgot-password">Forgot Password?</a>
        </div>

        <button class="btn btn-primary" [disabled]="submitting()">
          @if (submitting()) { <app-spinner [size]="20" /> } @else { Sign In }
        </button>
      </form>

      <div class="divider">or continue with</div>

      <div class="social-row">
        <button type="button" class="social-btn" (click)="signInWithGoogle()" [disabled]="submitting()" aria-label="Continue with Google">
          <svg width="21" height="21" viewBox="0 0 24 24" aria-hidden="true"><path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92a5.06 5.06 0 0 1-2.2 3.32v2.77h3.57c2.08-1.92 3.27-4.74 3.27-8.1z"/><path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84A11 11 0 0 0 12 23z"/><path fill="#FBBC05" d="M5.84 14.09a6.6 6.6 0 0 1 0-4.18V7.07H2.18a11 11 0 0 0 0 9.86l3.66-2.84z"/><path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15A11 11 0 0 0 2.18 7.07l3.66 2.84C6.71 7.31 9.14 5.38 12 5.38z"/></svg>
          <span>Google</span>
        </button>
        <button type="button" class="social-btn" (click)="signInWithApple()" [disabled]="submitting()" aria-label="Continue with Apple">
          <svg width="21" height="21" viewBox="0 0 24 24" fill="#1f1f3d" aria-hidden="true"><path d="M17.05 20.28c-.98.95-2.05.8-3.08.35-1.09-.46-2.09-.48-3.24 0-1.44.62-2.2.44-3.06-.35C2.79 15.25 3.51 7.59 9.05 7.31c1.35.07 2.29.74 3.08.8.98-.2 1.92-.87 3.24-.83 1.58.13 2.77.75 3.55 1.9-3.27 1.96-2.5 6.27.53 7.5-.6 1.57-1.37 3.13-2.4 3.6zM12.03 7.25c-.15-2.23 1.66-4.07 3.74-4.25.29 2.58-2.34 4.5-3.74 4.25z"/></svg>
          <span>Apple</span>
        </button>
      </div>

      <p class="switch">Don't have an account? <a routerLink="/auth/sign-up">Sign Up</a></p>
    </div>
  `,
  styles: [AUTH_PAGE_STYLES],
})
export class SignInComponent {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);
  readonly socialSignIn = inject(SocialSignInService);

  email = signal(this.route.snapshot.queryParamMap.get('email') ?? '');
  password = signal('');
  rememberMe = signal(true);
  showPassword = signal(false);
  submitting = signal(false);
  errors = signal<Record<string, string[]>>({});

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

  socialNotConfigured(provider: 'Google' | 'Apple'): void {
    this.toast.info(`${provider} sign-in isn't configured in this environment.`);
  }

  async signInWithGoogle(): Promise<void> {
    if (!this.socialSignIn.googleConfigured) {
      this.socialNotConfigured('Google');
      return;
    }
    try {
      const idToken = await this.socialSignIn.signInWithGoogle();
      if (!idToken) return;
      await this.completeSocialLogin(idToken, 'Google');
    } catch (err) {
      this.toast.error(err instanceof Error ? err.message : 'Google sign-in failed. Please try again.');
    }
  }

  async signInWithApple(): Promise<void> {
    if (!this.socialSignIn.appleConfigured) {
      this.socialNotConfigured('Apple');
      return;
    }
    try {
      const idToken = await this.socialSignIn.signInWithApple();
      if (!idToken) return;
      await this.completeSocialLogin(idToken, 'Apple');
    } catch (err) {
      this.toast.error(err instanceof Error ? err.message : 'Apple sign-in failed. Please try again.');
    }
  }

  private async completeSocialLogin(idToken: string, provider: 'Google' | 'Apple'): Promise<void> {
    if (this.submitting()) return;
    this.submitting.set(true);
    try {
      await firstValueFrom(this.auth.socialLogin(idToken, provider));
      const redirect = this.route.snapshot.queryParamMap.get('redirect');
      await this.router.navigateByUrl(redirect && redirect.startsWith('/') ? redirect : '/home');
    } catch (err) {
      this.toast.error(err instanceof Error ? err.message : `${provider} sign-in failed. Please try again.`);
    } finally {
      this.submitting.set(false);
    }
  }

  async submit(): Promise<void> {
    if (this.submitting()) return;
    const errors: Record<string, string[]> = {};
    if (!this.email().trim()) errors['email'] = ['Email address is required.'];
    else if (!EMAIL_RE.test(this.email().trim())) errors['email'] = ['A valid email address is required.'];
    if (!this.password()) errors['password'] = ['Password is required.'];
    this.errors.set(errors);
    if (Object.keys(errors).length) return;

    this.submitting.set(true);
    try {
      await firstValueFrom(this.auth.login(this.email().trim(), this.password(), this.rememberMe()));
      const redirect = this.route.snapshot.queryParamMap.get('redirect');
      await this.router.navigateByUrl(redirect && redirect.startsWith('/') ? redirect : '/home');
    } catch (err) {
      if (err instanceof ApiRequestError && Object.keys(err.fieldErrors).length) {
        const merged: Record<string, string[]> = {};
        for (const [code, msgs] of Object.entries(err.fieldErrors)) {
          const key = SERVER_FIELD_MAP[code] ?? code;
          merged[key] = [...(merged[key] ?? []), ...msgs];
        }
        this.errors.set(merged);
      } else {
        this.toast.error(err instanceof Error ? err.message : 'Sign in failed. Please try again.');
      }
    } finally {
      this.submitting.set(false);
    }
  }
}
