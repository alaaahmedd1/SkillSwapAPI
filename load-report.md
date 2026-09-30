# SkillSwapAPI load report

- Target: `http://localhost:5099`
- Database: `Server=.;Database=SkillSwapLoadTestDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`
- Generated (UTC): 2026-09-30 05:23:15
- Seed run stamp: `260930051700`
- **Concurrent requests per endpoint: 1000** (fired simultaneously, no ramp)
- Endpoints measured: 45

## How to read this

Every endpoint was hit with the full concurrency in one burst. `Wall` is the time from the
first request leaving the harness until the last response arrived; `Req/s` is
`concurrency / wall`. Latency percentiles are measured client side and therefore include
queueing inside Kestrel and the SQL connection pool, which is what a real client would see.

`2xx` counts every non-error response, `4xx`/`5xx` are HTTP status buckets and `net` counts
requests that never produced a response (timeout, socket reset, refused connection).

## Summary

| Endpoint | Route | Wall (ms) | Req/s | p50 | p95 | p99 | Max | 2xx | 4xx | 5xx | net |
|---|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| health | `GET /api/health` | 63 | 15774.4 | 4 | 12 | 22 | 28 | 1000 | 0 | 0 | 0 |
| auth-reset-password | `POST /api/auth/reset-password` | 65 | 15284.3 | 39 | 51 | 54 | 59 | 0 | 1000 | 0 | 0 |
| guest-feed | `GET /api/guestfeed` | 73 | 13754.3 | 6 | 32 | 33 | 38 | 1000 | 0 | 0 | 0 |
| payments-webhook | `POST /api/v1/payments/webhook` | 117 | 8564.1 | 79 | 93 | 97 | 106 | 0 | 1000 | 0 | 0 |
| swap-reject | `PUT /api/v1/swap-requests/{swapRequestId}/reject` | 132 | 7577.4 | 107 | 113 | 114 | 118 | 0 | 1000 | 0 | 0 |
| swap-complete | `PUT /api/v1/swap-requests/{swapRequestId}/complete` | 152 | 6593.1 | 110 | 127 | 134 | 137 | 0 | 1000 | 0 | 0 |
| users-get-badges | `GET /api/v1/users/{userId}/badges` | 155 | 6448.0 | 127 | 137 | 144 | 148 | 1000 | 0 | 0 | 0 |
| proposal-accept | `PUT /api/v1/swap-requests/{id}/proposals/{proposalId}/accept` | 165 | 6075.1 | 115 | 139 | 143 | 144 | 0 | 1000 | 0 | 0 |
| review-submit | `POST /api/v1/reviews` | 171 | 5844.6 | 131 | 144 | 148 | 159 | 0 | 1000 | 0 | 0 |
| auth-logout | `POST /api/auth/logout` | 220 | 4547.1 | 152 | 180 | 192 | 216 | 1000 | 0 | 0 | 0 |
| proposal-reject | `PUT /api/v1/swap-requests/{id}/proposals/{proposalId}/reject` | 220 | 4540.3 | 64 | 183 | 197 | 214 | 0 | 1000 | 0 | 0 |
| auth-refresh-token | `POST /api/auth/refresh-token` | 248 | 4038.4 | 185 | 204 | 212 | 226 | 0 | 1000 | 0 | 0 |
| profiles-remove-skill | `DELETE /api/v1/profiles/me/skills/{userSkillId}` | 266 | 3757.8 | 192 | 238 | 244 | 257 | 1000 | 0 | 0 | 0 |
| payments-packages | `GET /api/v1/payments/packages` | 270 | 3698.5 | 126 | 186 | 194 | 269 | 1000 | 0 | 0 | 0 |
| wallet-transaction-details | `GET /api/v1/wallet/transactions/{id}` | 289 | 3464.0 | 230 | 247 | 253 | 287 | 1000 | 0 | 0 | 0 |
| live-session-join | `POST /api/v1/live-sessions/{swapId}/join` | 313 | 3190.5 | 279 | 304 | 309 | 313 | 1000 | 0 | 0 | 0 |
| swap-accept | `PUT /api/v1/swap-requests/{swapRequestId}/accept` | 331 | 3017.3 | 112 | 299 | 319 | 323 | 0 | 1000 | 0 | 0 |
| skills-catalog | `GET /api/v1/skills` | 334 | 2992.4 | 215 | 300 | 314 | 333 | 1000 | 0 | 0 | 0 |
| wallet-me | `GET /api/v1/wallet/me` | 343 | 2917.7 | 315 | 337 | 341 | 342 | 1000 | 0 | 0 | 0 |
| profiles-add-skill | `POST /api/v1/profiles/me/skills` | 406 | 2461.1 | 310 | 374 | 388 | 405 | 1000 | 0 | 0 | 0 |
| profiles-update-me | `PUT /api/v1/profiles/me` | 517 | 1935.8 | 421 | 486 | 502 | 513 | 2 | 998 | 0 | 0 |
| users-get-reviews | `GET /api/v1/users/{userId}/reviews` | 567 | 1764.7 | 373 | 536 | 553 | 563 | 1000 | 0 | 0 | 0 |
| admin-create-category | `POST /api/v1/admin/categories` | 619 | 1616.1 | 369 | 567 | 586 | 608 | 1000 | 0 | 0 | 0 |
| swap-cancel | `PUT /api/v1/swap-requests/{swapRequestId}/cancel` | 669 | 1495.2 | 338 | 639 | 651 | 667 | 1000 | 0 | 0 | 0 |
| wallet-transactions | `GET /api/v1/wallet/transactions` | 786 | 1271.6 | 453 | 733 | 758 | 785 | 1000 | 0 | 0 | 0 |
| profiles-get-me | `GET /api/v1/profiles/me` | 799 | 1251.8 | 406 | 771 | 788 | 797 | 1000 | 0 | 0 | 0 |
| swap-details | `GET /api/v1/swap-requests/{swapRequestId}` | 920 | 1087.0 | 362 | 799 | 885 | 892 | 1000 | 0 | 0 | 0 |
| conversations-messages | `GET /api/v1/conversations/{conversationId}/messages` | 970 | 1030.8 | 642 | 932 | 950 | 967 | 1000 | 0 | 0 | 0 |
| payments-checkout | `POST /api/v1/payments/checkout` | 1002 | 998.0 | 958 | 974 | 980 | 988 | 0 | 0 | 1000 | 0 |
| proposal-create | `POST /api/v1/swap-requests/{id}/proposals` | 1061 | 942.5 | 585 | 989 | 1019 | 1051 | 1000 | 0 | 0 | 0 |
| swap-create | `POST /api/v1/swap-requests` | 1253 | 797.8 | 1096 | 1219 | 1231 | 1245 | 1000 | 0 | 0 | 0 |
| wallet-receipt-email | `POST /api/v1/wallet/transactions/{id}/receipt/email` | 1784 | 560.5 | 1519 | 1740 | 1744 | 1768 | 1000 | 0 | 0 | 0 |
| wallet-transaction-receipt | `GET /api/v1/wallet/transactions/{id}/receipt` | 1844 | 542.4 | 1636 | 1811 | 1829 | 1839 | 1000 | 0 | 0 | 0 |
| admin-update-user-status | `PUT /api/v1/admin/users/{userId}/status` | 3225 | 310.1 | 3107 | 3161 | 3179 | 3189 | 2 | 998 | 0 | 0 |
| live-session-end | `POST /api/v1/live-sessions/{roomId}/end` | 3314 | 301.7 | 2004 | 3198 | 3284 | 3290 | 1000 | 0 | 0 | 0 |
| swap-list | `GET /api/v1/swap-requests` | 3964 | 252.3 | 2578 | 3897 | 3924 | 3955 | 1000 | 0 | 0 | 0 |
| badges-list | `GET /api/v1/badges` | 5071 | 197.2 | 724 | 5006 | 5048 | 5065 | 1000 | 0 | 0 | 0 |
| auth-forgot-password | `POST /api/auth/forgot-password` | 5285 | 189.2 | 1901 | 3570 | 4966 | 5275 | 626 | 0 | 374 | 0 |
| auth-login | `POST /api/auth/login` | 13569 | 73.7 | 10323 | 13519 | 13533 | 13551 | 1000 | 0 | 0 | 0 |
| auth-register | `POST /api/auth/register` | 15420 | 64.8 | 14060 | 15237 | 15302 | 15385 | 1000 | 0 | 0 | 0 |
| auth-verify-otp | `POST /api/auth/verify-otp` | 24758 | 40.4 | 14240 | 24466 | 24497 | 24719 | 1000 | 0 | 0 | 0 |
| admin-list-users | `GET /api/v1/admin/users` | 30156 | 33.2 | 28876 | 29758 | 29895 | 30146 | 1000 | 0 | 0 | 0 |
| admin-create-skill | `POST /api/v1/admin/skills` | 31538 | 31.7 | 18041 | 26436 | 26990 | 31505 | 1000 | 0 | 0 | 0 |
| auth-social-login | `POST /api/auth/social-login` | 33448 | 29.9 | 14350 | 24550 | 25713 | 33420 | 0 | 0 | 1000 | 0 |
| users-search | `GET /api/v1/users/search` | 82278 | 12.2 | 78862 | 82123 | 82217 | 82268 | 1000 | 0 | 0 | 0 |

