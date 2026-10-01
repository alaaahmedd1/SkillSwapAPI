import { Injectable } from '@angular/core';

import { environment } from '../../../environments/environment';

const GOOGLE_AUTH_URL = 'https://accounts.google.com/o/oauth2/v2/auth';
const GOOGLE_REDIRECT_PATH = '/home';
const APPLE_SDK_URL = 'https://appleid.cdn-apple.com/appleauth/static/jsapi/appleid/1/en_US/appleid.auth.js';

declare global {
  interface Window {
    AppleID?: {
      auth: {
        init(config: { clientId: string; scope: string; redirectURI: string; usePopup: boolean }): void;
        signIn(): Promise<{ authorization: { id_token: string } }>;
      };
    };
  }
}

export interface GoogleRedirectMessage {
  type: 'skillswap:google-oauth';
  hash: string;
}

@Injectable({ providedIn: 'root' })
export class SocialSignInService {
  readonly googleConfigured = !!environment.googleClientId;
  readonly appleConfigured = !!environment.appleClientId && !!environment.appleRedirectUri;

  private readonly scriptLoads = new Map<string, Promise<void>>();

  /** Resolves with the Google ID token, or null when the user closes the popup or cancels at Google. */
  async signInWithGoogle(): Promise<string | null> {
    if (!this.googleConfigured) {
      throw new Error('Google sign-in is not configured.');
    }

    const state = crypto.randomUUID();
    const nonce = crypto.randomUUID();
    const authUrl = new URL(GOOGLE_AUTH_URL);
    authUrl.searchParams.set('client_id', environment.googleClientId);
    authUrl.searchParams.set('redirect_uri', `${window.location.origin}${GOOGLE_REDIRECT_PATH}`);
    authUrl.searchParams.set('response_type', 'id_token');
    authUrl.searchParams.set('scope', 'openid email profile');
    authUrl.searchParams.set('state', state);
    authUrl.searchParams.set('nonce', nonce);
    authUrl.searchParams.set('prompt', 'select_account');

    const popup = window.open(authUrl.toString(), 'skillswap-google-signin', 'popup,width=480,height=640');
    if (!popup) {
      throw new Error('The sign-in popup was blocked. Allow popups for this site and try again.');
    }

    const hash = await this.waitForRedirect(popup);
    if (hash === null) return null;

    const params = new URLSearchParams(hash);
    const error = params.get('error');
    if (error) {
      if (error === 'access_denied') return null;
      throw new Error(`Google sign-in failed: ${error}`);
    }

    if (params.get('state') !== state) {
      throw new Error('Google sign-in failed: state mismatch.');
    }

    const idToken = params.get('id_token');
    if (!idToken) {
      throw new Error('Google sign-in failed: no identity token returned.');
    }

    if (jwtClaim(idToken, 'nonce') !== nonce) {
      throw new Error('Google sign-in failed: nonce mismatch.');
    }

    return idToken;
  }

  /** Resolves with the Apple identity token, or null when the user closes the popup. */
  async signInWithApple(): Promise<string | null> {
    if (!this.appleConfigured) {
      throw new Error('Apple sign-in is not configured.');
    }
    await this.loadScript(APPLE_SDK_URL);
    const appleId = window.AppleID;
    if (!appleId) {
      throw new Error('Apple sign-in failed to load.');
    }
    appleId.auth.init({
      clientId: environment.appleClientId,
      scope: 'name email',
      redirectURI: environment.appleRedirectUri,
      usePopup: true,
    });
    try {
      const response = await appleId.auth.signIn();
      return response.authorization.id_token;
    } catch (err) {
      if (isAppleCancellation(err)) return null;
      throw err;
    }
  }

  /**
   * Waits for the popup to land back on this origin. The app loaded inside the
   * popup posts the URL hash right before closing itself; polling is the fallback.
   */
  private waitForRedirect(popup: Window): Promise<string | null> {
    return new Promise((resolve) => {
      let settled = false;
      const finish = (hash: string | null) => {
        if (settled) return;
        settled = true;
        clearInterval(pollId);
        clearTimeout(timeoutId);
        window.removeEventListener('message', onMessage);
        if (!popup.closed) popup.close();
        resolve(hash);
      };
      const onMessage = (event: MessageEvent) => {
        if (event.origin !== window.location.origin || event.source !== popup) return;
        const data = event.data as GoogleRedirectMessage | null;
        if (data?.type === 'skillswap:google-oauth' && typeof data.hash === 'string') {
          finish(data.hash);
        }
      };
      const pollId = setInterval(() => {
        if (settled) return;
        if (popup.closed) {
          // The popup posts its result just before closing; give that message a beat to arrive.
          setTimeout(() => finish(null), 300);
          return;
        }
        let href: string | null = null;
        try {
          href = popup.location.href;
        } catch {
          // Still on accounts.google.com — cross-origin access is blocked until the redirect happens.
        }
        if (href?.startsWith(window.location.origin)) {
          finish(href.includes('#') ? href.slice(href.indexOf('#') + 1) : null);
        }
      }, 200);
      const timeoutId = setTimeout(() => finish(null), 5 * 60 * 1000);
      window.addEventListener('message', onMessage);
    });
  }

  private loadScript(src: string): Promise<void> {
    const existing = this.scriptLoads.get(src);
    if (existing) return existing;

    const load = new Promise<void>((resolve, reject) => {
      const script = document.createElement('script');
      script.src = src;
      script.async = true;
      script.onload = () => resolve();
      script.onerror = () => {
        this.scriptLoads.delete(src);
        reject(new Error(`Failed to load script: ${src}`));
      };
      document.head.appendChild(script);
    });
    this.scriptLoads.set(src, load);
    return load;
  }
}

function jwtClaim(token: string, claim: string): unknown {
  const part = token.split('.')[1];
  if (!part) return undefined;
  const base64 = part.replace(/-/g, '+').replace(/_/g, '/');
  const padded = base64 + '='.repeat((4 - (base64.length % 4)) % 4);
  try {
    return (JSON.parse(atob(padded)) as Record<string, unknown>)[claim];
  } catch {
    return undefined;
  }
}

function isAppleCancellation(err: unknown): boolean {
  if (typeof err !== 'object' || err === null) return false;
  const code = (err as { error?: string }).error;
  return code === 'popup_closed_by_user' || code === 'user_cancelled_authorize';
}
