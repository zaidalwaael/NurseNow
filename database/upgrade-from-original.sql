BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    IF EXISTS (SELECT 1 FROM WeeklyAvailabilities WHERE LEN(DayOfWeek) > 9)
        THROW 51000, 'Review invalid DayOfWeek values before migration.', 1;
    IF EXISTS (SELECT BookingId FROM Reviews GROUP BY BookingId HAVING COUNT(*) > 1)
        THROW 51001, 'Review duplicate booking reviews before migration.', 1;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    DROP INDEX [IX_WeeklyAvailabilities_NurseId] ON [WeeklyAvailabilities];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    DROP INDEX [IX_Reviews_BookingId] ON [Reviews];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    DROP INDEX [IX_Notifications_UserId] ON [Notifications];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    DROP INDEX [IX_Bookings_NurseId] ON [Bookings];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    DROP INDEX [IX_AvailabilityOverrides_NurseId] ON [AvailabilityOverrides];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[WeeklyAvailabilities]') AND [c].[name] = N'DayOfWeek');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [WeeklyAvailabilities] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [WeeklyAvailabilities] ALTER COLUMN [DayOfWeek] nvarchar(9) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    ALTER TABLE [Payments] ADD [StripePaymentIntentId] nvarchar(255) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    CREATE INDEX [IX_WeeklyAvailabilities_NurseId_DayOfWeek_IsActive] ON [WeeklyAvailabilities] ([NurseId], [DayOfWeek], [IsActive]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Reviews_BookingId] ON [Reviews] ([BookingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    CREATE INDEX [IX_Notifications_UserId_IsRead_CreatedAt] ON [Notifications] ([UserId], [IsRead], [CreatedAt]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    CREATE INDEX [IX_Bookings_NurseId_BookingDate_StartTime] ON [Bookings] ([NurseId], [BookingDate], [StartTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    CREATE INDEX [IX_AvailabilityOverrides_NurseId_Date] ON [AvailabilityOverrides] ([NurseId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261020222223_CleanArchitectureHardening'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261020222223_CleanArchitectureHardening', N'8.0.0');
END;
GO

COMMIT;
GO

