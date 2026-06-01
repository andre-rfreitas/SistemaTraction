BEGIN TRANSACTION;
ALTER TABLE [SkuCodes] ADD [DtfModelId] uniqueidentifier NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260601201617_AddSkuCodeDtfModelId', N'9.0.5');

COMMIT;
GO