## Slowest endpoints by p95

| Endpoint | Route | p95 (ms) | p99 (ms) | Req/s |
|---|---|---:|---:|---:|
| users-search | `GET /api/v1/users/search` | 82123 | 82217 | 12.2 |
| admin-list-users | `GET /api/v1/admin/users` | 29758 | 29895 | 33.2 |
| admin-create-skill | `POST /api/v1/admin/skills` | 26436 | 26990 | 31.7 |
| auth-social-login | `POST /api/auth/social-login` | 24550 | 25713 | 29.9 |
| auth-verify-otp | `POST /api/auth/verify-otp` | 24466 | 24497 | 40.4 |
| auth-register | `POST /api/auth/register` | 15237 | 15302 | 64.8 |
| auth-login | `POST /api/auth/login` | 13519 | 13533 | 73.7 |
| badges-list | `GET /api/v1/badges` | 5006 | 5048 | 197.2 |
| swap-list | `GET /api/v1/swap-requests` | 3897 | 3924 | 252.3 |
| auth-forgot-password | `POST /api/auth/forgot-password` | 3570 | 4966 | 189.2 |

## Fastest endpoints by p95

| Endpoint | Route | p95 (ms) | Req/s |
|---|---|---:|---:|
| health | `GET /api/health` | 12 | 15774.4 |
| guest-feed | `GET /api/guestfeed` | 32 | 13754.3 |
| auth-reset-password | `POST /api/auth/reset-password` | 51 | 15284.3 |
| payments-webhook | `POST /api/v1/payments/webhook` | 93 | 8564.1 |
| swap-reject | `PUT /api/v1/swap-requests/{swapRequestId}/reject` | 113 | 7577.4 |
| swap-complete | `PUT /api/v1/swap-requests/{swapRequestId}/complete` | 127 | 6593.1 |
| users-get-badges | `GET /api/v1/users/{userId}/badges` | 137 | 6448.0 |
| proposal-accept | `PUT /api/v1/swap-requests/{id}/proposals/{proposalId}/accept` | 139 | 6075.1 |
| review-submit | `POST /api/v1/reviews` | 144 | 5844.6 |
| auth-logout | `POST /api/auth/logout` | 180 | 4547.1 |

