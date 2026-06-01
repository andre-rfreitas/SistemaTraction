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
    WHERE [MigrationId] = N'20260529122420_InitialCreate'
)
BEGIN
    CREATE TABLE [FabricTypes] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [Variation] nvarchar(100) NOT NULL,
        [PricePerKg] decimal(18,2) NOT NULL,
        [AverageKgPerRoll] decimal(18,3) NOT NULL,
        [AveragePiecesPerRoll] int NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FabricTypes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529122420_InitialCreate'
)
BEGIN
    CREATE TABLE [FabricColors] (
        [Id] uniqueidentifier NOT NULL,
        [FabricTypeId] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [HexCode] nvarchar(7) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FabricColors] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FabricColors_FabricTypes_FabricTypeId] FOREIGN KEY ([FabricTypeId]) REFERENCES [FabricTypes] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529122420_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_FabricColors_FabricTypeId] ON [FabricColors] ([FabricTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529122420_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FabricTypes_Name_Variation] ON [FabricTypes] ([Name], [Variation]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529122420_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529122420_InitialCreate', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529140425_AddDtfModel'
)
BEGIN
    CREATE TABLE [DtfModels] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [SheetLabel] nvarchar(50) NOT NULL,
        [StampsPerSheet] int NOT NULL,
        [SheetCost] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DtfModels] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529140425_AddDtfModel'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'IsDeleted', N'Name', N'SheetCost', N'SheetLabel', N'StampsPerSheet', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[DtfModels]'))
        SET IDENTITY_INSERT [DtfModels] ON;
    EXEC(N'INSERT INTO [DtfModels] ([Id], [CreatedAt], [IsDeleted], [Name], [SheetCost], [SheetLabel], [StampsPerSheet], [UpdatedAt])
    VALUES (''11111111-0001-0000-0000-000000000000'', ''2024-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''Angel'', 49.8, N''Folha 1'', 9, ''2024-01-01T00:00:00.0000000Z''),
    (''11111111-0002-0000-0000-000000000000'', ''2024-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''Flying Souls'', 49.8, N''Folha 2'', 6, ''2024-01-01T00:00:00.0000000Z''),
    (''11111111-0003-0000-0000-000000000000'', ''2024-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''Old Skull Tongue'', 49.8, N''Folha 3'', 21, ''2024-01-01T00:00:00.0000000Z''),
    (''11111111-0005-0000-0000-000000000000'', ''2024-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''Red Rage'', 49.8, N''Folha 5'', 4, ''2024-01-01T00:00:00.0000000Z''),
    (''11111111-0007-0000-0000-000000000000'', ''2024-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''Flaming Skull'', 49.8, N''Folha 7'', 8, ''2024-01-01T00:00:00.0000000Z''),
    (''11111111-0009-0000-0000-000000000000'', ''2024-01-01T00:00:00.0000000Z'', CAST(0 AS bit), N''Made in Traction'', 49.8, N''Folha 9'', 5, ''2024-01-01T00:00:00.0000000Z'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'IsDeleted', N'Name', N'SheetCost', N'SheetLabel', N'StampsPerSheet', N'UpdatedAt') AND [object_id] = OBJECT_ID(N'[DtfModels]'))
        SET IDENTITY_INSERT [DtfModels] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529140425_AddDtfModel'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DtfModels_Name] ON [DtfModels] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529140425_AddDtfModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529140425_AddDtfModel', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529163252_AddDtfStock'
)
BEGIN
    CREATE TABLE [DtfStockItems] (
        [Id] uniqueidentifier NOT NULL,
        [DtfModelId] uniqueidentifier NOT NULL,
        [CurrentQuantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DtfStockItems] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DtfStockItems_DtfModels_DtfModelId] FOREIGN KEY ([DtfModelId]) REFERENCES [DtfModels] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529163252_AddDtfStock'
)
BEGIN
    CREATE TABLE [DtfStockMovements] (
        [Id] uniqueidentifier NOT NULL,
        [DtfStockItemId] uniqueidentifier NOT NULL,
        [Type] int NOT NULL,
        [Delta] int NOT NULL,
        [Reason] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DtfStockMovements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DtfStockMovements_DtfStockItems_DtfStockItemId] FOREIGN KEY ([DtfStockItemId]) REFERENCES [DtfStockItems] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529163252_AddDtfStock'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DtfStockItems_DtfModelId] ON [DtfStockItems] ([DtfModelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529163252_AddDtfStock'
)
BEGIN
    CREATE INDEX [IX_DtfStockMovements_DtfStockItemId] ON [DtfStockMovements] ([DtfStockItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529163252_AddDtfStock'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529163252_AddDtfStock', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529164714_AddAppConfig'
)
BEGIN
    CREATE TABLE [AppConfigs] (
        [Id] uniqueidentifier NOT NULL,
        [Key] nvarchar(100) NOT NULL,
        [Value] nvarchar(1000) NOT NULL,
        [Description] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_AppConfigs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529164714_AddAppConfig'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsDeleted', N'Key', N'UpdatedAt', N'Value') AND [object_id] = OBJECT_ID(N'[AppConfigs]'))
        SET IDENTITY_INSERT [AppConfigs] ON;
    EXEC(N'INSERT INTO [AppConfigs] ([Id], [CreatedAt], [Description], [IsDeleted], [Key], [UpdatedAt], [Value])
    VALUES (''aaaaaaaa-0000-0000-0000-000011110001'', ''2024-01-01T00:00:00.0000000Z'', N''Quantidade mínima de folhas por modelo antes de alertar reposição'', CAST(0 AS bit), N''dtf.alerta_estoque_minimo'', ''2024-01-01T00:00:00.0000000Z'', N''3''),
    (''aaaaaaaa-0000-0000-0000-000011110002'', ''2024-01-01T00:00:00.0000000Z'', N''Custo padrão de uma folha DTF em reais'', CAST(0 AS bit), N''dtf.custo_folha_padrao'', ''2024-01-01T00:00:00.0000000Z'', N''49.80''),
    (''aaaaaaaa-0000-0000-0000-000011110003'', ''2024-01-01T00:00:00.0000000Z'', N''Prazo médio em dias para recebimento de pedidos a fornecedores'', CAST(0 AS bit), N''pedido.lead_time_padrao_dias'', ''2024-01-01T00:00:00.0000000Z'', N''7''),
    (''aaaaaaaa-0000-0000-0000-000011110004'', ''2024-01-01T00:00:00.0000000Z'', N''Quantidade mínima em kg de tecido antes de alertar reposição'', CAST(0 AS bit), N''estoque.tecido.alerta_minimo_kg'', ''2024-01-01T00:00:00.0000000Z'', N''5''),
    (''aaaaaaaa-0000-0000-0000-000011110005'', ''2024-01-01T00:00:00.0000000Z'', N''Fuso horário padrão do sistema'', CAST(0 AS bit), N''sistema.timezone'', ''2024-01-01T00:00:00.0000000Z'', N''America/Sao_Paulo''),
    (''aaaaaaaa-0000-0000-0000-000011110006'', ''2024-01-01T00:00:00.0000000Z'', N''Moeda padrão do sistema'', CAST(0 AS bit), N''sistema.moeda'', ''2024-01-01T00:00:00.0000000Z'', N''BRL'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsDeleted', N'Key', N'UpdatedAt', N'Value') AND [object_id] = OBJECT_ID(N'[AppConfigs]'))
        SET IDENTITY_INSERT [AppConfigs] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529164714_AddAppConfig'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AppConfigs_Key] ON [AppConfigs] ([Key]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529164714_AddAppConfig'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529164714_AddAppConfig', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529172731_ReplaceAppConfigSeed'
)
BEGIN
    EXEC(N'DELETE FROM [AppConfigs]
    WHERE [Id] = ''aaaaaaaa-0000-0000-0000-000011110001'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529172731_ReplaceAppConfigSeed'
)
BEGIN
    EXEC(N'DELETE FROM [AppConfigs]
    WHERE [Id] = ''aaaaaaaa-0000-0000-0000-000011110002'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529172731_ReplaceAppConfigSeed'
)
BEGIN
    EXEC(N'DELETE FROM [AppConfigs]
    WHERE [Id] = ''aaaaaaaa-0000-0000-0000-000011110003'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529172731_ReplaceAppConfigSeed'
)
BEGIN
    EXEC(N'DELETE FROM [AppConfigs]
    WHERE [Id] = ''aaaaaaaa-0000-0000-0000-000011110004'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529172731_ReplaceAppConfigSeed'
)
BEGIN
    EXEC(N'DELETE FROM [AppConfigs]
    WHERE [Id] = ''aaaaaaaa-0000-0000-0000-000011110005'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529172731_ReplaceAppConfigSeed'
)
BEGIN
    EXEC(N'DELETE FROM [AppConfigs]
    WHERE [Id] = ''aaaaaaaa-0000-0000-0000-000011110006'';
    SELECT @@ROWCOUNT');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529172731_ReplaceAppConfigSeed'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsDeleted', N'Key', N'UpdatedAt', N'Value') AND [object_id] = OBJECT_ID(N'[AppConfigs]'))
        SET IDENTITY_INSERT [AppConfigs] ON;
    EXEC(N'INSERT INTO [AppConfigs] ([Id], [CreatedAt], [Description], [IsDeleted], [Key], [UpdatedAt], [Value])
    VALUES (''cccccccc-0000-0000-0000-000000000001'', ''2024-01-01T00:00:00.0000000Z'', N''Preço padrão de corte por peça (R$)'', CAST(0 AS bit), N''cutting_price_default'', ''2024-01-01T00:00:00.0000000Z'', N''1.00''),
    (''cccccccc-0000-0000-0000-000000000002'', ''2024-01-01T00:00:00.0000000Z'', N''Preço padrão de costura por peça — tamanhos P/M/G/GG (R$)'', CAST(0 AS bit), N''sewing_price_default'', ''2024-01-01T00:00:00.0000000Z'', N''5.60''),
    (''cccccccc-0000-0000-0000-000000000003'', ''2024-01-01T00:00:00.0000000Z'', N''Preço de costura por peça — tamanho G1 (R$)'', CAST(0 AS bit), N''sewing_price_g1'', ''2024-01-01T00:00:00.0000000Z'', N''6.30''),
    (''cccccccc-0000-0000-0000-000000000004'', ''2024-01-01T00:00:00.0000000Z'', N''Preço padrão por folha DTF (R$)'', CAST(0 AS bit), N''dtf_sheet_price_default'', ''2024-01-01T00:00:00.0000000Z'', N''49.80''),
    (''cccccccc-0000-0000-0000-000000000005'', ''2024-01-01T00:00:00.0000000Z'', N''Quantidade mínima em estoque antes de disparar alerta de reposição'', CAST(0 AS bit), N''stock_alert_threshold'', ''2024-01-01T00:00:00.0000000Z'', N''15''),
    (''cccccccc-0000-0000-0000-000000000006'', ''2024-01-01T00:00:00.0000000Z'', N''Dias de histórico usados para calcular recomendação de corte'', CAST(0 AS bit), N''recommendation_days'', ''2024-01-01T00:00:00.0000000Z'', N''30''),
    (''cccccccc-0000-0000-0000-000000000007'', ''2024-01-01T00:00:00.0000000Z'', N''Tamanhos disponíveis para produção, separados por vírgula'', CAST(0 AS bit), N''sizes_available'', ''2024-01-01T00:00:00.0000000Z'', N''P,M,G,G1,GG'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsDeleted', N'Key', N'UpdatedAt', N'Value') AND [object_id] = OBJECT_ID(N'[AppConfigs]'))
        SET IDENTITY_INSERT [AppConfigs] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529172731_ReplaceAppConfigSeed'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529172731_ReplaceAppConfigSeed', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529180354_AddFabricRollAndFinancialEntry'
)
BEGIN
    CREATE TABLE [FabricRolls] (
        [Id] uniqueidentifier NOT NULL,
        [FabricTypeId] uniqueidentifier NOT NULL,
        [FabricColorId] uniqueidentifier NOT NULL,
        [WeightKg] decimal(18,3) NOT NULL,
        [PriceTotal] decimal(18,2) NOT NULL,
        [PricePerKgActual] decimal(18,4) NOT NULL,
        [ReceivedAt] datetime2 NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FabricRolls] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_FabricRolls_FabricColors_FabricColorId] FOREIGN KEY ([FabricColorId]) REFERENCES [FabricColors] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_FabricRolls_FabricTypes_FabricTypeId] FOREIGN KEY ([FabricTypeId]) REFERENCES [FabricTypes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529180354_AddFabricRollAndFinancialEntry'
)
BEGIN
    CREATE TABLE [FinancialEntries] (
        [Id] uniqueidentifier NOT NULL,
        [Type] nvarchar(max) NOT NULL,
        [Category] nvarchar(100) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [ReferenceId] uniqueidentifier NULL,
        [ReferenceType] nvarchar(100) NULL,
        [EntryDate] datetime2 NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_FinancialEntries] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529180354_AddFabricRollAndFinancialEntry'
)
BEGIN
    CREATE INDEX [IX_FabricRolls_FabricColorId] ON [FabricRolls] ([FabricColorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529180354_AddFabricRollAndFinancialEntry'
)
BEGIN
    CREATE INDEX [IX_FabricRolls_FabricTypeId] ON [FabricRolls] ([FabricTypeId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529180354_AddFabricRollAndFinancialEntry'
)
BEGIN
    CREATE INDEX [IX_FinancialEntries_Category] ON [FinancialEntries] ([Category]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529180354_AddFabricRollAndFinancialEntry'
)
BEGIN
    CREATE INDEX [IX_FinancialEntries_EntryDate] ON [FinancialEntries] ([EntryDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529180354_AddFabricRollAndFinancialEntry'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529180354_AddFabricRollAndFinancialEntry', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529183938_AddCuttingOrder'
)
BEGIN
    CREATE TABLE [CuttingOrders] (
        [Id] uniqueidentifier NOT NULL,
        [OrderNumber] int NOT NULL,
        [FabricRollId] uniqueidentifier NOT NULL,
        [RequestedPiecesJson] nvarchar(max) NOT NULL,
        [Status] nvarchar(max) NOT NULL,
        [SentAt] datetime2 NULL,
        [Notes] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CuttingOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CuttingOrders_FabricRolls_FabricRollId] FOREIGN KEY ([FabricRollId]) REFERENCES [FabricRolls] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529183938_AddCuttingOrder'
)
BEGIN
    CREATE INDEX [IX_CuttingOrders_FabricRollId] ON [CuttingOrders] ([FabricRollId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529183938_AddCuttingOrder'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CuttingOrders_OrderNumber] ON [CuttingOrders] ([OrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529183938_AddCuttingOrder'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529183938_AddCuttingOrder', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529184016_AddWhatsAppAppConfig'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsDeleted', N'Key', N'UpdatedAt', N'Value') AND [object_id] = OBJECT_ID(N'[AppConfigs]'))
        SET IDENTITY_INSERT [AppConfigs] ON;
    EXEC(N'INSERT INTO [AppConfigs] ([Id], [CreatedAt], [Description], [IsDeleted], [Key], [UpdatedAt], [Value])
    VALUES (''dddddddd-0000-0000-0000-000000000001'', ''2024-01-01T00:00:00.0000000Z'', N''Número de WhatsApp do cortador (com DDI, ex: 5511999999999)'', CAST(0 AS bit), N''wp_cutter_phone'', ''2024-01-01T00:00:00.0000000Z'', N''''),
    (''dddddddd-0000-0000-0000-000000000002'', ''2024-01-01T00:00:00.0000000Z'', N''Nome do cortador exibido na tela de revisão'', CAST(0 AS bit), N''wp_cutter_name'', ''2024-01-01T00:00:00.0000000Z'', N''Cortador''),
    (''dddddddd-0000-0000-0000-000000000003'', ''2024-01-01T00:00:00.0000000Z'', N''Número de WhatsApp do costureiro (com DDI)'', CAST(0 AS bit), N''wp_sewer_phone'', ''2024-01-01T00:00:00.0000000Z'', N''''),
    (''dddddddd-0000-0000-0000-000000000004'', ''2024-01-01T00:00:00.0000000Z'', N''Nome do costureiro exibido na tela de revisão'', CAST(0 AS bit), N''wp_sewer_name'', ''2024-01-01T00:00:00.0000000Z'', N''Costureiro''),
    (''dddddddd-0000-0000-0000-000000000005'', ''2024-01-01T00:00:00.0000000Z'', N''Número de WhatsApp do fornecedor DTF (com DDI)'', CAST(0 AS bit), N''wp_dtf_phone'', ''2024-01-01T00:00:00.0000000Z'', N''''),
    (''dddddddd-0000-0000-0000-000000000006'', ''2024-01-01T00:00:00.0000000Z'', N''Nome do fornecedor DTF exibido na tela de revisão'', CAST(0 AS bit), N''wp_dtf_name'', ''2024-01-01T00:00:00.0000000Z'', N''Fornecedor DTF'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedAt', N'Description', N'IsDeleted', N'Key', N'UpdatedAt', N'Value') AND [object_id] = OBJECT_ID(N'[AppConfigs]'))
        SET IDENTITY_INSERT [AppConfigs] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529184016_AddWhatsAppAppConfig'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529184016_AddWhatsAppAppConfig', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529190552_AddCuttingDelivery'
)
BEGIN
    CREATE TABLE [CuttingDeliveries] (
        [Id] uniqueidentifier NOT NULL,
        [CuttingOrderId] uniqueidentifier NOT NULL,
        [DeliveredPiecesJson] nvarchar(max) NOT NULL,
        [DeliveredAt] datetime2 NOT NULL,
        [CuttingCostTotal] decimal(18,2) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_CuttingDeliveries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CuttingDeliveries_CuttingOrders_CuttingOrderId] FOREIGN KEY ([CuttingOrderId]) REFERENCES [CuttingOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529190552_AddCuttingDelivery'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CuttingDeliveries_CuttingOrderId] ON [CuttingDeliveries] ([CuttingOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260529190552_AddCuttingDelivery'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260529190552_AddCuttingDelivery', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601131541_AddSewingDeliveryAndStock'
)
BEGIN
    CREATE TABLE [SewingDeliveries] (
        [Id] uniqueidentifier NOT NULL,
        [CuttingOrderId] uniqueidentifier NOT NULL,
        [GoodPiecesJson] nvarchar(max) NOT NULL,
        [DefectivePiecesJson] nvarchar(max) NOT NULL,
        [DeliveredAt] datetime2 NOT NULL,
        [SewingCostTotal] decimal(18,4) NOT NULL,
        [DefectCostTotal] decimal(18,4) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SewingDeliveries] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SewingDeliveries_CuttingOrders_CuttingOrderId] FOREIGN KEY ([CuttingOrderId]) REFERENCES [CuttingOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601131541_AddSewingDeliveryAndStock'
)
BEGIN
    CREATE TABLE [StockItems] (
        [Id] uniqueidentifier NOT NULL,
        [FabricColorId] uniqueidentifier NOT NULL,
        [FabricColorName] nvarchar(100) NOT NULL,
        [FabricTypeName] nvarchar(100) NOT NULL,
        [FabricTypeVariation] nvarchar(100) NOT NULL,
        [Size] nvarchar(10) NOT NULL,
        [Quantity] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_StockItems] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601131541_AddSewingDeliveryAndStock'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SewingDeliveries_CuttingOrderId] ON [SewingDeliveries] ([CuttingOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601131541_AddSewingDeliveryAndStock'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockItems_FabricColorId_Size] ON [StockItems] ([FabricColorId], [Size]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601131541_AddSewingDeliveryAndStock'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260601131541_AddSewingDeliveryAndStock', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601190104_AddSeparationList'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601190104_AddSeparationList'
)
BEGIN
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
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601190104_AddSeparationList'
)
BEGIN
    CREATE INDEX [IX_SeparationItems_DtfModelId] ON [SeparationItems] ([DtfModelId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601190104_AddSeparationList'
)
BEGIN
    CREATE INDEX [IX_SeparationItems_SeparationListId] ON [SeparationItems] ([SeparationListId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601190104_AddSeparationList'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260601190104_AddSeparationList', N'9.0.5');
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601195427_AddSkuCode'
)
BEGIN
    CREATE TABLE [SkuCodes] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Value] nvarchar(200) NOT NULL,
        [Category] nvarchar(20) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SkuCodes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260601195427_AddSkuCode'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260601195427_AddSkuCode', N'9.0.5');
END;

COMMIT;
GO

