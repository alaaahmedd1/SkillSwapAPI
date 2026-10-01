import { Component, computed, inject, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterLink, RouterLinkActive],
  template: `
    <div class="app-frame shell">
      <header class="topbar">
        <a class="brand" routerLink="/home" aria-label="SkillSwap home">
          <span class="brand-mark">&infin;</span>
          <span class="brand-name">Skill<b>Swap</b></span>
        </a>
        <div class="topbar-actions">
          <ng-content select="[topbar-actions]" />
          @if (!isAuthenticated()) {
            <a class="signin-pill" routerLink="/auth/sign-in">Sign in</a>
          } @else {
            <span class="avatar" [title]="userName()">{{ initials() }}</span>
          }
        </div>
      </header>

      <main class="shell-content page-enter">
        <ng-content />
      </main>

      <nav class="bottom-nav" aria-label="Primary">
        @for (item of navItems; track item.route) {
          <a
            [routerLink]="item.route"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{ exact: item.exact }"
            class="nav-item"
            [attr.aria-label]="item.label"
          >
            <span class="nav-icon" [innerHTML]="item.icon"></span>
            <span class="nav-label">{{ item.label }}</span>
          </a>
        }
      </nav>
    </div>
  `,
  styles: `
    .shell {
      display: flex;
      flex-direction: column;
      padding-bottom: calc(var(--nav-height) + env(safe-area-inset-bottom, 0px));
    }

    .topbar {
      position: sticky;
      top: 0;
      z-index: 50;
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 14px 20px;
      background: rgba(251, 251, 254, 0.88);
      backdrop-filter: blur(12px);
      border-bottom: 1px solid var(--border);
    }

    .brand {
      display: inline-flex;
      align-items: center;
      gap: 7px;
      text-decoration: none;
      color: var(--text);
      font-size: 18px;

      .brand-mark {
        font-size: 30px;
        font-weight: 800;
        line-height: 1;
        background: var(--gradient);
        -webkit-background-clip: text;
        background-clip: text;
        color: transparent;
        transform: translateY(-2px);
      }

      .brand-name {
        font-weight: 500;

        b {
          font-weight: 700;
        }
      }
    }

    .topbar-actions {
      display: flex;
      align-items: center;
      gap: 10px;
    }

    .signin-pill {
      font-size: 13px;
      font-weight: 600;
      color: var(--primary);
      background: var(--primary-light);
      padding: 8px 16px;
      border-radius: 999px;
    }

    .avatar {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: var(--gradient);
      color: #fff;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      font-size: 13px;
      font-weight: 700;
    }

    .shell-content {
      flex: 1;
      padding: 18px 20px 28px;
    }

    .bottom-nav {
      position: fixed;
      bottom: 0;
      left: 50%;
      transform: translateX(-50%);
      width: 100%;
      max-width: 480px;
      height: calc(var(--nav-height) + env(safe-area-inset-bottom, 0px));
      display: flex;
      background: #fff;
      border-top: 1px solid var(--border);
      z-index: 50;
      padding-bottom: env(safe-area-inset-bottom, 0px);

      .nav-item {
        flex: 1;
        display: flex;
        flex-direction: column;
        align-items: center;
        justify-content: center;
        gap: 3px;
        color: var(--text-muted);
        text-decoration: none;
        transition: color 0.15s ease;

        .nav-icon {
          display: inline-flex;

          ::ng-deep svg {
            width: 23px;
            height: 23px;
          }
        }

        .nav-label {
          font-size: 10.5px;
          font-weight: 500;
        }

        &.active {
          color: var(--primary);

          .nav-icon {
            transform: translateY(-1px);
          }
        }
      }
    }
  `,
})
export class AppShellComponent {
  private readonly auth = inject(AuthService);

  protected readonly isAuthenticated = this.auth.isAuthenticated;
  protected readonly userName = computed(() => {
    const user = this.auth.user();
    return user ? `${user.firstName} ${user.lastName}` : '';
  });
  protected readonly initials = computed(() => {
    const user = this.auth.user();
    return user ? `${user.firstName[0] ?? ''}${user.lastName[0] ?? ''}`.toUpperCase() : '';
  });

  protected readonly navItems = [
    {
      route: '/wallet',
      label: 'Wallet',
      exact: true,
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M20 7H5a2 2 0 0 1-2-2 2 2 0 0 1 2-2h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15a1 1 0 0 0 1-1V8a1 1 0 0 0-1-1"/><path d="M16 13.5h.01"/></svg>`,
    },
    {
      route: '/explore',
      label: 'Explore',
      exact: false,
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="m16.2 7.8-2 6.3-6.4 2.1 2-6.3z"/></svg>`,
    },
    {
      route: '/messages',
      label: 'Messages',
      exact: false,
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>`,
    },
    {
      route: '/profile',
      label: 'Profile',
      exact: false,
      icon: `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="8" r="4"/><path d="M4 21c0-4 3.6-6.5 8-6.5s8 2.5 8 6.5"/></svg>`,
    },
  ];
}