## Per-endpoint detail

### Admin

#### `POST /api/v1/admin/categories` — admin-create-category

- Kind: mutating
- Notes: Unique name per request; writes an audit log row
- Throughput: **1616.1 req/s** over 619 ms for 1000 concurrent requests
- Latency (ms): min 145 / avg 392 / p50 369 / p95 567 / p99 586 / max 608
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 201×1000

#### `POST /api/v1/admin/skills` — admin-create-skill

- Kind: mutating
- Notes: Unique name per request; writes an audit log row
- Throughput: **31.7 req/s** over 31538 ms for 1000 concurrent requests
- Latency (ms): min 4598 / avg 17476 / p50 18041 / p95 26436 / p99 26990 / max 31505
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 201×1000

#### `GET /api/v1/admin/users` — admin-list-users

- Kind: read-only
- Throughput: **33.2 req/s** over 30156 ms for 1000 concurrent requests
- Latency (ms): min 801 / avg 24688 / p50 28876 / p95 29758 / p99 29895 / max 30146
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `PUT /api/v1/admin/users/{userId}/status` — admin-update-user-status

- Kind: mutating
- Notes: Suspends the dedicated hammer user; cascades token revocation and swap cancellation
- Throughput: **310.1 req/s** over 3225 ms for 1000 concurrent requests
- Latency (ms): min 1096 / avg 2799 / p50 3107 / p95 3161 / p99 3179 / max 3189
- Outcomes: 2xx 2, 4xx 998, 5xx 0, transport 0
- Status codes: 400×998, 200×2
- Sample failures:
  - `HTTP 400 PUT /api/v1/admin/users/6d97b1c8-cbe3-4e34-1cd8-08df1eb15f40/status -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`
  - `HTTP 400 PUT /api/v1/admin/users/6d97b1c8-cbe3-4e34-1cd8-08df1eb15f40/status -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`
  - `HTTP 400 PUT /api/v1/admin/users/6d97b1c8-cbe3-4e34-1cd8-08df1eb15f40/status -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`
  - `HTTP 400 PUT /api/v1/admin/users/6d97b1c8-cbe3-4e34-1cd8-08df1eb15f40/status -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`
  - `HTTP 400 PUT /api/v1/admin/users/6d97b1c8-cbe3-4e34-1cd8-08df1eb15f40/status -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`

### Auth

#### `POST /api/auth/register` — auth-register

- Kind: mutating
- Notes: Unique email per request; creates AspNetUser + OTP token
- Throughput: **64.8 req/s** over 15420 ms for 1000 concurrent requests
- Latency (ms): min 1702 / avg 11534 / p50 14060 / p95 15237 / p99 15302 / max 15385
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `POST /api/auth/login` — auth-login

- Kind: mutating
- Notes: Repeatable; issues a new refresh token per call
- Throughput: **73.7 req/s** over 13569 ms for 1000 concurrent requests
- Latency (ms): min 4966 / avg 10086 / p50 10323 / p95 13519 / p99 13533 / max 13551
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `POST /api/auth/social-login` — auth-social-login

