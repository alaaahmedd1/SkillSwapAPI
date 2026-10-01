import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, catchError, tap, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginResponse, RegisterRequest, TokenResponse, UserDto } from '../models/api.models';

interface StoredSession {
  user: UserDto;
  accessToken: string;
  refreshToken: string;
  expiresOnUtc: string;
  rememberMe: boolean;
}

const SESSION_KEY = 'skillswap.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly baseUrl = environment.apiUrl;

  private readonly _user = signal<UserDto | null>(this.readSession()?.user ?? null);
  readonly user = this._user.asReadonly();
  readonly isAuthenticated = computed(() => this._user() !== null);

  // ── Token access (used by the HTTP interceptor) ──────────────────────
  getAccessToken(): string | null {
    return this.readSession()?.accessToken ?? null;
  }

  getRefreshToken(): string | null {
    return this.readSession()?.refreshToken ?? null;
  }

  // ── API calls ─────────────────────────────────────────────────────────
  register(request: RegisterRequest): Observable<UserDto> {
    return this.http.post<UserDto>(`${this.baseUrl}/api/auth/register`, request);
  }

  login(email: string, password: string, rememberMe: boolean): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/api/auth/login`, { email, password })
      .pipe(tap((response) => this.persistSession(response, rememberMe)));
  }

  socialLogin(idToken: string, provider: 'Google' | 'Facebook' | 'Apple'): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.baseUrl}/api/auth/social-login`, { idToken, provider })
      .pipe(tap((response) => this.persistSession(response, true)));
  }

  refreshSession(): Observable<TokenResponse> {
    const session = this.readSession();
    if (!session) {
      return throwError(() => new Error('No session to refresh.'));
    }
    return this.http
      .post<TokenResponse>(`${this.baseUrl}/api/auth/refresh-token`, {
        refreshToken: session.refreshToken,
        expiredAccessToken: session.accessToken,
      })
      .pipe(
        tap((response) => this.persistSession({ ...response, user: session.user }, session.rememberMe)),
        catchError((err) => {
          this.clearSession();
          return throwError(() => err);
        })
      );
  }

  verifyOtp(email: string, otp: string): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/api/auth/verify-otp`, { email, otp });
  }

  resendOtp(email: string): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/api/auth/resend-otp`, { email });
  }

  forgotPassword(email: string): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/api/auth/forgot-password`, { email });
  }

  resetPassword(email: string, otp: string, newPassword: string, confirmPassword: string): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/api/auth/reset-password`, { email, otp, newPassword, confirmPassword });
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();
    this.clearSession();
    if (refreshToken) {
      // Best effort — revoke the refresh token server side.
      this.http.post(`${this.baseUrl}/api/auth/logout`, { refreshToken }).subscribe();
    }
    void this.router.navigateByUrl('/welcome');
  }

  /** Called by the interceptor when a refresh cycle fails. */
  forceLogout(): void {
    this.clearSession();
    if (this.router.url !== '/auth/sign-in') {
      void this.router.navigateByUrl('/auth/sign-in');
    }
  }

  // ── Session persistence ───────────────────────────────────────────────
  private persistSession(response: TokenResponse & { user: UserDto }, rememberMe: boolean): void {
    const session: StoredSession = {
      user: response.user,
      accessToken: response.accessToken,
      refreshToken: response.refreshToken,
      expiresOnUtc: response.expiresOnUtc,
      rememberMe,
    };
    this.writeSession(session);
    this._user.set(response.user);
  }

  applyRefreshedTokens(response: TokenResponse): void {
    const session = this.readSession();
    if (!session) return;
    this.writeSession({ ...session, ...response });
  }

  /** Updates the display name after a profile rename. */
  applyNameUpdate(firstName: string, lastName: string): void {
    const user = this._user();
    if (!user) return;
    const updated = { ...user, firstName, lastName };
    this._user.set(updated);
    const session = this.readSession();
    if (session) this.writeSession({ ...session, user: updated });
  }

  private readSession(): StoredSession | null {
    for (const storage of [localStorage, sessionStorage]) {
      const raw = storage.getItem(SESSION_KEY);
      if (!raw) continue;
      try {
        return JSON.parse(raw) as StoredSession;
      } catch {
        storage.removeItem(SESSION_KEY);
      }
    }
    return null;
  }

  private writeSession(session: StoredSession): void {
    this.clearSession();
    const storage = session.rememberMe ? localStorage : sessionStorage;
    storage.setItem(SESSION_KEY, JSON.stringify(session));
  }

  private clearSession(): void {
    this._user.set(null);
    localStorage.removeItem(SESSION_KEY);
    sessionStorage.removeItem(SESSION_KEY);
  }
}
