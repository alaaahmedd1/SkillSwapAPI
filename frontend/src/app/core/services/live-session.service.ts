import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  LiveSessionRoomDto,
  LiveSessionTimerDto,
  RtcIceCandidateDto,
  RtcSessionDescriptionDto,
} from '../models/domain.models';
import { AuthService } from './auth.service';

/** REST + SignalR surface for live session rooms (/hubs/live-session). */
@Injectable({ providedIn: 'root' })
export class LiveSessionService {
  private readonly http = inject(HttpClient);
  private readonly auth = inject(AuthService);
  private readonly apiBase = environment.apiUrl;

  private hub: signalR.HubConnection | null = null;

  private readonly sessionStartedSubject = new Subject<LiveSessionRoomDto>();
  private readonly sessionEndedSubject = new Subject<LiveSessionRoomDto>();
  private readonly offerSubject = new Subject<RtcSessionDescriptionDto>();
  private readonly answerSubject = new Subject<RtcSessionDescriptionDto>();
  private readonly iceCandidateSubject = new Subject<RtcIceCandidateDto>();
  private readonly whiteboardSubject = new Subject<string>();
  readonly sessionStarted$ = this.sessionStartedSubject.asObservable();
  readonly sessionEnded$ = this.sessionEndedSubject.asObservable();
  readonly offer$ = this.offerSubject.asObservable();
  readonly answer$ = this.answerSubject.asObservable();
  readonly iceCandidate$ = this.iceCandidateSubject.asObservable();
  readonly whiteboardOperation$ = this.whiteboardSubject.asObservable();

  // ── REST ──────────────────────────────────────────────────────────────
  joinRoom(swapRequestId: string) {
    return this.http.post<LiveSessionRoomDto>(`${this.apiBase}/api/v1/live-sessions/${swapRequestId}/join`, {});
  }

  endRoom(roomId: string) {
    return this.http.post<LiveSessionRoomDto>(`${this.apiBase}/api/v1/live-sessions/${roomId}/end`, {});
  }

  // ── Hub ───────────────────────────────────────────────────────────────
  async connect(): Promise<void> {
    if (this.hub && this.hub.state === signalR.HubConnectionState.Connected) return;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${this.apiBase}/hubs/live-session`, {
        accessTokenFactory: () => this.auth.getAccessToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('SessionStarted', (room: LiveSessionRoomDto) => this.sessionStartedSubject.next(room));
    this.hub.on('SessionEnded', (room: LiveSessionRoomDto) => this.sessionEndedSubject.next(room));
    this.hub.on('ReceiveOffer', (desc: RtcSessionDescriptionDto) => this.offerSubject.next(desc));
    this.hub.on('ReceiveAnswer', (desc: RtcSessionDescriptionDto) => this.answerSubject.next(desc));
    this.hub.on('ReceiveIceCandidate', (candidate: RtcIceCandidateDto) => this.iceCandidateSubject.next(candidate));
    this.hub.on('ReceiveWhiteboardOperation', (operation: string) => this.whiteboardSubject.next(operation));
    this.hub.onreconnecting(() => {
      this.auth.refreshSession().subscribe({ error: () => undefined });
    });

    await this.hub.start();
  }

  async joinHubRoom(swapRequestId: string): Promise<void> {
    await this.requireHub().invoke('JoinRoom', swapRequestId);
  }

  async leaveHubRoom(swapRequestId: string): Promise<void> {
    const hub = this.hub;
    if (!hub || hub.state !== signalR.HubConnectionState.Connected) return;
    await hub.invoke('LeaveRoom', swapRequestId);
  }

  async startSession(swapRequestId: string): Promise<LiveSessionRoomDto> {
    return this.requireHub().invoke('StartSession', swapRequestId);
  }

  async sendHeartbeat(swapRequestId: string): Promise<LiveSessionTimerDto> {
    return this.requireHub().invoke('SendSessionHeartbeat', swapRequestId);
  }

  async sendOffer(swapRequestId: string, description: RtcSessionDescriptionDto): Promise<void> {
    await this.requireHub().invoke('SendOffer', swapRequestId, description);
  }

  async sendAnswer(swapRequestId: string, description: RtcSessionDescriptionDto): Promise<void> {
    await this.requireHub().invoke('SendAnswer', swapRequestId, description);
  }

  async sendIceCandidate(swapRequestId: string, candidate: RtcIceCandidateDto): Promise<void> {
    await this.requireHub().invoke('SendIceCandidate', swapRequestId, candidate);
  }

  async sendWhiteboardOperation(swapRequestId: string, operation: string): Promise<void> {
    await this.requireHub().invoke('SendWhiteboardOperation', swapRequestId, operation);
  }

  async disconnect(): Promise<void> {
    const hub = this.hub;
    this.hub = null;
    if (hub) await hub.stop();
  }

  private requireHub(): signalR.HubConnection {
    if (!this.hub || this.hub.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Live session hub is not connected.');
    }
    return this.hub;
  }
}
