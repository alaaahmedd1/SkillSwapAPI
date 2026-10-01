import { Injectable, inject } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';

import { environment } from '../../../environments/environment';
import { MessageDto } from '../models/domain.models';
import { AuthService } from './auth.service';

/** Manages the single SignalR connection to /hubs/chat. */
@Injectable({ providedIn: 'root' })
export class ChatHubService {
  private readonly auth = inject(AuthService);

  private hub: signalR.HubConnection | null = null;
  private readonly messageSubject = new Subject<MessageDto>();
  /** Fires for every message broadcast to a joined conversation (including our own). */
  readonly messages$ = this.messageSubject.asObservable();

  async connect(): Promise<void> {
    if (this.hub && this.hub.state === signalR.HubConnectionState.Connected) return;

    this.hub = new signalR.HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/chat`, {
        accessTokenFactory: () => this.auth.getAccessToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hub.on('ReceiveMessage', (message: MessageDto) => this.messageSubject.next(message));
    this.hub.onreconnecting(() => {
      // Renew the JWT so the re-negotiated connection doesn't fail on an expired token.
      this.auth.refreshSession().subscribe({ error: () => undefined });
    });

    await this.hub.start();
  }

  async joinConversation(conversationId: string): Promise<void> {
    await this.requireHub().invoke('JoinConversation', conversationId);
  }

  async leaveConversation(conversationId: string): Promise<void> {
    const hub = this.hub;
    if (!hub || hub.state !== signalR.HubConnectionState.Connected) return;
    await hub.invoke('LeaveConversation', conversationId);
  }

  /** The broadcast echo appends the message everywhere via messages$. */
  async sendMessage(conversationId: string, content: string): Promise<void> {
    await this.requireHub().invoke('SendMessage', conversationId, content);
  }

  async disconnect(): Promise<void> {
    const hub = this.hub;
    this.hub = null;
    if (hub) await hub.stop();
  }

  private requireHub(): signalR.HubConnection {
    if (!this.hub || this.hub.state !== signalR.HubConnectionState.Connected) {
      throw new Error('Chat is not connected.');
    }
    return this.hub;
  }
}
