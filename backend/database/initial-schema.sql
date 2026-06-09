IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609171506_InitialCreate'
)
BEGIN
    CREATE TABLE [KnowledgeRecords] (
        [Id] uniqueidentifier NOT NULL,
        [Title] nvarchar(200) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Source] nvarchar(500) NULL,
        [Type] nvarchar(32) NOT NULL,
        [Status] nvarchar(16) NOT NULL,
        [AiStatus] nvarchar(16) NOT NULL,
        [Category] nvarchar(100) NULL,
        [Summary] nvarchar(4000) NULL,
        [Recommendations] nvarchar(max) NULL,
        [AiError] nvarchar(1000) NULL,
        [CreatedAtUtc] datetimeoffset(0) NOT NULL,
        [UpdatedAtUtc] datetimeoffset(0) NOT NULL,
        [ArchivedAtUtc] datetimeoffset(0) NULL,
        [AiProcessedAtUtc] datetimeoffset(0) NULL,
        CONSTRAINT [PK_KnowledgeRecords] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609171506_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KnowledgeRecords_AiStatus] ON [KnowledgeRecords] ([AiStatus]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609171506_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KnowledgeRecords_Category] ON [KnowledgeRecords] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609171506_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KnowledgeRecords_CreatedAtUtc] ON [KnowledgeRecords] ([CreatedAtUtc]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609171506_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KnowledgeRecords_Status] ON [KnowledgeRecords] ([Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609171506_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_KnowledgeRecords_Type] ON [KnowledgeRecords] ([Type]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260609171506_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260609171506_InitialCreate', N'9.0.9');
END;

COMMIT;
GO
