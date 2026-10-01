import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';
import {
  CheckoutResponseDto,
  CreditPackageDto,
  PaymentConfigDto,
  WalletBalanceDto,
  WalletTransactionDto,
} from '../models/domain.models';

export const WALLET_FILTER = { All: 0, Earned: 1, Spent: 2 } as const;

@Injectable({ providedIn: 'root' })
export class WalletService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/wallet`;
  private readonly paymentsUrl = `${environment.apiUrl}/api/v1/payments`;

  getBalance(): Observable<WalletBalanceDto> {
    return this.http.get<WalletBalanceDto>(`${this.baseUrl}/me`);
  }

  getTransactions(
    filter: number = WALLET_FILTER.All,
    pageNumber = 1,
    pageSize = 10
  ): Observable<PagedResult<WalletTransactionDto>> {
    const params = new HttpParams()
      .set('filter', filter)
      .set('pageNumber', pageNumber)
      .set('pageSize', pageSize);
    return this.http.get<PagedResult<WalletTransactionDto>>(`${this.baseUrl}/transactions`, { params });
  }

  getTransactionDetails(transactionId: string): Observable<WalletTransactionDto> {
    return this.http.get<WalletTransactionDto>(`${this.baseUrl}/transactions/${transactionId}`);
  }

  requestReceiptEmail(transactionId: string): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/transactions/${transactionId}/receipt/email`, {});
  }

  getReceiptPdf(transactionId: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/transactions/${transactionId}/receipt`, {
      responseType: 'blob',
    });
  }

  getCreditPackages(): Observable<CreditPackageDto[]> {
    return this.http.get<CreditPackageDto[]>(`${this.paymentsUrl}/packages`);
  }

  checkout(packageId: string): Observable<CheckoutResponseDto> {
    return this.http.post<CheckoutResponseDto>(`${this.paymentsUrl}/checkout`, { packageId });
  }

  getPaymentConfig(): Observable<PaymentConfigDto> {
    return this.http.get<PaymentConfigDto>(`${this.paymentsUrl}/config`);
  }
}
