BEGIN TRANSACTION;
CREATE TABLE [SeparationLists] (
    [Id] uniqueidentifier NOT NULL,
    [FileName] nvarchar(500) NOT NULL,
    [UploadedAt] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_SeparationLists] PRIMARY KEY ([Id])
);

CREATE TABLE [SeparationItems] (
    [Id] uniqueidentifier NOT NULL,
    [SeparationListId] uniqueidentifier NOT NULL,
    [Sku] nvarchar(100) NOT NULL,
    [Color] nvarchar(100) NOT NULL,
    [Size] nvarchar(10) NOT NULL,
    [Quantity] int NOT NULL,
    [DtfModelId] uniqueidentifier NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    [IsDeleted] bit NOT NULL,
    CONSTRAINT [PK_SeparationItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SeparationItems_DtfModels_DtfModelId] FOREIGN KEY ([DtfModelId]) REFERENCES [DtfModels] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_SeparationItems_SeparationLists_SeparationListId] FOREIGN KEY ([SeparationListId]) REFERENCES [SeparationLists] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_SeparationItems_DtfModelId] ON [SeparationItems] ([DtfModelId]);

CREATE INDEX [IX_SeparationItems_SeparationListId] ON [SeparationItems] ([SeparationListId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260601190104_AddSeparationList', N'9.0.5');

COMMIT;
GO

