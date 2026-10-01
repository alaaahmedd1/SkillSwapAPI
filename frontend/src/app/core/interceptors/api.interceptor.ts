import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, switchMap, throwError } from 'rxjs';
import { Observable, shareReplay } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';
import { ApiRequestError, extractApiError } from '../utils/api-error';
import { TokenResponse } from '../models/api.models';

// Deduplicates concurrent 401s into a single refresh round-trip.
let refreshInFlight: Observable<TokenResponse> | null = null;

function refresh(auth: AuthService): Observable<TokenResponse> {
  refreshInFlight ??= auth.refreshSession().pipe(
    shareReplay({ bufferSize: 1, refCount: false }),
    finalize(() => (refreshInFlight = null))
  );
  return refreshInFlight;
}

export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  // Only intercept calls to our API.
  if (!req.url.startsWith(environment.apiUrl)) {
    return next(req);
  }

  const auth = inject(AuthService);
  const router = inject(Router);

  const isAuthEndpoint = /\/api\/auth\//.test(req.url);
  const token = auth.getAccessToken();
  const authedReq =
    token && !isAuthEndpoint ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }) : req;

  return next(authedReq).pipe(
    catchError((rawErr: unknown) => {
      const err = rawErr instanceof HttpErrorResponse ? rawErr : null;

      // Access token expired (and this wasn't already a retry, and we have a session):
      // refresh once, then replay the original request.
      if (err?.status === 401 && !isAuthEndpoint && token && !req.headers.has('x-retried')) {
        return refresh(auth).pipe(
          switchMap((response) => {
            auth.applyRefreshedTokens(response);
            return next(req.clone({ setHeaders: { Authorization: `Bearer ${response.accessToken}`, 'x-retried': '1' } }));
          }),
          catchError((refreshErr: unknown) => {
            auth.forceLogout();
            return throwError(() => extractApiError(refreshErr));
          })
        );
      }

      if (err?.status === 401 && !isAuthEndpoint) {
        auth.forceLogout();
      }

      return throwError(() =>
        rawErr instanceof ApiRequestError ? rawErr : extractApiError(rawErr)
      );
    })
  );
};
