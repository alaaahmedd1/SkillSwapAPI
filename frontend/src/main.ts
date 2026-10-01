import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// The Google sign-in popup lands back on this origin with the OAuth result in
// the URL hash. Forward it to the opener that started the flow and close.
const hash = window.location.hash;
const isGoogleRedirect =
  hash.includes('id_token=') || (hash.includes('state=') && hash.includes('error='));
if (window.opener && isGoogleRedirect) {
  window.opener.postMessage(
    { type: 'skillswap:google-oauth', hash: hash.slice(1) },
    window.location.origin
  );
  window.close();
}

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
