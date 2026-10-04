import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-welcome',
  standalone: true,
  imports: [RouterLink],
  template: `
    <div class="app-frame welcome">
      <div class="hero">
        <img src="/assets/illustrations/welcome-hero.png" alt="People exchanging skills" />
      </div>

      <div class="copy">
        <h1>Welcome To <span>Skill Swap</span></h1>
        <p>Exchange skills, learn together, and grow — one hour at a time.</p>
      </div>

      <div class="actions">
        <a class="btn btn-primary" routerLink="/auth/sign-up">Sign Up</a>
        <a class="btn btn-outline" routerLink="/auth/sign-in">Sign In</a>
        <a class="text-link guest" routerLink="/home">Continue as guest</a>
      </div>
    </div>
  `,
  styles: `
    :host { display: contents; }
    .welcome { display: flex; flex-direction: column; justify-content: space-between; gap: 24px; padding: 48px 24px calc(32px + env(safe-area-inset-bottom)); background: var(--bg); }
    .hero { display: flex; justify-content: center; padding-top: 12px; }
    .hero img { width: 100%; max-width: 340px; aspect-ratio: 1; object-fit: cover; border-radius: var(--radius-lg); box-shadow: var(--shadow-card); }
    .copy { text-align: center; }
    .copy h1 { font-size: 28px; font-weight: 700; line-height: 1.3; color: var(--text); }
    .copy h1 span { background: var(--gradient); -webkit-background-clip: text; background-clip: text; color: transparent; }
    .copy p { font-size: 15px; line-height: 1.65; color: var(--text-secondary); margin: 12px auto 0; max-width: 300px; }
    .actions { display: flex; flex-direction: column; gap: 14px; }
    .guest { align-self: center; font-size: 14px; }

    @media (min-width: 768px) {
      .welcome { align-items: center; justify-content: center; padding: 64px 24px; }
      .hero img { max-width: 400px; }
      .copy h1 { font-size: 34px; }
      .copy p { max-width: 380px; font-size: 16px; }
      .actions { width: 100%; max-width: 380px; }
    }
  `,
})
export class WelcomeComponent {}
