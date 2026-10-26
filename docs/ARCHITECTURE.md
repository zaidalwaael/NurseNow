# Architecture decisions

| Project | Responsibilities | Project references |
|---|---|---|
| NurseNow.Domain | Existing entities, identity user shape, booking overlap rule | None |
| NurseNow.Application | DTOs, use-case services, application results, current-request/payment/file/token ports, context/UoW contract | Domain |
| NurseNow.Infrastructure | SQL Server context and migrations, Identity stores, JWT, SMTP, Stripe, file storage, seeding | Application |
| NurseNow.API | HTTP binding, authorization attributes, result mapping, error handler, authentication pipeline and composition root | Application, Infrastructure |

All 104 original active actions retain their HTTP verb, action route, controller name and class authorization. Business methods no longer inherit ControllerBase or return IActionResult. Their outcomes use OperationResult. The two originally commented-out controllers remain archived under docs; reports/settings were not silently enabled. The idle NotificationSchedulerService was removed because it did no work.

IApplicationDbContext acts as a scoped Unit of Work and exposes EF DbSet collections. Both this interface and Identity stores use the same scoped ApplicationDbContext. A generic repository wrapper was not added. Queries still execute on the database; SQL Server configuration resides in Infrastructure. Registration, admin creation/deletion and booking creation use explicit transactions. Booking/payment status and review flags use optimistic concurrency.

This is a pragmatic boundary, not framework-pure Clean Architecture: Domain's ApplicationUser still inherits IdentityUser, Application still references EF Core and Identity abstractions, and several feature services remain large. Separating Identity into a persistence-only representation and replacing every EF query with purpose-built repository/query interfaces would require a further design step. These dependencies are explicit, not disguised behind folder names.

The IDs and original entity namespaces were preserved to avoid breaking account references, EF snapshots or client contracts. Existing int business keys and string Identity IDs are unchanged. GUIDs are used only for file names, not new database identifiers.

The API's file endpoint and JWT validation perform short infrastructure-facing authorization checks. All original business controllers delegate to Application. HTTP-specific ClaimsPrincipal access is exposed through IRequestContext; the application does not depend on HttpContext or MVC.
