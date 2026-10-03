import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';
import { ReviewDto, UserBadgeDto, UserSearchResultDto, BadgeDto } from '../models/domain.models';

@Injectable({ providedIn: 'root' })
export class UsersService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/users`;

  searchUsers(params: { searchTerm?: string; pageNumber?: number; pageSize?: number }): Observable<PagedResult<UserSearchResultDto>> {
    let httpParams = new HttpParams();
    if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    httpParams = httpParams.set('pageNumber', params.pageNumber ?? 1);
    httpParams = httpParams.set('pageSize', params.pageSize ?? 10);
    return this.http.get<PagedResult<UserSearchResultDto>>(`${this.baseUrl}/search`, { params: httpParams });
  }

  getUserReviews(userId: string, pageNumber = 1, pageSize = 10): Observable<PagedResult<ReviewDto>> {
    return this.http.get<PagedResult<ReviewDto>>(`${this.baseUrl}/${userId}/reviews`, {
      params: { pageNumber, pageSize },
    });
  }

  getUserBadges(userId: string): Observable<UserBadgeDto[]> {
    return this.http.get<UserBadgeDto[]>(`${this.baseUrl}/${userId}/badges`);
  }

  getBadges(): Observable<BadgeDto[]> {
    return this.http.get<BadgeDto[]>(`${environment.apiUrl}/api/v1/badges`);
  }
}