- Kind: read-only
- Notes: Requires Google IdP - expected to be rejected without a valid id_token
- Throughput: **29.9 req/s** over 33448 ms for 1000 concurrent requests
- Latency (ms): min 6606 / avg 13880 / p50 14350 / p95 24550 / p99 25713 / max 33420
- Outcomes: 2xx 0, 4xx 0, 5xx 1000, transport 0
- Status codes: 500×1000
- Sample failures:
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`
  - `HTTP 500 POST /api/auth/social-login -> {"code":"SocialAuth.TokenVerificationFailed","message":"An error occurred while verifying the social token."}`

#### `POST /api/auth/refresh-token` — auth-refresh-token

- Kind: mutating
- Notes: Single-use rotation; distinct seeded token per request
- Throughput: **4038.4 req/s** over 248 ms for 1000 concurrent requests
- Latency (ms): min 113 / avg 187 / p50 185 / p95 204 / p99 212 / max 226
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 401×1000
- Sample failures:
  - `HTTP 401 POST /api/auth/refresh-token -> {"code":"Token.RefreshTokenExpired","message":"Refresh token has expired or does not exist."}`
  - `HTTP 401 POST /api/auth/refresh-token -> {"code":"Token.RefreshTokenExpired","message":"Refresh token has expired or does not exist."}`
  - `HTTP 401 POST /api/auth/refresh-token -> {"code":"Token.RefreshTokenExpired","message":"Refresh token has expired or does not exist."}`
  - `HTTP 401 POST /api/auth/refresh-token -> {"code":"Token.RefreshTokenExpired","message":"Refresh token has expired or does not exist."}`
  - `HTTP 401 POST /api/auth/refresh-token -> {"code":"Token.RefreshTokenExpired","message":"Refresh token has expired or does not exist."}`

#### `POST /api/auth/forgot-password` — auth-forgot-password

- Kind: mutating
- Notes: Writes a PasswordReset OTP per call
- Throughput: **189.2 req/s** over 5285 ms for 1000 concurrent requests
- Latency (ms): min 250 / avg 1795 / p50 1901 / p95 3570 / p99 4966 / max 5275
- Outcomes: 2xx 626, 4xx 0, 5xx 374, transport 0
- Status codes: 200×626, 500×374
- Sample failures:
  - `HTTP 500 POST /api/auth/forgot-password -> {"title":"Internal Server Error","status":500,"detail":"An error occurred while saving the entity changes. See the inner exception for details.","instance":"/api/auth/forgot-password"}`
  - `HTTP 500 POST /api/auth/forgot-password -> {"title":"Internal Server Error","status":500,"detail":"An error occurred while saving the entity changes. See the inner exception for details.","instance":"/api/auth/forgot-password"}`
  - `HTTP 500 POST /api/auth/forgot-password -> {"title":"Internal Server Error","status":500,"detail":"An error occurred while saving the entity changes. See the inner exception for details.","instance":"/api/auth/forgot-password"}`
  - `HTTP 500 POST /api/auth/forgot-password -> {"title":"Internal Server Error","status":500,"detail":"An error occurred while saving the entity changes. See the inner exception for details.","instance":"/api/auth/forgot-password"}`
  - `HTTP 500 POST /api/auth/forgot-password -> {"title":"Internal Server Error","status":500,"detail":"An error occurred while saving the entity changes. See the inner exception for details.","instance":"/api/auth/forgot-password"}`

#### `POST /api/auth/verify-otp` — auth-verify-otp

- Kind: mutating
- Notes: Single-use; distinct unconfirmed user + real OTP per request
- Throughput: **40.4 req/s** over 24758 ms for 1000 concurrent requests
- Latency (ms): min 1003 / avg 15444 / p50 14240 / p95 24466 / p99 24497 / max 24719
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `POST /api/auth/reset-password` — auth-reset-password

- Kind: mutating
- Notes: Single-use; distinct user + real OTP per request
- Throughput: **15284.3 req/s** over 65 ms for 1000 concurrent requests
- Latency (ms): min 1 / avg 38 / p50 39 / p95 51 / p99 54 / max 59
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 400×1000
- Sample failures:
  - `HTTP 400 POST /api/auth/reset-password -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"ConfirmPassword":["The ConfirmPassword field is required."]},"traceId":"00-0c585f03c1...`
  - `HTTP 400 POST /api/auth/reset-password -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"ConfirmPassword":["The ConfirmPassword field is required."]},"traceId":"00-27fd337880...`
  - `HTTP 400 POST /api/auth/reset-password -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"ConfirmPassword":["The ConfirmPassword field is required."]},"traceId":"00-43d52fa658...`
  - `HTTP 400 POST /api/auth/reset-password -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"ConfirmPassword":["The ConfirmPassword field is required."]},"traceId":"00-7e656c652f...`
  - `HTTP 400 POST /api/auth/reset-password -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"ConfirmPassword":["The ConfirmPassword field is required."]},"traceId":"00-b9a0971e04...`
  - `HTTP 400 POST /api/auth/reset-password -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"ConfirmPassword":["The ConfirmPassword field is required."]},"traceId":"00-cebc105553...`

#### `POST /api/auth/logout` — auth-logout

