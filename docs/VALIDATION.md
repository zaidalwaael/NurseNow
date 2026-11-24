# Validation performed

Repository/Unit of Work refactor verified on 2026-11-24 using .NET SDK 9.0.318 and ASP.NET Core runtime 8.0.34. All projects continue to target net8.0. The SDK version comes from the repository's existing global.json.

- Release build succeeded for all four production projects and the regression executable with zero warnings and zero errors.
- All 60 regression checks passed. Exact output: `docs/test-results.txt`.
- The regression suite checks all 104 legacy HTTP actions and class authorization; all 23 application services resolve using production dependency-injection registrations.
- Domain and Application assembly references are checked for EF Core/ASP.NET dependencies. Repository contracts are checked for IQueryable/DbSet leakage.
- Relational SQLite checks exercise repository staging, cross-repository saves, rollback, immediate bulk-update rollback, cancellation, tracked writes/no-tracking reads, filtering, pagination, projections, booking conflicts, payments, file ownership and optimistic concurrency.
- Identity checks exercise actual UserManager hashing/validation, staged creation and role assignment, profile persistence, rollback, password-reset tokens/security stamps, lockout, login counters, updates and cascading deletion. AuthService registration/login/reset flows are tested through the new interfaces.
- The SQL Server relational model has **zero differences** from the existing migration snapshot. No new migration is introduced by this refactor, and no database was upgraded.
- Started the real API host in Development with isolated test configuration and seeding disabled. `/health/live` returned 200; `/swagger/v1/swagger.json` returned 200 with 99 distinct paths; anonymous requests to `/api/Notification` and `/api/files/document.pdf` returned 401. The test host was stopped afterwards.
- Added `.github/workflows/backend.yml` to build and run the regression executable on pushes and pull requests. Local results do not imply the remote workflow has already run.

The test executable uses SQLite, not EF InMemory, with TimeSpan converted to ticks in its fixture. SQL Server retains its native time mapping. Payments use a fake gateway. The SQL Server model comparison validates metadata only, without opening a SQL Server connection.

Not performed: live SQL Server execution or migration replay, concurrent-load testing against SQL Server, Flutter/frontend end-to-end testing, live Stripe payments, SMTP delivery or performance benchmarks. Route/authorization and selected JSON projections are covered; full HTTP payload compatibility for every action is not exhaustively tested.

Run from the solution root:

```powershell
dotnet restore NurseNow.sln
dotnet build NurseNow.sln --configuration Release
dotnet run --project tests/NurseNow.RegressionTests --configuration Release --no-build -- docs/endpoint-baseline.json
```

The tests are a console executable, not xUnit: a failed check exits with an error. `dotnet test` alone does not execute them. The restricted validation host used serial MSBuild (`-m:1 -nr:false -p:UseSharedCompilation=false`); normal local/CI builds can use the commands above.
