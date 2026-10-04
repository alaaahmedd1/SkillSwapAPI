import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';

interface OnboardingSlide {
  image: string;
  title: string;
  text: string;
}

const STORAGE_KEY = 'ss.onboarded';

@Component({
  selector: 'app-onboarding',
  standalone: true,
  template: `
    <div class="app-frame onboarding">
      <button class="skip" (click)="finish()">Skip</button>

      <div class="slides">
        <div class="slide">
          <img [src]="slides[index()].image" [alt]="slides[index()].title" class="art" />
          <div class="copy">
            <h1>{{ slides[index()].title }}</h1>
            <p>{{ slides[index()].text }}</p>
          </div>
        </div>
      </div>

      <div class="dots">
        @for (s of slides; track $index) {
          <button
            class="dot"
            [class.active]="$index === index()"
            [attr.aria-label]="'Go to slide ' + ($index + 1)"
            (click)="index.set($index)"></button>
        }
      </div>

      <div class="actions">
        @if (!isFirst()) {
          <button class="btn btn-outline" (click)="back()" aria-label="Previous slide">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="m15 18-6-6 6-6"/></svg>
          </button>
        }
        <button class="btn btn-primary next" (click)="next()">
          {{ isLast() ? 'Get Started' : 'Next' }}
        </button>
      </div>
    </div>
  `,
  styles: `
    :host { display: contents; }
    .onboarding { display: flex; flex-direction: column; padding: 20px 24px calc(24px + env(safe-area-inset-bottom)); background: var(--bg); }
    .skip { align-self: flex-end; border: none; background: none; padding: 8px 4px; font-size: 14px; font-weight: 600; color: var(--text-secondary); cursor: pointer; }
    .slides { flex: 1; display: flex; align-items: center; min-height: 0; }
    .slide { display: flex; flex-direction: column; align-items: center; text-align: center; width: 100%; }
    .art { width: 100%; max-width: 340px; aspect-ratio: 1; object-fit: cover; border-radius: var(--radius-lg); box-shadow: var(--shadow-card); }
    .copy { margin-top: 32px; }
    .copy h1 { font-size: 26px; font-weight: 700; line-height: 1.3; color: var(--text); }
    .copy p { font-size: 15px; line-height: 1.65; color: var(--text-secondary); margin: 12px auto 0; max-width: 320px; }
    .dots { display: flex; justify-content: center; gap: 8px; margin: 28px 0 24px; }
    .dot { width: 8px; height: 8px; border-radius: 50%; border: none; background: var(--border); padding: 0; cursor: pointer; transition: all .25s; }
    .dot.active { width: 26px; border-radius: 5px; background: var(--primary); }
    .actions { display: flex; gap: 12px; }
    .actions .next { flex: 1; }
    .actions .btn-outline { width: 56px; flex-shrink: 0; }

    @media (min-width: 768px) {
      .onboarding { max-width: 720px; margin: 0 auto; padding-top: 40px; }
      .art { max-width: 400px; }
      .copy h1 { font-size: 30px; }
      .copy p { max-width: 420px; font-size: 16px; }
      .actions { justify-content: center; }
      .actions .next { flex: 0 0 auto; padding: 14px 34px; }
    }
  `,
})
export class OnboardingComponent implements OnInit {
  private router = inject(Router);

  slides: OnboardingSlide[] = [
    {
      image: '/assets/illustrations/onboarding-skills.png',
      title: 'Exchange Skills, Earn Time',
      text: 'Teach what you know, learn what you love — every session you give banks time credits you can spend on lessons from others.',
    },
    {
      image: '/assets/illustrations/onboarding-hour.png',
      title: 'One Hour at a Time',
      text: 'Every skill swap is one focused hour. Schedule sessions around your life and track your teaching and learning hours.',
    },
    {
      image: '/assets/illustrations/onboarding-matching.png',
      title: 'Get Matched Instantly',
      text: 'Tell us what you offer and what you want to learn. We match you with the right people so you can start swapping today.',
    },
  ];

  index = signal(0);

  isFirst = computed(() => this.index() === 0);
  isLast = computed(() => this.index() === this.slides.length - 1);

  ngOnInit(): void {
    if (localStorage.getItem(STORAGE_KEY)) {
      this.router.navigateByUrl('/welcome', { replaceUrl: true });
    }
  }

  back(): void {
    this.index.update((i) => Math.max(0, i - 1));
  }

  next(): void {
    if (this.isLast()) {
      this.finish();
    } else {
      this.index.update((i) => i + 1);
    }
  }

  finish(): void {
    localStorage.setItem(STORAGE_KEY, '1');
    this.router.navigateByUrl('/welcome', { replaceUrl: true });
  }
}
