import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WalletService } from '../../../core/services/wallet.service';

@Component({
  selector: 'app-upgrade-plan',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './upgrade-plan.html',
  styleUrls: ['./upgrade-plan.scss']
})
export class UpgradePlanComponent implements OnInit {
  private walletService = inject(WalletService);

  isYearly: boolean = true;
  loadingPlanId: string | null = null;
  creditPackages: any[] = [];

  plans = [
    {
      id: '',
      name: 'Free Basic',
      priceMonthly: 0,
      priceYearly: 0,
      hoursPerDay: '2 Hours / day of Skill exchange',
      features: ['Standard community matching', 'Community forum & public posts']
    },
    {
      id: '',
      name: 'Skill Pro',
      priceMonthly: 9.90,
      priceYearly: 7.90,
      hoursPerDay: '5 Hours / day skill exchange pool',
      isPopular: true,
      features: ['Priority mentor matching & zero ads', 'Unlimited audio & direct audio chat']
    },
    {
      id: '',
      name: 'Unlimited Master',
      priceMonthly: 19.99,
      priceYearly: 15.75,
      hoursPerDay: 'Unlimited daily hours of exchange',
      features: ['Priority 24/7 support & Filter gold badge', 'Host verified workshops with 100+ guests']
    }
  ];

  ngOnInit(): void {
    this.walletService.getCreditPackages().subscribe({
      next: (packages) => {
        this.creditPackages = packages;
        if (packages && packages.length > 0) {
          if (packages[0]) this.plans[1].id = packages[0].id;
          if (packages[1]) this.plans[2].id = packages[1].id;
        }
      },
      error: (err) => console.error('Failed to load packages:', err)
    });
  }

  toggleBilling(yearly: boolean): void {
    this.isYearly = yearly;
  }

  selectPlan(plan: any): void {
    if (plan.priceMonthly === 0) return;

    const targetPackageId = plan.id || (this.creditPackages[0] ? this.creditPackages[0].id : null);
    if (!targetPackageId) return;

    this.loadingPlanId = plan.name;

    this.walletService.checkout(targetPackageId).subscribe({
      next: (res: any) => {
        this.loadingPlanId = null;
        console.log('Checkout Response Success:', res);

        if (res?.url || res?.checkoutUrl) {
          window.location.href = res.url || res.checkoutUrl;
        }
      },
      error: (err) => {
        this.loadingPlanId = null;
        console.error('Checkout Error:', err);
      }
    });
  }
}