# Repository and Unit of Work refactor — 2026-11-24

- Replaced IApplicationDbContext/DbSet access with 14 explicit repository interfaces in Application and their EF Core implementations in Infrastructure.
- Moved LINQ persistence queries out of all application services; consolidated repeated queries and added named read projections.
- Added scoped IUnitOfWork/UnitOfWork and application-owned transaction contracts. All business repositories and Identity user writes share one context.
- Removed EF Core and ASP.NET Identity dependencies from Domain/Application. Kept Microsoft UserManager behind IIdentityService and implemented a staged infrastructure user store.
- Preserved the Identity schema and existing migrations; the SQL Server model comparison finds no differences.
- Routed file ownership checks and JWT account validation through application interfaces.
- Updated regressions, architecture documentation and CI. See VALIDATION.md for 60 passing checks and the limits of validation.

---

# Changes and rationale

- Split the single web project into four projects with inward project references and explicit DI registration.
- Moved DTOs and 104 action implementations into Application; moved context, migrations, SMTP, JWT, Stripe and filesystem operations into Infrastructure.
- Preserved 104 original action routes and authorization attributes. Added protected file downloads and `/health/live`.
- Added cancellation to EF async query/save calls. Read-only actions use AsNoTracking; write paths retain tracking.
- Available-date lookup fetches overrides and weekly days once per request instead of querying on every loop iteration (roughly 3 queries total including nurse existence, rather than 1 + up to 2 per day).
- Bulk notification read uses a single scoped SQL UPDATE, with UserId in the predicate.
- Payment statistics aggregate by status in SQL rather than loading every payment entity.
- Nurse search paging is bounded, with a stable user-ID tie-breaker. daysAhead is bounded to 90. Existing opt-in appointment limit is capped at 100.
- Booking overlap detects partial intersections, ignores cancelled/rejected bookings, checks duration and past dates, and wraps conflict check plus writes in a serializable transaction. Available slots exclude reserved intervals. Adjacent bookings remain allowed.
- Added optimistic concurrency for booking/payment transitions and review flags, plus a unique review-per-booking index.
- Registration only accepts Patient/Nurse. Identity checks unique email; new passwords require 12 characters. Login lockout is enabled and auth endpoints are rate-limited by IP.
- JWT validation checks current account status, security stamp and admin role. SuperAdmin checks use the role claim, not a hardcoded email address.
- Removed hardcoded bootstrap credentials. Seeding is opt-in and uses external configuration.
- Payment intent IDs are persisted; repeat requests reuse the stored intent. Confirmation retrieves provider status and validates currency, amount, booking and patient metadata before updating payment and booking. The existing USD currency is retained.
- Files use private storage. Download authorization distinguishes profile images from certificates/IDs; private files require owner or administrator. Legacy `/uploads/{name}` URLs are routed through authorization. Added type/size checks and bounded streaming for uploads.
- Added centralized ProblemDetails for unexpected failures and concurrency conflicts, trace IDs, development-only Swagger, explicit CORS origins, HTTPS redirection and validated startup configuration.
- Preserved old migrations; added one migration and reviewed SQL. New constraints stop on incompatible existing data rather than silently deleting it.

No benchmark percentage is claimed. The query-count and allocation changes are structural improvements; production latency, throughput and execution plans still require representative SQL Server measurements.

Reference material consulted:
- https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying
- https://learn.microsoft.com/en-us/ef/core/saving/execute-insert-update-delete
