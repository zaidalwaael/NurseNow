# Intentional behavior changes

| Area | Required client/deployment action |
|---|---|
| JWT | Sign in again. Old tokens have no security_stamp/admin_role and will be rejected. Password reset or account suspension invalidates access. |
| Registration/reset | New passwords require 12 characters; public registration only permits Patient/Nurse. Existing password hashes are retained. |
| Forgot password | Unknown email no longer returns 404. The original endpoint remains an acknowledgment; no reset email workflow is added. |
| Payments | Call create-intent, complete Stripe client confirmation, then call payments/confirm. Confirmation without provider success is rejected. Existing endpoint payload shapes stay the same. |
| Previously initiated payments | Intents created before this upgrade have no persisted ID. Reconcile them before deploying; do not blindly create another charge. |
| Files | Send Bearer Authorization on image/document downloads. An HTML img tag cannot attach this header: fetch a blob through the authenticated client or use your app's authenticated image loader. |
| Storage | Existing supplied uploads are moved under API/App_Data/uploads. Configure a durable private storage directory in production and preserve files across releases. |
| Origins/hostnames | Set PublicBaseUrl, AllowedHosts and Cors:AllowedOrigins for your environment. |
| Validation | Invalid DTOs can now return automatic 400 validation responses. Paging/daysAhead bounds are enforced. |
| Concurrency | Conflicting state updates can return 409; refresh before retrying. |
| Auth throttling | Excess auth requests return 429. The default is 20 requests per IP per minute, per process. |
| Bootstrap | No default administrator password and no automatic schema migrations. Seed roles explicitly when needed. |
| Database | Apply reviewed schema upgrade before running payment or new query paths. |

No changes were made to the Flutter/admin client because it was not included. HTTP route preservation is tested; full UI regression still needs the real clients.
