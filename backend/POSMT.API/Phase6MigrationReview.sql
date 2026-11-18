BEGIN TRANSACTION;
ALTER TABLE [Sales] DROP CONSTRAINT [CK_Sales_Amounts];

ALTER TABLE [SaleItems] DROP CONSTRAINT [CK_SaleItems_Quantity];

ALTER TABLE [Users] ADD [TerminalId] int NULL;

DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Payments]') AND [c].[name] = N'PaidAt');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Payments] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Payments] ALTER COLUMN [PaidAt] datetime2 NULL;

ALTER TABLE [Payments] ADD [PaymentStatus] nvarchar(20) NOT NULL DEFAULT N'Pending';

CREATE INDEX [IX_Users_TerminalId] ON [Users] ([TerminalId]);

ALTER TABLE [Terminals] ADD CONSTRAINT [CK_Terminals_Status] CHECK ([Status] IN ('Active', 'Inactive'));

ALTER TABLE [Sales] ADD CONSTRAINT [CK_Sales_Amounts] CHECK ([Subtotal] >= 0 AND [Discount] >= 0 AND [Discount] <= [Subtotal] AND [TotalAmount] = [Subtotal] - [Discount]);

ALTER TABLE [Sales] ADD CONSTRAINT [CK_Sales_Status] CHECK ([Status] IN ('Pending', 'Completed', 'Cancelled'));

ALTER TABLE [SaleItems] ADD CONSTRAINT [CK_SaleItems_Quantity] CHECK ([Quantity] > 0 AND [UnitPrice] >= 0 AND [LineTotal] >= 0 AND [LineTotal] = [Quantity] * [UnitPrice]);

ALTER TABLE [Payments] ADD CONSTRAINT [CK_Payments_Amount] CHECK ([Amount] >= 0);

ALTER TABLE [Payments] ADD CONSTRAINT [CK_Payments_Method] CHECK ([PaymentMethod] IN ('Cash', 'Card'));

ALTER TABLE [Payments] ADD CONSTRAINT [CK_Payments_Status] CHECK ([PaymentStatus] IN ('Pending', 'Completed', 'Failed', 'Refunded'));

ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Terminals_TerminalId] FOREIGN KEY ([TerminalId]) REFERENCES [Terminals] ([TerminalId]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261118184115_Phase6MultiTerminalSales', N'10.0.15');

COMMIT;
GO

