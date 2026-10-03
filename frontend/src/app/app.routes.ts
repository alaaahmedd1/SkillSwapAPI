import { Routes } from '@angular/router';
import { requireAuthGuard, requireGuestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'onboarding' },
  {
    path: 'onboarding',
    loadComponent: () =>
      import('./features/onboarding/onboarding.component').then((m) => m.OnboardingComponent),
  },
  {
    path: 'welcome',
    canActivate: [requireGuestGuard],
    loadComponent: () =>
      import('./features/welcome/welcome.component').then((m) => m.WelcomeComponent),
  },
  {
    path: 'auth/sign-up',
    canActivate: [requireGuestGuard],
    loadComponent: () =>
      import('./features/auth/sign-up/sign-up.component').then((m) => m.SignUpComponent),
  },
  {
    path: 'auth/sign-in',
    canActivate: [requireGuestGuard],
    loadComponent: () =>
      import('./features/auth/sign-in/sign-in.component').then((m) => m.SignInComponent),
  },
  {
    path: 'auth/forgot-password',
    canActivate: [requireGuestGuard],
    loadComponent: () =>
      import('./features/auth/forgot-password/forgot-password.component').then(
        (m) => m.ForgotPasswordComponent
      ),
  },
  {
    path: 'auth/verify-otp',
    canActivate: [requireGuestGuard],
    loadComponent: () =>
      import('./features/auth/verify-otp/verify-otp.component').then((m) => m.VerifyOtpComponent),
  },
  {
    path: 'auth/reset-password',
    canActivate: [requireGuestGuard],
    loadComponent: () =>
      import('./features/auth/reset-password/reset-password.component').then(
        (m) => m.ResetPasswordComponent
      ),
  },
  {
    path: 'swaps/:id/schedule',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/swaps/schedule-session.component').then(
        (m) => m.ScheduleSessionComponent
      ),
  },
  {
    path: 'swaps/:id/review',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/swaps/review-swap.component').then((m) => m.ReviewSwapComponent),
  },
  {
    path: 'wallet',
    canActivate: [requireAuthGuard],
    loadComponent: () => import('./features/wallet/wallet.component').then((m) => m.WalletComponent),
  },
  {
    path: 'wallet/transactions',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/wallet/transaction-history.component').then(
        (m) => m.TransactionHistoryComponent
      ),
  },
  {
    path: 'wallet/transactions/:id',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/wallet/transaction-details.component').then(
        (m) => m.TransactionDetailsComponent
      ),
  },
  {
    path: 'home',
    loadComponent: () => import('./features/home/home.component').then((m) => m.HomeComponent),
  },
  {
    path: 'explore',
    loadComponent: () =>
      import('./features/explore/explore.component').then((m) => m.ExploreComponent),
  },
  {
    path: 'messages',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/messages/messages.component').then((m) => m.MessagesComponent),
    children: [
      {
        path: ':swapId',
        loadComponent: () =>
          import('./features/messages/chat-room.component').then((m) => m.ChatRoomComponent),
      },
    ],
  },
  {
    path: 'swaps',
    canActivate: [requireAuthGuard],
    loadComponent: () => import('./features/swaps/swaps.component').then((m) => m.SwapsComponent),
  },
  {
    path: 'swaps/:id',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/swaps/swap-details.component').then((m) => m.SwapDetailsComponent),
  },
  {
    path: 'sessions/:swapId',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/sessions/live-session.component').then((m) => m.LiveSessionComponent),
  },
  {
    path: 'users/:id',
    loadComponent: () =>
      import('./features/users/user-profile.component').then((m) => m.UserProfileComponent),
  },
  {
    path: 'profile',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/profile/profile.component').then((m) => m.ProfileComponent),
  },
  {
    path: 'profile/edit',
    canActivate: [requireAuthGuard],
    loadComponent: () =>
      import('./features/profile/edit-profile.component').then((m) => m.EditProfileComponent),
  },
  { path: '**', redirectTo: 'home' },
];
