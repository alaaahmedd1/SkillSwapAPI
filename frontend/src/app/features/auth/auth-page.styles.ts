// Shared layout/styles for the auth pages (sign-up, sign-in, forgot/verify/reset).
export const AUTH_PAGE_STYLES = `
  :host { display: contents; }
  .auth-page { display: flex; flex-direction: column; padding: 16px 24px calc(32px + env(safe-area-inset-bottom)); background: var(--bg); }
  .auth-header { display: flex; align-items: center; height: 44px; }
  .back {
    width: 40px; height: 40px; display: inline-flex; align-items: center; justify-content: center;
    border: none; background: var(--input-bg); color: var(--text); border-radius: 12px; cursor: pointer;
  }
  h1 { font-size: 24px; font-weight: 700; margin: 10px 0 6px; }
  .sub { font-size: 13.5px; line-height: 1.6; color: var(--text-secondary); margin: 0 0 26px; }
  form .btn-primary { margin-top: 8px; }
  .switch { text-align: center; font-size: 13.5px; color: var(--text-secondary); margin: 22px 0 0; }
  .switch a { font-weight: 600; }
  .social-row { display: flex; gap: 14px; }
  .social-btn {
    flex: 1; height: 52px; display: inline-flex; align-items: center; justify-content: center; gap: 10px;
    background: #fff; border: 1px solid var(--border); border-radius: var(--radius-md); cursor: pointer;
    box-shadow: var(--shadow-card); transition: transform 0.12s ease;
    font-size: 14.5px; font-weight: 600; color: var(--text);
  }
  .social-btn:active { transform: scale(0.97); }
  .social-btn:disabled { opacity: 0.6; cursor: not-allowed; }
  .form-row { display: flex; align-items: center; justify-content: space-between; margin: -6px 0 18px; }
  .checkbox-row { display: inline-flex; align-items: center; gap: 8px; cursor: pointer; font-size: 13px; color: var(--text-secondary); user-select: none; }
  .checkbox-row input { width: 17px; height: 17px; accent-color: var(--primary); margin: 0; cursor: pointer; }

  @media (min-width: 768px) {
    .auth-page { align-items: center; padding: 48px 24px 64px; }
    .auth-page > * { width: 100%; max-width: 440px; }
    h1 { font-size: 28px; }
    .back { transition: background 0.15s ease, color 0.15s ease; }
    .back:hover { background: var(--primary-soft); color: var(--primary); }
    .social-btn:hover { border-color: var(--primary); }
    .social-btn:active { transform: none; }
  }
`;