- Kind: mutating
- Notes: Revokes a refresh token; distinct token per request
- Throughput: **4547.1 req/s** over 220 ms for 1000 concurrent requests
- Latency (ms): min 76 / avg 147 / p50 152 / p95 180 / p99 192 / max 216
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### Badges

#### `GET /api/v1/badges` — badges-list

- Kind: read-only
- Throughput: **197.2 req/s** over 5071 ms for 1000 concurrent requests
- Latency (ms): min 705 / avg 2324 / p50 724 / p95 5006 / p99 5048 / max 5065
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### Conversations

#### `GET /api/v1/conversations/{conversationId}/messages` — conversations-messages

- Kind: read-only
- Throughput: **1030.8 req/s** over 970 ms for 1000 concurrent requests
- Latency (ms): min 300 / avg 609 / p50 642 / p95 932 / p99 950 / max 967
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### GuestFeed

#### `GET /api/guestfeed` — guest-feed

- Kind: read-only
- Notes: Static sanitized listing data
- Throughput: **13754.3 req/s** over 73 ms for 1000 concurrent requests
- Latency (ms): min 1 / avg 11 / p50 6 / p95 32 / p99 33 / max 38
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### Health

#### `GET /api/health` — health

- Kind: read-only
- Throughput: **15774.4 req/s** over 63 ms for 1000 concurrent requests
- Latency (ms): min 0 / avg 5 / p50 4 / p95 12 / p99 22 / max 28
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### LiveSessions

#### `POST /api/v1/live-sessions/{swapId}/join` — live-session-join

- Kind: mutating
- Notes: Idempotent - returns the existing room token
- Throughput: **3190.5 req/s** over 313 ms for 1000 concurrent requests
- Latency (ms): min 245 / avg 278 / p50 279 / p95 304 / p99 309 / max 313
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `POST /api/v1/live-sessions/{roomId}/end` — live-session-end

- Kind: mutating
- Notes: First call settles the wallet, the rest are idempotent
- Throughput: **301.7 req/s** over 3314 ms for 1000 concurrent requests
- Latency (ms): min 134 / avg 1829 / p50 2004 / p95 3198 / p99 3284 / max 3290
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### Payments

#### `GET /api/v1/payments/packages` — payments-packages

- Kind: read-only
- Throughput: **3698.5 req/s** over 270 ms for 1000 concurrent requests
- Latency (ms): min 97 / avg 136 / p50 126 / p95 186 / p99 194 / max 269
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `POST /api/v1/payments/checkout` — payments-checkout

- Kind: mutating
- Notes: Calls Stripe - fails without an API key in the load profile
- Throughput: **998.0 req/s** over 1002 ms for 1000 concurrent requests
- Latency (ms): min 929 / avg 955 / p50 958 / p95 974 / p99 980 / max 988
- Outcomes: 2xx 0, 4xx 0, 5xx 1000, transport 0
- Status codes: 500×1000
- Sample failures:
  - `HTTP 500 POST /api/v1/payments/checkout -> {"title":"Internal Server Error","status":500,"detail":"No API key provided. Set your API key using \u0060var client = new Stripe.StripeClient(\u0022\u003CAPI-KEY\u003E\u0022)\u0060.You can generate API keys from the Str...`
  - `HTTP 500 POST /api/v1/payments/checkout -> {"title":"Internal Server Error","status":500,"detail":"No API key provided. Set your API key using \u0060var client = new Stripe.StripeClient(\u0022\u003CAPI-KEY\u003E\u0022)\u0060.You can generate API keys from the Str...`
  - `HTTP 500 POST /api/v1/payments/checkout -> {"title":"Internal Server Error","status":500,"detail":"No API key provided. Set your API key using \u0060var client = new Stripe.StripeClient(\u0022\u003CAPI-KEY\u003E\u0022)\u0060.You can generate API keys from the Str...`
  - `HTTP 500 POST /api/v1/payments/checkout -> {"title":"Internal Server Error","status":500,"detail":"No API key provided. Set your API key using \u0060var client = new Stripe.StripeClient(\u0022\u003CAPI-KEY\u003E\u0022)\u0060.You can generate API keys from the Str...`
  - `HTTP 500 POST /api/v1/payments/checkout -> {"title":"Internal Server Error","status":500,"detail":"No API key provided. Set your API key using \u0060var client = new Stripe.StripeClient(\u0022\u003CAPI-KEY\u003E\u0022)\u0060.You can generate API keys from the Str...`

#### `POST /api/v1/payments/webhook` — payments-webhook

