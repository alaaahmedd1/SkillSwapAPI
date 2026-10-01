import { Component, ElementRef, OnDestroy, OnInit, ViewChild, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription, firstValueFrom } from 'rxjs';

import { LiveSessionRoomDto, LiveSessionStatus, SwapRequestStatus } from '../../core/models/domain.models';
import { AuthService } from '../../core/services/auth.service';
import { LiveSessionService } from '../../core/services/live-session.service';
import { SwapRequestsService } from '../../core/services/swap-requests.service';
import { ToastService } from '../../core/services/toast.service';
import { extractApiError } from '../../core/utils/api-error';
import { AppShellComponent } from '../../shared/components/app-shell/app-shell.component';
import { LoadingSpinnerComponent } from '../../shared/components/loading-spinner/loading-spinner.component';

const RTC_CONFIG: RTCConfiguration = {
  iceServers: [{ urls: 'stun:stun.l.google.com:19302' }],
};

const OFFER_RETRY_MS = 4000;
const HEARTBEAT_MS = 5000;
const WB_FLUSH_MS = 40;

interface StrokeOp {
  t: 's' | 'clear';
  c?: string;
  w?: number;
  pts?: number[];
}

@Component({
  selector: 'app-live-session',
  imports: [RouterLink, AppShellComponent, LoadingSpinnerComponent],
  template: `
    <app-shell>
      <a class="icon-btn" topbar-actions [routerLink]="backLink()" aria-label="Leave meeting">
        <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M19 12H5m7-7-7 7 7 7"/></svg>
      </a>

      @if (loading()) {
        <div class="state-box"><app-spinner [size]="32" /></div>
      } @else if (fatalError()) {
        <div class="state-box">
          <p>{{ fatalError() }}</p>
          <a class="btn btn-soft btn-sm" [routerLink]="backLink()">Go back</a>
        </div>
      } @else {
        <header class="room-head">
          <div>
            <b>{{ peerName() }}</b>
            <p class="sub">{{ skillsLine() }}</p>
          </div>
          @if (elapsed() !== null) {
            <span class="timer" [class.live]="isLive()">
              <span class="dot"></span>{{ formattedElapsed() }}
            </span>
          }
        </header>

        @if (mediaError()) {
          <p class="media-error">{{ mediaError() }}</p>
        }
        @if (hubError()) {
          <p class="media-error">{{ hubError() }}</p>
        }

        <div class="stage">
          <video #remoteVideo class="remote-video" autoplay playsinline></video>
          @if (!remoteActive()) {
            <div class="stage-overlay">
              @if (waitingForPeer()) {
                <app-spinner [size]="26" />
                <p>Waiting for {{ peerName() }} to join…</p>
              } @else {
                <p>Peer video will appear here.</p>
              }
            </div>
          }
          <video #localVideo class="local-video" autoplay playsinline muted></video>

          @if (whiteboardOpen()) {
            <div class="whiteboard">
              <div class="wb-toolbar">
                @for (color of wbColors; track color) {
                  <button class="wb-color" [class.active]="wbColor() === color" [style.background]="color" (click)="wbColor.set(color)" [attr.aria-label]="'Pen ' + color"></button>
                }
                <button class="wb-clear" (click)="clearWhiteboard(true)">Clear</button>
                <button class="wb-close" (click)="whiteboardOpen.set(false)" aria-label="Close whiteboard">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.4" stroke-linecap="round"><path d="M18 6 6 18M6 6l12 12"/></svg>
                </button>
              </div>
              <canvas #wbCanvas class="wb-canvas" (pointerdown)="onWbDown($event)" (pointermove)="onWbMove($event)" (pointerup)="onWbUp()" (pointerleave)="onWbUp()"></canvas>
            </div>
          }
        </div>

        <div class="controls">
          <button class="ctl" [class.off]="!micOn()" (click)="toggleMic()" [attr.aria-label]="micOn() ? 'Mute microphone' : 'Unmute microphone'">
            @if (micOn()) {
              <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="2" width="6" height="12" rx="3"/><path d="M5 10a7 7 0 0 0 14 0"/><path d="M12 19v3"/></svg>
            } @else {
              <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="2" width="6" height="12" rx="3"/><path d="m2 2 20 20"/><path d="M5 10a7 7 0 0 0 14 0"/><path d="M12 19v3"/></svg>
            }
          </button>
          <button class="ctl" [class.off]="!camOn()" (click)="toggleCam()" [attr.aria-label]="camOn() ? 'Turn camera off' : 'Turn camera on'">
            @if (camOn()) {
              <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M23 7l-7 5 7 5V7z"/><rect x="1" y="5" width="15" height="14" rx="2"/></svg>
            } @else {
              <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="m2 2 20 20"/><path d="M23 7l-7 5 7 5V7z"/><rect x="1" y="5" width="15" height="14" rx="2"/></svg>
            }
          </button>
          <button class="ctl" [class.active-ctl]="whiteboardOpen()" (click)="toggleWhiteboard()" aria-label="Whiteboard">
            <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 19l7-7 3 3-7 7-3-3z"/><path d="M18 13l-1.5-7.5L2 2l3.5 14.5L13 18l5-5z"/><circle cx="11" cy="11" r="2"/></svg>
          </button>
          @if (canStart()) {
            <button class="ctl start" (click)="startSession()" aria-label="Start session" [disabled]="starting()">
              @if (starting()) { <app-spinner [size]="18" /> } @else {
                <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polygon points="6 3 20 12 6 21 6 3"/></svg>
              }
            </button>
          }
          <button class="ctl end" (click)="endSession()" aria-label="End session">
            <svg width="19" height="19" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="transform: rotate(135deg)"><path d="M10.68 13.31a16 16 0 0 0 3.41.6 2 2 0 0 1 1.91 2v3a2 2 0 0 1-2.18 2 19.79 19.79 0 0 1-8.63-3.07 19.5 19.5 0 0 1-6-6 19.79 19.79 0 0 1-3.07-8.67A2 2 0 0 1 4.11 2h3a2 2 0 0 1 2 1.72c.13.96.36 1.9.7 2.81a2 2 0 0 1-.45 2.11L8.09 9.91a16 16 0 0 0 6 6l1.27-1.27a2 2 0 0 1 2.11-.45c.91.34 1.85.57 2.81.7A2 2 0 0 1 22 16.92z"/></svg>
          </button>
        </div>
      }
    </app-shell>
  `,
  styles: `
    :host { display: contents; }
    .icon-btn { width: 38px; height: 38px; display: inline-flex; align-items: center; justify-content: center; background: var(--input-bg); color: var(--text); border-radius: 12px; }
    .state-box { display: flex; flex-direction: column; align-items: center; gap: 12px; text-align: center; padding: 36px 16px; color: var(--text-secondary); font-size: 13.5px; }

    .room-head { display: flex; align-items: center; justify-content: space-between; gap: 10px; margin-bottom: 12px; }
    .room-head b { font-size: 15px; }
    .sub { font-size: 11.5px; color: var(--text-muted); margin: 2px 0 0; }
    .timer { display: inline-flex; align-items: center; gap: 6px; font-size: 12px; font-weight: 600; color: var(--text-secondary); background: var(--input-bg); padding: 6px 12px; border-radius: 999px; }
    .timer .dot { width: 7px; height: 7px; border-radius: 50%; background: var(--text-muted); }
    .timer.live .dot { background: var(--danger); animation: pulse 1.2s ease infinite; }
    @keyframes pulse { 50% { opacity: 0.35; } }

    .media-error { font-size: 12px; color: var(--warning); background: #fff6e5; border-radius: var(--radius-sm); padding: 8px 12px; margin: 0 0 10px; }

    .stage { position: relative; border-radius: var(--radius-lg); overflow: hidden; background: #14142b; aspect-ratio: 4 / 3; box-shadow: var(--shadow-card); }
    .remote-video { position: absolute; inset: 0; width: 100%; height: 100%; object-fit: cover; }
    .stage-overlay { position: absolute; inset: 0; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 10px; color: rgba(255, 255, 255, 0.85); font-size: 12.5px; }
    .local-video { position: absolute; right: 10px; bottom: 10px; width: 96px; aspect-ratio: 3 / 4; object-fit: cover; border-radius: 12px; border: 2px solid rgba(255, 255, 255, 0.6); background: #000; z-index: 2; }

    .whiteboard { position: absolute; inset: 0; z-index: 3; display: flex; flex-direction: column; background: #fff; }
    .wb-toolbar { display: flex; align-items: center; gap: 8px; padding: 8px 10px; border-bottom: 1px solid var(--border); }
    .wb-color { width: 22px; height: 22px; border-radius: 50%; border: 2px solid transparent; cursor: pointer; }
    .wb-color.active { border-color: var(--text); }
    .wb-clear { margin-left: auto; font-size: 11.5px; font-weight: 600; color: var(--danger); background: none; border: none; cursor: pointer; }
    .wb-close { background: var(--input-bg); border: none; border-radius: 8px; width: 26px; height: 26px; display: inline-flex; align-items: center; justify-content: center; cursor: pointer; color: var(--text-secondary); }
    .wb-canvas { flex: 1; width: 100%; touch-action: none; cursor: crosshair; }

    .controls { display: flex; justify-content: center; gap: 12px; margin-top: 16px; }
    .ctl { width: 48px; height: 48px; border-radius: 50%; border: none; background: var(--input-bg); color: var(--text); display: inline-flex; align-items: center; justify-content: center; cursor: pointer; transition: transform 0.12s ease, background 0.15s ease; }
    .ctl:active { transform: scale(0.94); }
    .ctl.off { background: var(--danger-light); color: var(--danger); }
    .ctl.active-ctl { background: var(--primary-light); color: var(--primary); }
    .ctl.start { background: var(--success-light); color: var(--success); }
    .ctl.end { background: var(--danger); color: #fff; }
  `,
})
export class LiveSessionComponent implements OnInit, OnDestroy {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly auth = inject(AuthService);
  private readonly swapRequests = inject(SwapRequestsService);
  private readonly liveSession = inject(LiveSessionService);
  private readonly toast = inject(ToastService);

