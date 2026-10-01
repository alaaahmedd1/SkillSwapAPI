import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { PagedResult } from '../models/api.models';
import { MessageDto } from '../models/domain.models';

@Injectable({ providedIn: 'root' })
export class ConversationsService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/conversations`;

  getMessages(conversationId: string, pageNumber = 1, pageSize = 50): Observable<PagedResult<MessageDto>> {
    return this.http.get<PagedResult<MessageDto>>(`${this.baseUrl}/${conversationId}/messages`, {
      params: { pageNumber, pageSize },
    });
  }
}