- Kind: mutating
- Notes: Anonymous; rejected without a valid Stripe-Signature
- Throughput: **8564.1 req/s** over 117 ms for 1000 concurrent requests
- Latency (ms): min 56 / avg 77 / p50 79 / p95 93 / p99 97 / max 106
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 400×1000
- Sample failures:
  - `HTTP 400 POST /api/v1/payments/webhook -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"Bad Request","status":400,"traceId":"00-4bcaaca3f3c2b13af1f01132fb8b92ef-a38df7b7a4e2da61-00"}`
  - `HTTP 400 POST /api/v1/payments/webhook -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"Bad Request","status":400,"traceId":"00-70e54cbd7cba5aaa3fbf4dd0e9d4bae0-1a3a7e17262a161c-00"}`
  - `HTTP 400 POST /api/v1/payments/webhook -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"Bad Request","status":400,"traceId":"00-7cff14fc6667901cfe0364c9f4815c96-e1fa5c18f4f9833d-00"}`
  - `HTTP 400 POST /api/v1/payments/webhook -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"Bad Request","status":400,"traceId":"00-c122556834ca2723ebf2f83c088fd5bf-17b2bee549bc8cf3-00"}`
  - `HTTP 400 POST /api/v1/payments/webhook -> {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"Bad Request","status":400,"traceId":"00-d7c325eaae5dc6bf1115d59ad694db80-dc87fe54b9183ecb-00"}`

### Profiles

#### `GET /api/v1/profiles/me` — profiles-get-me

- Kind: read-only
- Throughput: **1251.8 req/s** over 799 ms for 1000 concurrent requests
- Latency (ms): min 315 / avg 463 / p50 406 / p95 771 / p99 788 / max 797
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `PUT /api/v1/profiles/me` — profiles-update-me

- Kind: mutating
- Notes: Idempotent write of the same profile values
- Throughput: **1935.8 req/s** over 517 ms for 1000 concurrent requests
- Latency (ms): min 178 / avg 416 / p50 421 / p95 486 / p99 502 / max 513
- Outcomes: 2xx 2, 4xx 998, 5xx 0, transport 0
- Status codes: 400×998, 200×2
- Sample failures:
  - `HTTP 400 PUT /api/v1/profiles/me -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`
  - `HTTP 400 PUT /api/v1/profiles/me -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`
  - `HTTP 400 PUT /api/v1/profiles/me -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`
  - `HTTP 400 PUT /api/v1/profiles/me -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`
  - `HTTP 400 PUT /api/v1/profiles/me -> {"code":"ConcurrencyFailure","message":"Optimistic concurrency failure, object has been modified.","errors":[{"code":"ConcurrencyFailure","description":"Optimistic concurrency failure, object has been modified.","type":2...`

#### `POST /api/v1/profiles/me/skills` — profiles-add-skill

- Kind: mutating
- Notes: Distinct pool user per request so every insert is new
- Throughput: **2461.1 req/s** over 406 ms for 1000 concurrent requests
- Latency (ms): min 148 / avg 301 / p50 310 / p95 374 / p99 388 / max 405
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `DELETE /api/v1/profiles/me/skills/{userSkillId}` — profiles-remove-skill

- Kind: mutating
- Notes: Single-use delete; distinct user-skill per request
- Throughput: **3757.8 req/s** over 266 ms for 1000 concurrent requests
- Latency (ms): min 132 / avg 192 / p50 192 / p95 238 / p99 244 / max 257
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### Reviews

#### `POST /api/v1/reviews` — review-submit

- Kind: mutating
- Notes: Distinct completed swap per request; triggers rating recalculation
- Throughput: **5844.6 req/s** over 171 ms for 1000 concurrent requests
- Latency (ms): min 96 / avg 125 / p50 131 / p95 144 / p99 148 / max 159
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 400×1000
- Sample failures:
  - `HTTP 400 POST /api/v1/reviews -> {"code":"Reviews.InvalidReviewee","message":"The reviewee must be the other participant of the swap request.","errors":[{"code":"Reviews.InvalidReviewee","description":"The reviewee must be the other participant of the s...`
  - `HTTP 400 POST /api/v1/reviews -> {"code":"Reviews.InvalidReviewee","message":"The reviewee must be the other participant of the swap request.","errors":[{"code":"Reviews.InvalidReviewee","description":"The reviewee must be the other participant of the s...`
  - `HTTP 400 POST /api/v1/reviews -> {"code":"Reviews.InvalidReviewee","message":"The reviewee must be the other participant of the swap request.","errors":[{"code":"Reviews.InvalidReviewee","description":"The reviewee must be the other participant of the s...`
  - `HTTP 400 POST /api/v1/reviews -> {"code":"Reviews.InvalidReviewee","message":"The reviewee must be the other participant of the swap request.","errors":[{"code":"Reviews.InvalidReviewee","description":"The reviewee must be the other participant of the s...`
  - `HTTP 400 POST /api/v1/reviews -> {"code":"Reviews.InvalidReviewee","message":"The reviewee must be the other participant of the swap request.","errors":[{"code":"Reviews.InvalidReviewee","description":"The reviewee must be the other participant of the s...`

### SessionProposals

#### `POST /api/v1/swap-requests/{id}/proposals` — proposal-create

- Kind: mutating
- Notes: Distinct accepted swap with no active proposal per request
- Throughput: **942.5 req/s** over 1061 ms for 1000 concurrent requests
- Latency (ms): min 357 / avg 649 / p50 585 / p95 989 / p99 1019 / max 1051
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 201×1000

