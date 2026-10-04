import { DecimalPipe } from '@angular/common';
import { Component, ElementRef, EventEmitter, OnInit, Output, ViewChild, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { firstValueFrom } from 'rxjs';
import { loadStripe, Stripe, StripeElements, StripePaymentElement } from '@stripe/stripe-js';

import {
  CheckoutResponseDto,
  CreditPackageDto,
} from '../../core/models/domain.models';
import { WalletService } from '../../core/services/wallet.service';
import { extractApiError } from '../../core/utils/api-error';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

type Step = 'packages' | 'payment' | 'success' | 'failed';

@Component({
  selector: 'app-top-up-dialog',
  imports: [DecimalPipe, FormsModule, LoadingSpinnerComponent],
  template: `
    <div class="overlay" (click)="close()">
      <div class="sheet" (click)="$event.stopPropagation()">
        <span class="grabber"></span>

        @switch (step()) {
          @case ('packages') {
            <h3>Top-Up Your Time Credits</h3>
            <p class="sub">Learn more skills without waiting to earn hours.</p>

            @if (loading()) {
              <div class="state-box"><app-spinner [size]="26" /></div>
            } @else if (error()) {
              <p class="field-error">{{ error() }}</p>
            } @else {
              <div class="packages">
                @for (pkg of packages(); track pkg.id) {
                  <button
                    class="pkg"
                    [class.selected]="pkg.id === selectedId()"
                    (click)="selectedId.set(pkg.id)"
                  >
                    <div class="pkg-main">
                      <b>{{ pkg.name }}</b>
                      <small>{{ pkg.description }}</small>
                    </div>
                    <b class="pkg-price">{{ pkg.price | number: '1.2-2' }} {{ pkg.currency }}</b>
                  </button>
                }
              </div>
              <button class="btn btn-primary" [disabled]="!selectedId() || starting()" (click)="startCheckout()">
                @if (starting()) { <app-spinner [size]="16" /> } @else { Continue to Payment }
              </button>
              <p class="secure">
                <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
                Secured with Stripe · cards, Apple Pay &amp; Google Pay
              </p>
            }
          }

          @case ('payment') {
            <h3>Pay {{ checkout()?.amount | number: '1.2-2' }} {{ checkout()?.currency }}</h3>
            <p class="sub">Complete your purchase securely with Stripe.</p>
            <div #paymentElement class="payment-element"></div>
            @if (payError()) { <p class="field-error">{{ payError() }}</p> }
            <button class="btn btn-primary" [disabled]="paying()" (click)="confirmPayment()">
              @if (paying()) { <app-spinner [size]="16" /> } @else { Pay Now }
            </button>
            <button class="btn btn-soft" [disabled]="paying()" (click)="backToPackages()">Cancel</button>
          }

          @case ('success') {
            <div class="result">
              <span class="result-icon success">
                <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6 9 17l-5-5"/></svg>
              </span>
              <h3>Payment Successful</h3>
              <p class="sub">Your account has been topped-up. Your wallet balance will update shortly.</p>
              <div class="success-box">
                <div class="row"><span>Amount Paid</span><b>{{ checkout()?.amount | number: '1.2-2' }} {{ checkout()?.currency }}</b></div>
                <div class="row"><span>Order ID</span><b class="mono small">{{ checkout()?.orderId }}</b></div>
              </div>
              <button class="btn btn-primary" (click)="close()">Back to Wallet</button>
            </div>
          }

          @case ('failed') {
            <div class="result">
              <span class="result-icon failed">
                <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round" stroke-linejoin="round"><path d="M18 6 6 18M6 6l12 12"/></svg>
              </span>
              <h3>Payment Failed</h3>
              <p class="sub">We couldn't process your transaction. Your wallet balance has not been charged.</p>
              @if (payError()) { <p class="field-error">{{ payError() }}</p> }
              <button class="btn btn-primary" (click)="step.set('payment')">Try Again</button>
              <button class="btn btn-soft" (click)="step.set('packages')">Change Package</button>
            </div>
          }
        }
      </div>
    </div>
  `,
  styles: `
    :host { position: fixed; inset: 0; z-index: 100; }
    .overlay {
      position: absolute; inset: 0; background: rgba(31, 31, 61, 0.45);
      display: flex; align-items: flex-end; justify-content: center;
    }
    .sheet {
      width: 100%; max-width: 480px; background: #fff;
      border-radius: 24px 24px 0 0; padding: 12px 20px calc(20px + env(safe-area-inset-bottom, 0px));
      max-height: 88dvh; overflow-y: auto; text-align: center;
      animation: sheetUp 0.28s ease both;
    }
    @keyframes sheetUp { from { transform: translateY(40px); opacity: 0; } to { transform: none; opacity: 1; } }
    .grabber { display: block; width: 44px; height: 5px; border-radius: 999px; background: var(--border); margin: 0 auto 14px; }

    @media (min-width: 640px) {
      .overlay { align-items: center; padding: 24px; }
      .sheet {
        max-width: 460px; border-radius: var(--radius-lg);
        padding: 22px 24px 24px;
        box-shadow: 0 24px 64px rgba(0, 0, 0, 0.22);
        animation: pop-in 0.22s ease both;
      }
      .grabber { display: none; }
      @keyframes pop-in { from { transform: scale(0.96); opacity: 0; } to { transform: none; opacity: 1; } }
    }

    h3 { font-size: 17px; font-weight: 700; margin: 0; }
    .sub { font-size: 12.5px; color: var(--text-secondary); margin: 6px 0 16px; }

    .packages { display: flex; flex-direction: column; gap: 10px; margin-bottom: 16px; text-align: left; }
    .pkg {
      display: flex; align-items: center; justify-content: space-between; gap: 12px;
      border: 1.5px solid var(--border); background: #fff; border-radius: var(--radius-md);
      padding: 14px 16px; cursor: pointer; text-align: left; font-family: inherit;
      transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }
    .pkg.selected { border-color: var(--primary); box-shadow: 0 0 0 3px var(--primary-light); }
    .pkg-main { display: flex; flex-direction: column; gap: 2px; }
    .pkg-main b { font-size: 14px; }
    .pkg-main small { font-size: 11.5px; color: var(--text-secondary); }
    .pkg-price { font-size: 14.5px; font-weight: 700; color: var(--primary); }

    .payment-element { min-height: 90px; margin-bottom: 14px; text-align: left; }
    .btn + .btn { margin-top: 10px; }

    .secure {
      display: flex; align-items: center; justify-content: center; gap: 5px;
      font-size: 11px; color: var(--text-muted); margin: 12px 0 0;
    }

    .result { display: flex; flex-direction: column; align-items: center; gap: 6px; padding: 10px 0 4px; }
    .result-icon {
      width: 72px; height: 72px; border-radius: 50%;
      display: flex; align-items: center; justify-content: center; margin-bottom: 8px;
    }
    .result-icon.success { background: var(--success-light); color: var(--success); }
    .result-icon.failed { background: var(--danger-light); color: var(--danger); }

    .success-box {
      width: 100%; background: var(--primary-soft); border-radius: var(--radius-md);
      padding: 4px 14px; margin: 10px 0 14px;
    }
    .row { display: flex; align-items: center; justify-content: space-between; gap: 10px; padding: 10px 0; font-size: 12.5px; border-bottom: 1px solid var(--border); }
    .row:last-child { border-bottom: none; }
    .row span { color: var(--text-secondary); }
    .row b { font-weight: 600; }
    .mono { font-family: ui-monospace, monospace; }
    .mono.small { font-size: 10px; word-break: break-all; text-align: right; }

    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; padding: 30px 16px; color: var(--text-secondary); font-size: 13.5px; }
  `,
})
export class TopUpDialogComponent implements OnInit {
  private readonly wallet = inject(WalletService);

  readonly requiredMinutes = input(0);

  @Output() closed = new EventEmitter<void>();

  @ViewChild('paymentElement') paymentElementHost?: ElementRef<HTMLElement>;

  protected readonly packages = signal<CreditPackageDto[]>([]);
  protected readonly selectedId = signal('');
  protected readonly loading = signal(true);
  protected readonly starting = signal(false);
  protected readonly paying = signal(false);
  protected readonly error = signal('');
  protected readonly payError = signal('');
  protected readonly step = signal<Step>('packages');
  protected readonly checkout = signal<CheckoutResponseDto | null>(null);

  private stripe: Stripe | null = null;
  private elements: StripeElements | null = null;
  private paymentElement: StripePaymentElement | null = null;

  async ngOnInit(): Promise<void> {
    try {
      const [packages] = await Promise.all([firstValueFrom(this.wallet.getCreditPackages())]);
      this.packages.set(packages.filter((p) => p.creditsCount > 0));
      if (packages.length) this.selectedId.set(packages[Math.min(1, packages.length - 1)].id);
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  protected close(): void {
    this.closed.emit();
  }

  protected async startCheckout(): Promise<void> {
    const pkgId = this.selectedId();
    if (!pkgId || this.starting()) return;
    this.starting.set(true);
    this.error.set('');
    try {
      const checkout = await firstValueFrom(this.wallet.checkout(pkgId));
      this.checkout.set(checkout);
      const ok = await this.initStripe(checkout.clientSecret);
      if (ok) {
        this.step.set('payment');
      } else {
        this.payError.set('Online payment is not configured yet. Please try again later.');
        this.step.set('failed');
      }
    } catch (err) {
      this.error.set(extractApiError(err).message);
    } finally {
      this.starting.set(false);
    }
  }

  private async initStripe(clientSecret: string): Promise<boolean> {
    try {
      const config = await firstValueFrom(this.wallet.getPaymentConfig());
      if (!config.publishableKey) return false;
      this.stripe = await loadStripe(config.publishableKey);
      if (!this.stripe) return false;
      this.elements = this.stripe.elements({ clientSecret });
      this.paymentElement = this.elements.create('payment');
      // Wait a tick so the payment step is rendered before mounting.
      setTimeout(() => {
        if (this.paymentElement && this.paymentElementHost?.nativeElement) {
          this.paymentElement.mount(this.paymentElementHost.nativeElement);
        }
      });
      return true;
    } catch {
      return false;
    }
  }

  protected async confirmPayment(): Promise<void> {
    if (!this.stripe || !this.elements || this.paying()) return;
    this.paying.set(true);
    this.payError.set('');
    try {
      const result = await this.stripe.confirmPayment({
        elements: this.elements,
        redirect: 'if_required',
      });
      if (result.error) {
        this.payError.set(result.error.message ?? 'Your payment could not be processed.');
        this.step.set('failed');
      } else {
        this.step.set('success');
      }
    } catch (err) {
      this.payError.set(extractApiError(err).message);
      this.step.set('failed');
    } finally {
      this.paying.set(false);
    }
  }

  protected backToPackages(): void {
    this.paymentElement?.destroy();
    this.paymentElement = null;
    this.elements = null;
    this.checkout.set(null);
    this.step.set('packages');
  }
}