  @ViewChild('localVideo') private localVideoRef?: ElementRef<HTMLVideoElement>;
  @ViewChild('remoteVideo') private remoteVideoRef?: ElementRef<HTMLVideoElement>;
  @ViewChild('wbCanvas') private wbCanvasRef?: ElementRef<HTMLCanvasElement>;

  protected readonly loading = signal(true);
  protected readonly fatalError = signal('');
  protected readonly mediaError = signal('');
  protected readonly hubError = signal('');
  protected readonly peerName = signal('');
  protected readonly skillsLine = signal('');
  protected readonly room = signal<LiveSessionRoomDto | null>(null);
  protected readonly elapsed = signal<number | null>(null);
  protected readonly starting = signal(false);
  protected readonly micOn = signal(true);
  protected readonly camOn = signal(true);
  protected readonly whiteboardOpen = signal(false);
  protected readonly wbColor = signal('#1f1f3d');
  protected readonly wbColors = ['#1f1f3d', '#7c5cfc', '#2fbf71', '#ff5a5a', '#ffb930'];

  private swapId = '';
  private requesterId = '';
  private localStream: MediaStream | null = null;
  private peer: RTCPeerConnection | null = null;
  private offerAnswered = false;
  private offerRetryTimer: ReturnType<typeof setInterval> | null = null;
  private heartbeatTimer: ReturnType<typeof setInterval> | null = null;
  private remoteDescSet = false;
  private pendingOffers: RTCSessionDescriptionInit[] = [];
  private drawing = false;
  private lastPt: { x: number; y: number } | null = null;
  private wbPendingPts: number[] = [];
  private wbLastSentAt = 0;
  private wbFlushTimer: ReturnType<typeof setTimeout> | null = null;
  private cameraStarting = false;
  private renegotiateInFlight = false;
  private hubSubscriptions: Subscription[] = [];