#### `PUT /api/v1/swap-requests/{id}/proposals/{proposalId}/accept` — proposal-accept

- Kind: mutating
- Notes: Distinct proposal per request; also upserts a live session room
- Throughput: **6075.1 req/s** over 165 ms for 1000 concurrent requests
- Latency (ms): min 90 / avg 113 / p50 115 / p95 139 / p99 143 / max 144
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 403×1000
- Sample failures:
  - `HTTP 403 PUT /api/v1/swap-requests/3629fc89-b129-4a6d-b087-8f63467b3b22/proposals/0a3ac7a6-a098-4d32-bf4f-bd8ecdb6b6ca/accept -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/4b5e67c7-651d-4636-80cd-3d37ac165c9a/proposals/09d322b1-2527-4f7b-b74e-a338b0096369/accept -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/91cbd731-3e91-4774-87be-b30e4a4d8b56/proposals/24ec47c2-dbaa-4527-9b95-cb5307b6cd27/accept -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/b43bad4a-5516-41e3-b86f-ac7d59317c8f/proposals/fe4911f4-7752-4b06-924f-006bd7f58f5c/accept -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/bf0eae8c-71fd-442c-b88b-c21dfa8c0bd2/proposals/12cf8bfb-a6fe-469d-8bc7-b50c84972510/accept -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`

#### `PUT /api/v1/swap-requests/{id}/proposals/{proposalId}/reject` — proposal-reject

- Kind: mutating
- Notes: Distinct proposal per request
- Throughput: **4540.3 req/s** over 220 ms for 1000 concurrent requests
- Latency (ms): min 27 / avg 87 / p50 64 / p95 183 / p99 197 / max 214
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 403×1000
- Sample failures:
  - `HTTP 403 PUT /api/v1/swap-requests/107b9ad9-2a07-492b-b895-8c971e47be5a/proposals/f0a040e3-9edf-42c1-8b18-aad8ace3d391/reject -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/9581d727-f85d-40ae-9118-b679b39078b3/proposals/62c9b37a-1661-41db-a4ad-b2f410491e48/reject -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/a785792c-5752-4b06-8538-a80c99a8cd11/proposals/232192ca-c90c-4a04-9f90-d10b0abf3743/reject -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/dbe4a424-17d7-4166-a8bf-6060b89603ec/proposals/1b9fec91-4721-4cd5-a459-30b785b1097f/reject -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/e7857859-5146-4136-9f06-b1178472f2b0/proposals/59229781-d78a-4002-84ce-c607cbb4a6af/reject -> {"code":"Scheduling.NotParticipant","message":"You are not a participant in this swap request."}`

### Skills

#### `GET /api/v1/skills` — skills-catalog

- Kind: read-only
- Throughput: **2992.4 req/s** over 334 ms for 1000 concurrent requests
- Latency (ms): min 139 / avg 220 / p50 215 / p95 300 / p99 314 / max 333
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### SwapRequests

#### `POST /api/v1/swap-requests` — swap-create

- Kind: mutating
- Notes: Distinct receiver per request to avoid the pending-duplicate rule
- Throughput: **797.8 req/s** over 1253 ms for 1000 concurrent requests
- Latency (ms): min 924 / avg 1099 / p50 1096 / p95 1219 / p99 1231 / max 1245
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 201×1000

#### `GET /api/v1/swap-requests` — swap-list

- Kind: read-only
- Throughput: **252.3 req/s** over 3964 ms for 1000 concurrent requests
- Latency (ms): min 1683 / avg 2623 / p50 2578 / p95 3897 / p99 3924 / max 3955
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `GET /api/v1/swap-requests/{swapRequestId}` — swap-details

- Kind: read-only
- Throughput: **1087.0 req/s** over 920 ms for 1000 concurrent requests
- Latency (ms): min 181 / avg 410 / p50 362 / p95 799 / p99 885 / max 892
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `PUT /api/v1/swap-requests/{swapRequestId}/accept` — swap-accept

- Kind: mutating
- Notes: Distinct pending swap per request
- Throughput: **3017.3 req/s** over 331 ms for 1000 concurrent requests
- Latency (ms): min 73 / avg 152 / p50 112 / p95 299 / p99 319 / max 323
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 403×1000
- Sample failures:
  - `HTTP 403 PUT /api/v1/swap-requests/29ae5922-81a9-4683-95ef-a224d04d6e13/accept -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/3e55e63f-95d8-42b2-aa03-2737bc19c33f/accept -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/67c7f230-47b8-4264-91e9-cbb30526183d/accept -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/b0db1aec-f97b-4c8c-b90b-b596c13a0f3a/accept -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/b7422e42-4143-4138-84c9-cbd7a9048026/accept -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`

#### `PUT /api/v1/swap-requests/{swapRequestId}/reject` — swap-reject

