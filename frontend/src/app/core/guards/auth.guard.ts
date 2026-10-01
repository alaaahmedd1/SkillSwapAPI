import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

/** Redirects anonymous users to sign-in (preserving where they wanted to go). */
export const requireAuthGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated()) {
    return true;
  }
  const target = '/' + route.url.map((s) => s.path).join('/');
  return router.createUrlTree(['/auth/sign-in'], {
    queryParams: target === '/' ? {} : { redirect: target },
  });
};

/** Redirects signed-in users away from guest-only pages (welcome/auth/onboarding). */
export const requireGuestGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(['/home']);
};