  protected readonly isLive = computed(() => this.room()?.status === LiveSessionStatus.InProgress);
  protected readonly canStart = computed(() => {
    const room = this.room();
    return !!room && room.status === LiveSessionStatus.Waiting;
  });
  protected readonly remoteActive = computed(() => this.remoteDescSet);
  protected readonly waitingForPeer = computed(() => this.isLive() && !this.remoteDescSet);

  protected readonly formattedElapsed = computed(() => {
    const seconds = this.elapsed();
    if (seconds === null) return '';
    const m = Math.floor(seconds / 60);
    const s = seconds % 60;
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  });

  protected readonly backLink = computed(() => {
    const swapId = this.route.snapshot.paramMap.get('swapId');
    return swapId ? ['/swaps', swapId] : ['/swaps'];
  });

  async ngOnInit(): Promise<void> {
    this.swapId = this.route.snapshot.paramMap.get('swapId') ?? '';
    if (!this.swapId) {
      this.fatalError.set('Missing session link.');
      this.loading.set(false);
      return;
    }
    try {
      const swap = await firstValueFrom(this.swapRequests.details(this.swapId));
      if (swap.status !== SwapRequestStatus.Accepted) {
        this.fatalError.set('The meeting opens once the swap is accepted.');
        this.loading.set(false);
        return;
      }
      this.requesterId = swap.requesterId;
      const myId = this.auth.user()?.userId ?? '';
      this.peerName.set(
        swap.requesterId === myId
          ? `${swap.receiverFirstName} ${swap.receiverLastName}`
          : `${swap.requesterFirstName} ${swap.requesterLastName}`
      );
      this.skillsLine.set(`${swap.offeredSkill.skillName} ↔ ${swap.requestedSkill.skillName}`);

      const room = await firstValueFrom(this.liveSession.joinRoom(this.swapId));
      this.room.set(room);
      await this.connectHub();

      if (room.status === LiveSessionStatus.InProgress) {
        await this.beginMediaAndCall();
        this.startHeartbeat();
      }
    } catch (err) {
      this.fatalError.set(extractApiError(err).message);
    } finally {
      this.loading.set(false);
    }
  }

