import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { AvailabilitySlotDto, ProfileDto, UserSkillDto } from '../models/domain.models';

export interface AddUserSkillPayload {
  skillId: string;
  type: number;
  proficiencyLevel: number;
  yearsOfExperience?: number | null;
}

export interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
  title?: string | null;
  bio?: string | null;
  city?: string | null;
  country?: string | null;
  timeZone?: string | null;
  openForInstantSwaps?: boolean;
  onlineOnly?: boolean;
  autoMatchBarterRequests?: boolean;
}

@Injectable({ providedIn: 'root' })
export class ProfileService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/api/v1/profiles`;

  getProfile(): Observable<ProfileDto> {
    return this.http.get<ProfileDto>(`${this.baseUrl}/me`);
  }

  updateProfile(payload: UpdateProfilePayload): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/me`, payload);
  }

  getAvailability(): Observable<AvailabilitySlotDto[]> {
    return this.http.get<AvailabilitySlotDto[]>(`${this.baseUrl}/me/availability`);
  }

  updateAvailability(slots: AvailabilitySlotDto[]): Observable<unknown> {
    return this.http.put(`${this.baseUrl}/me/availability`, { slots });
  }

  addSkill(payload: AddUserSkillPayload): Observable<UserSkillDto> {
    return this.http.post<UserSkillDto>(`${this.baseUrl}/me/skills`, payload);
  }

  removeSkill(userSkillId: string): Observable<unknown> {
    return this.http.delete(`${this.baseUrl}/me/skills/${userSkillId}`);
  }
}
