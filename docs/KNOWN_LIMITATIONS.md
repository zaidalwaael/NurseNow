# Boundaries and remaining work

- This is a refactor and targeted hardening pass, not a production/security certification. See ARCHITECTURE.md for retained EF/Identity coupling.
- No access to the user's SQL Server, live Stripe/SMTP configuration or frontends. SQL scripts were generated, not applied. SQLite tests cannot prove SQL Server locking, query plans or schema upgrade compatibility.
- Payments still require the client to invoke confirmation. A signed Stripe webhook plus reconciliation/outbox and refund/cancellation policy are not implemented. A successfully captured payment concurrent with booking cancellation needs reconciliation.
- The original forgot-password endpoint only acknowledges requests; it does not generate or deliver reset tokens. The original registration's EmailConfirmed behavior remains. The SMTP adapter is retained but not presented as a completed email-verification flow.
- Scheduling retains server-local DateTime.Today and string statuses/day names. A clinic timezone and formal state machine need an explicit product decision.
- Read-only legacy list endpoints that had no pagination keep their response contracts; they can still grow large. Notification broadcasts still materialize recipient IDs and use one transaction; very large audiences need batching/outbox.
- Uploads validate extensions and size, not malware or full file signatures. File writes and SQL updates are not a distributed transaction; an interrupted update can leave an orphan file. A cleanup job and scanner are not included.
- Role-level access is preserved; VerificationAdmin/SupportAdmin permissions were not redesigned into a granular permission matrix. JWT validation performs one small account query per authenticated request to enforce revocation.
- Auth throttling is in-memory per instance. Configure trusted proxy forwarding and shared throttling for multiple instances; never trust arbitrary forwarded headers.
- Original package versions are pinned for compatibility. Dependency updates/security advisory audit and the runtime support policy must be checked for your deployment.
- The default bootstrap administrator in the original source had a hardcoded password. If that account was used, change its password before deployment. No original credentials or connection secrets are included in this deliverable.