  private async connectHub(): Promise<void> {
    try {
      await this.liveSession.connect();
      await this.liveSession.joinHubRoom(this.swapId);
      this.hubSubscriptions.push(
        this.liveSession.sessionStarted$.subscribe(() => {
          this.room.update((r) => (r ? { ...r, status: LiveSessionStatus.InProgress } : r));
          void this.onSessionStarted();
        }),
        this.liveSession.offer$.subscribe((desc) => void this.handleOffer(desc)),
        this.liveSession.answer$.subscribe((desc) => void this.handleAnswer(desc)),
        this.liveSession.iceCandidate$.subscribe((candidate) => void this.handleIceCandidate(candidate)),
        this.liveSession.whiteboardOperation$.subscribe((op) => this.handleWhiteboardOp(op))
      );
    } catch (err) {
      this.hubError.set(extractApiError(err).message);
    }
  }

  private async onSessionStarted(): Promise<void> {
    await this.beginMediaAndCall();
    this.startHeartbeat();
  }

  protected async startSession(): Promise<void> {
    if (this.starting()) return;
    this.starting.set(true);
    try {
      const room = await this.liveSession.startSession(this.swapId);
      this.room.set(room);
      // SessionStarted echo also fires; onSessionStarted guards against double setup.
      await this.onSessionStarted();
    } catch (err) {
      this.toast.error(extractApiError(err).message);
    } finally {
      this.starting.set(false);
    }
  }

  // ── WebRTC ──────────────────────────────────────────────────────────────
  private isRequester(): boolean {
    return this.auth.user()?.userId === this.requesterId;
  }

  private async beginMediaAndCall(): Promise<void> {
    if (this.peer) return; // already set up (SessionStarted echo)
    this.localStream = await this.acquireMedia();

    this.peer = new RTCPeerConnection(RTC_CONFIG);
    if (this.localStream) {
      for (const track of this.localStream.getTracks()) {
        this.peer.addTrack(track, this.localStream);
      }
    }
    this.peer.onicecandidate = (event) => {
      if (event.candidate) {
        void this.liveSession
          .sendIceCandidate(this.swapId, {
            candidate: event.candidate.candidate,
            sdpMid: event.candidate.sdpMid,
            sdpMLineIndex: event.candidate.sdpMLineIndex,
          })
          .catch(() => undefined);
      }
    };
    this.peer.ontrack = (event) => {
      if (this.remoteVideoRef) {
        this.remoteVideoRef.nativeElement.srcObject = event.streams[0] ?? null;
      }
    };
    this.peer.onconnectionstatechange = () => {
      if (this.peer?.connectionState === 'connected') {
        this.stopOfferRetry();
      }
    };

    // Process any offer that arrived before the peer was ready.
    for (const offer of this.pendingOffers.splice(0)) {
      await this.answerOffer(offer);
    }

    if (this.isRequester()) {
      void this.sendOfferWithRetry();
    }
  }