- Kind: mutating
- Notes: Distinct pending swap per request
- Throughput: **7577.4 req/s** over 132 ms for 1000 concurrent requests
- Latency (ms): min 95 / avg 107 / p50 107 / p95 113 / p99 114 / max 118
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 403×1000
- Sample failures:
  - `HTTP 403 PUT /api/v1/swap-requests/0260f758-5c2b-49e9-a9dd-6ebd979f462b/reject -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/2819b65c-e1d6-4aaf-a5ac-3540c177a59c/reject -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/789d9f86-c940-4719-8ebf-c629039d2e93/reject -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/92fd1e50-5a6b-4f4b-8bf2-ec996a53a4da/reject -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/e32625f7-813b-457b-86a7-9a04f6251efb/reject -> {"code":"SwapRequests.OnlyReceiverCanRespond","message":"Only the receiver can accept or reject this swap request."}`

#### `PUT /api/v1/swap-requests/{swapRequestId}/cancel` — swap-cancel

- Kind: mutating
- Notes: Distinct pending swap per request
- Throughput: **1495.2 req/s** over 669 ms for 1000 concurrent requests
- Latency (ms): min 183 / avg 374 / p50 338 / p95 639 / p99 651 / max 667
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `PUT /api/v1/swap-requests/{swapRequestId}/complete` — swap-complete

- Kind: mutating
- Notes: Pre-confirmed by requester so one call completes the swap
- Throughput: **6593.1 req/s** over 152 ms for 1000 concurrent requests
- Latency (ms): min 90 / avg 112 / p50 110 / p95 127 / p99 134 / max 137
- Outcomes: 2xx 0, 4xx 1000, 5xx 0, transport 0
- Status codes: 403×1000
- Sample failures:
  - `HTTP 403 PUT /api/v1/swap-requests/472f618d-70ff-4a63-8f9a-ae8946504970/complete -> {"code":"SwapRequests.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/768cf14f-eda0-4e83-ae0c-d23651249eb7/complete -> {"code":"SwapRequests.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/ac6929f4-486e-4480-a476-3c5220b41c2d/complete -> {"code":"SwapRequests.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/feb94848-0931-4237-97c0-317e6deab5f6/complete -> {"code":"SwapRequests.NotParticipant","message":"You are not a participant in this swap request."}`
  - `HTTP 403 PUT /api/v1/swap-requests/ffd3e071-827a-45d3-b407-18423812dd67/complete -> {"code":"SwapRequests.NotParticipant","message":"You are not a participant in this swap request."}`

### Users

#### `GET /api/v1/users/search` — users-search

- Kind: read-only
- Throughput: **12.2 req/s** over 82278 ms for 1000 concurrent requests
- Latency (ms): min 66398 / avg 78477 / p50 78862 / p95 82123 / p99 82217 / max 82268
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `GET /api/v1/users/{userId}/reviews` — users-get-reviews

- Kind: read-only
- Throughput: **1764.7 req/s** over 567 ms for 1000 concurrent requests
- Latency (ms): min 332 / avg 425 / p50 373 / p95 536 / p99 553 / max 563
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `GET /api/v1/users/{userId}/badges` — users-get-badges

- Kind: read-only
- Throughput: **6448.0 req/s** over 155 ms for 1000 concurrent requests
- Latency (ms): min 113 / avg 127 / p50 127 / p95 137 / p99 144 / max 148
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

### Wallet

#### `GET /api/v1/wallet/me` — wallet-me

- Kind: read-only
- Throughput: **2917.7 req/s** over 343 ms for 1000 concurrent requests
- Latency (ms): min 293 / avg 316 / p50 315 / p95 337 / p99 341 / max 342
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `GET /api/v1/wallet/transactions` — wallet-transactions

- Kind: read-only
- Throughput: **1271.6 req/s** over 786 ms for 1000 concurrent requests
- Latency (ms): min 168 / avg 452 / p50 453 / p95 733 / p99 758 / max 785
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `GET /api/v1/wallet/transactions/{id}` — wallet-transaction-details

- Kind: read-only
- Throughput: **3464.0 req/s** over 289 ms for 1000 concurrent requests
- Latency (ms): min 211 / avg 231 / p50 230 / p95 247 / p99 253 / max 287
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `GET /api/v1/wallet/transactions/{id}/receipt` — wallet-transaction-receipt

- Kind: read-only
- Notes: Generates a PDF per request (QuestPDF) - CPU heavy
- Throughput: **542.4 req/s** over 1844 ms for 1000 concurrent requests
- Latency (ms): min 956 / avg 1507 / p50 1636 / p95 1811 / p99 1829 / max 1839
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 200×1000

#### `POST /api/v1/wallet/transactions/{id}/receipt/email` — wallet-receipt-email

- Kind: mutating
- Notes: Queues a receipt email; SMTP is unconfigured in the load profile
- Throughput: **560.5 req/s** over 1784 ms for 1000 concurrent requests
- Latency (ms): min 1489 / avg 1590 / p50 1519 / p95 1740 / p99 1744 / max 1768
- Outcomes: 2xx 1000, 4xx 0, 5xx 0, transport 0
- Status codes: 202×1000
