import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';
import {
  CreateSwapRequestPayload,
  SessionProposalDto,
  SwapRequestDetailsDto,
  SwapRequestDto,
} from '../models/domain.models';

export interface CreateProposalPayload {
  scheduledDate: string; // yyyy-MM-dd
  startTime: string; // HH:mm:ss
  endTime: string;
  durationMinutes: number;
}

@Injectable({ providedIn: 'root' })
export class SwapRequestsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/swap-requests`;

  create(payload: CreateSwapRequestPayload): Observable<SwapRequestDto> {
    return this.http.post<SwapRequestDto>(this.baseUrl, payload);
  }

  list(status?: number, pageNumber = 1, pageSize = 10): Observable<PagedResult<SwapRequestDto>> {
    let params = new HttpParams().set('pageNumber', pageNumber).set('pageSize', pageSize);
    if (status) params = params.set('status', status);
    return this.http.get<PagedResult<SwapRequestDto>>(this.baseUrl, { params });
  }

  details(id: string): Observable<SwapRequestDetailsDto> {
    return this.http.get<SwapRequestDetailsDto>(`${this.baseUrl}/${id}`);
  }

  accept(id: string): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/${id}/accept`, {});
  }

  reject(id: string): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/${id}/reject`, {});
  }

  cancel(id: string): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/${id}/cancel`, {});
  }

  complete(id: string): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/${id}/complete`, {});
  }

  createProposal(swapRequestId: string, payload: CreateProposalPayload): Observable<SessionProposalDto> {
    return this.http.post<SessionProposalDto>(`${this.baseUrl}/${swapRequestId}/proposals`, payload);
  }

  acceptProposal(swapRequestId: string, proposalId: string): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/${swapRequestId}/proposals/${proposalId}/accept`, {});
  }

  rejectProposal(swapRequestId: string, proposalId: string): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/${swapRequestId}/proposals/${proposalId}/reject`, {});
  }
}