  private async acquireMedia(): Promise<MediaStream | null> {
    if (!navigator.mediaDevices?.getUserMedia) {
      this.mediaError.set('Camera/microphone unavailable in this browser. You can still use chat and the whiteboard.');
      this.micOn.set(false);
      this.camOn.set(false);
      return null;
    }

    let mediaFailure: unknown = null;
    const attempts: MediaStreamConstraints[] = [
      { video: true, audio: true },
      { audio: true },
      { video: true },
    ];
    for (const constraints of attempts) {
      try {
        const stream = await navigator.mediaDevices.getUserMedia(constraints);
        if (this.localVideoRef) this.localVideoRef.nativeElement.srcObject = stream;
        const hasAudio = stream.getAudioTracks().length > 0;
        const hasVideo = stream.getVideoTracks().length > 0;
        this.micOn.set(hasAudio);
        this.camOn.set(hasVideo);
        if (hasAudio && hasVideo) {
          this.mediaError.set('');
        } else {
          this.mediaError.set(this.mediaFailureMessage(mediaFailure));
        }
        return stream;
      } catch (err) {
        mediaFailure = err;
      }
    }

    this.mediaError.set(this.mediaFailureMessage(mediaFailure));
    this.micOn.set(false);
    this.camOn.set(false);
    return null;
  }

  private mediaFailureMessage(err: unknown): string {
    const name = err instanceof DOMException ? err.name : '';
    switch (name) {
      case 'NotAllowedError':
      case 'SecurityError':
        return 'Camera blocked by browser or Windows privacy settings — allow camera for this site and in Windows Settings → Privacy & security → Camera.';
      case 'NotReadableError':
      case 'AbortError':
        return 'Camera is in use by another app — close apps like Zoom/Teams/OBS, then press the camera button to retry.';
      case 'NotFoundError':
      case 'OverconstrainedError':
        return 'No camera found on this device.';
      default:
        return 'Camera unavailable. You can still use voice, chat and the whiteboard.';
    }
  }

  private async handleOffer(desc: { type: string; sdp: string }): Promise<void> {
    const offer: RTCSessionDescriptionInit = { type: desc.type as RTCSdpType, sdp: desc.sdp };
    if (!this.peer) {
      this.pendingOffers.push(offer);
      return;
    }
    await this.answerOffer(offer);
  }

  private async answerOffer(offer: RTCSessionDescriptionInit): Promise<void> {
    if (!this.peer) return;
    try {
      await this.peer.setRemoteDescription(offer);
      this.remoteDescSet = true;
      const answer = await this.peer.createAnswer();
      await this.peer.setLocalDescription(answer);
      await this.liveSession.sendAnswer(this.swapId, { type: answer.type, sdp: answer.sdp ?? '' });
    } catch {
      // The requester's retry loop will re-offer.
    }
  }

  private async handleAnswer(desc: { type: string; sdp: string }): Promise<void> {
    if (!this.peer) return;
    try {
      await this.peer.setRemoteDescription({ type: desc.type as RTCSdpType, sdp: desc.sdp });
      this.remoteDescSet = true;
      this.offerAnswered = true;
      this.stopOfferRetry();
    } catch {
      // Ignore — retry loop keeps going until the connection is stable.
    }
  }

  private async handleIceCandidate(candidate: { candidate: string; sdpMid: string | null; sdpMLineIndex: number | null }): Promise<void> {
    if (!this.peer || !candidate.candidate) return;
    try {
      await this.peer.addIceCandidate({
        candidate: candidate.candidate,
        sdpMid: candidate.sdpMid,
        sdpMLineIndex: candidate.sdpMLineIndex ?? 0,
      });
    } catch {
      // Candidates can race the remote description; non-fatal.
    }
  }

  private sendOfferWithRetry(): void {
    void this.sendOfferOnce();
    this.offerRetryTimer = setInterval(() => void this.sendOfferOnce(), OFFER_RETRY_MS);
  }

  private async sendOfferOnce(): Promise<void> {
    if (!this.peer || this.offerAnswered) {
      this.stopOfferRetry();
      return;
    }
    try {
      const offer = await this.peer.createOffer();
      await this.peer.setLocalDescription(offer);
      await this.liveSession.sendOffer(this.swapId, { type: offer.type, sdp: offer.sdp ?? '' });
    } catch {
      // Retry on next tick.
    }
  }

  private stopOfferRetry(): void {
    if (this.offerRetryTimer) {
      clearInterval(this.offerRetryTimer);
      this.offerRetryTimer = null;
    }
  }

  private async renegotiate(): Promise<void> {
    if (!this.peer || this.renegotiateInFlight) return;
    this.renegotiateInFlight = true;
    this.offerAnswered = false;
    try {
      await this.sendOfferOnce();
      this.stopOfferRetry();
      this.offerRetryTimer = setInterval(() => void this.sendOfferOnce(), OFFER_RETRY_MS);
    } finally {
      this.renegotiateInFlight = false;
    }
  }

  // ── Heartbeat timer ─────────────────────────────────────────────────────
  private startHeartbeat(): void {
    if (this.heartbeatTimer) return;
    void this.pulse();
    this.heartbeatTimer = setInterval(() => void this.pulse(), HEARTBEAT_MS);
  }

  private async pulse(): Promise<void> {
    try {
      const timer = await this.liveSession.sendHeartbeat(this.swapId);
      this.elapsed.set(timer.elapsedSeconds);
    } catch {
      // Room may have ended; timer UI simply stops updating.
    }
  }

  // ── Media controls ──────────────────────────────────────────────────────
  protected toggleMic(): void {
    const track = this.localStream?.getAudioTracks()[0];
    if (!track) return;
    track.enabled = !track.enabled;
    this.micOn.set(track.enabled);
  }

  protected async toggleCam(): Promise<void> {
    const track = this.localStream?.getVideoTracks()[0];
    if (track) {
      track.enabled = !track.enabled;
      this.camOn.set(track.enabled);
      return;
    }
    if (!navigator.mediaDevices?.getUserMedia || this.cameraStarting) return;
    this.cameraStarting = true;
    try {
      const stream = await navigator.mediaDevices.getUserMedia({ video: true });
      const videoTrack = stream.getVideoTracks()[0];
      if (!videoTrack) return;
      if (this.localStream) this.localStream.addTrack(videoTrack);
      else this.localStream = stream;
      if (this.localVideoRef) this.localVideoRef.nativeElement.srcObject = this.localStream;
      this.camOn.set(true);
      this.mediaError.set('');
      if (this.peer) {
        this.peer.addTrack(videoTrack, this.localStream);
        await this.renegotiate();
      }
    } catch (err) {
      this.mediaError.set(this.mediaFailureMessage(err));
    } finally {
      this.cameraStarting = false;
    }
  }

  // ── Whiteboard ──────────────────────────────────────────────────────────
  protected toggleWhiteboard(): void {
    this.whiteboardOpen.update((v) => !v);
    if (this.whiteboardOpen()) {
      queueMicrotask(() => this.setupCanvas());
    }
  }

  private setupCanvas(): void {
    const canvas = this.wbCanvasRef?.nativeElement;
    if (!canvas) return;
    const parent = canvas.parentElement!;
    canvas.width = parent.clientWidth;
    canvas.height = parent.clientHeight - 39; // toolbar height
  }

  private canvasPoint(event: PointerEvent): { x: number; y: number } {
    const canvas = this.wbCanvasRef!.nativeElement;
    const rect = canvas.getBoundingClientRect();
    return {
      x: (event.clientX - rect.left) / rect.width,
      y: (event.clientY - rect.top) / rect.height,
    };
  }

  protected onWbDown(event: PointerEvent): void {
    if (!this.whiteboardOpen()) return;
    this.drawing = true;
    this.lastPt = this.canvasPoint(event);
  }

  protected onWbMove(event: PointerEvent): void {
    if (!this.drawing || !this.lastPt) return;
    const pt = this.canvasPoint(event);
    const op: StrokeOp = { t: 's', c: this.wbColor(), w: 3, pts: [this.lastPt.x, this.lastPt.y, pt.x, pt.y] };
    this.drawStroke(op);
    this.wbPendingPts.push(this.lastPt.x, this.lastPt.y, pt.x, pt.y);
    this.scheduleStrokeFlush();
    this.lastPt = pt;
  }

  protected onWbUp(): void {
    this.drawing = false;
    this.lastPt = null;
    this.flushStroke();
  }

  private scheduleStrokeFlush(): void {
    const dueIn = Math.max(0, WB_FLUSH_MS - (Date.now() - this.wbLastSentAt));
    if (dueIn === 0) {
      this.flushStroke();
    } else if (!this.wbFlushTimer) {
      this.wbFlushTimer = setTimeout(() => this.flushStroke(), dueIn);
    }
  }

  private flushStroke(): void {
    if (this.wbFlushTimer) {
      clearTimeout(this.wbFlushTimer);
      this.wbFlushTimer = null;
    }
    if (this.wbPendingPts.length < 4) {
      this.wbPendingPts = [];
      return;
    }
    const op: StrokeOp = { t: 's', c: this.wbColor(), w: 3, pts: [...this.wbPendingPts] };
    this.wbPendingPts = [];
    this.wbLastSentAt = Date.now();
    void this.liveSession.sendWhiteboardOperation(this.swapId, JSON.stringify(op)).catch(() => undefined);
  }

  private drawStroke(op: StrokeOp): void {
    const canvas = this.wbCanvasRef?.nativeElement;
    if (!canvas || !op.pts || op.pts.length < 4) return;
    const ctx = canvas.getContext('2d');
    if (!ctx) return;
    ctx.strokeStyle = op.c ?? '#1f1f3d';
    ctx.lineWidth = op.w ?? 3;
    ctx.lineCap = 'round';
    ctx.beginPath();
    for (let i = 0; i + 3 < op.pts.length; i += 2) {
      ctx.moveTo(op.pts[i] * canvas.width, op.pts[i + 1] * canvas.height);
      ctx.lineTo(op.pts[i + 2] * canvas.width, op.pts[i + 3] * canvas.height);
    }
    ctx.stroke();
  }

  protected clearWhiteboard(broadcast: boolean): void {
    const canvas = this.wbCanvasRef?.nativeElement;
    canvas?.getContext('2d')?.clearRect(0, 0, canvas.width, canvas.height);
    this.wbPendingPts = [];
    if (broadcast) {
      const op: StrokeOp = { t: 'clear' };
      void this.liveSession.sendWhiteboardOperation(this.swapId, JSON.stringify(op)).catch(() => undefined);
    }
  }

  private handleWhiteboardOp(raw: string): void {
    if (!this.whiteboardOpen()) this.whiteboardOpen.set(true);
    queueMicrotask(() => {
      let op: StrokeOp;
      try {
        op = JSON.parse(raw) as StrokeOp;
      } catch {
        return;
      }
      if (op.t === 'clear') {
        this.clearWhiteboard(false);
      } else {
        this.drawStroke(op);
      }
    });
  }

  // ── Teardown ────────────────────────────────────────────────────────────
  protected async endSession(): Promise<void> {
    const room = this.room();
    if (room && room.status !== LiveSessionStatus.Ended
        && !window.confirm('End this session? Elapsed time will be settled from the learner\'s wallet.')) {
      return;
    }
    try {
      if (room && room.status !== LiveSessionStatus.Ended) {
        await firstValueFrom(this.liveSession.endRoom(room.id));
      }
    } catch {
      // Best effort — local teardown below still runs.
    }
    this.toast.success('Session ended.');
    void this.router.navigate(this.backLink());
  }

  ngOnDestroy(): void {
    this.stopOfferRetry();
    if (this.wbFlushTimer) {
      clearTimeout(this.wbFlushTimer);
      this.wbFlushTimer = null;
    }
    if (this.heartbeatTimer) {
      clearInterval(this.heartbeatTimer);
      this.heartbeatTimer = null;
    }
    for (const sub of this.hubSubscriptions) sub.unsubscribe();
    this.hubSubscriptions = [];
    this.peer?.close();
    this.peer = null;
    this.localStream?.getTracks().forEach((t) => t.stop());
    this.localStream = null;
    void this.liveSession.leaveHubRoom(this.swapId).catch(() => undefined);
    void this.liveSession.disconnect();
  }
}
