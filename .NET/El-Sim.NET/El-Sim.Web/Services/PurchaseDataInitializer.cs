namespace El_Sim.Web.Services;

public class PurchaseDataInitializer
{
    private readonly ElSimDbContext _dbContext;

    public PurchaseDataInitializer(ElSimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[ProductPurchases]', N'U') IS NULL
            BEGIN
                CREATE TABLE [ProductPurchases] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_ProductPurchases] PRIMARY KEY,
                    [UserId] int NOT NULL,
                    [ProductId] int NOT NULL,
                    [Category] nvarchar(32) NOT NULL,
                    [ProductType] nvarchar(32) NOT NULL CONSTRAINT [DF_ProductPurchases_ProductType] DEFAULT N'',
                    [ProductName] nvarchar(80) NOT NULL,
                    [PhoneNumber] nvarchar(32) NOT NULL CONSTRAINT [DF_ProductPurchases_PhoneNumber] DEFAULT N'',
                    [PhonePrefix] nvarchar(2) NOT NULL CONSTRAINT [DF_ProductPurchases_PhonePrefix] DEFAULT N'',
                    [ProductCurrency] nvarchar(3) NOT NULL,
                    [ProductAmount] decimal(18,2) NOT NULL,
                    [TotalAzn] decimal(18,2) NOT NULL,
                    [ExchangeRate] decimal(18,6) NULL,
                    [CommissionRate] decimal(8,4) NOT NULL,
                    [HasStaticIp] bit NOT NULL CONSTRAINT [DF_ProductPurchases_HasStaticIp] DEFAULT CAST(0 AS bit),
                    [StaticIpRate] decimal(8,4) NOT NULL CONSTRAINT [DF_ProductPurchases_StaticIpRate] DEFAULT 0,
                    [StaticIpFeeAzn] decimal(18,2) NOT NULL CONSTRAINT [DF_ProductPurchases_StaticIpFeeAzn] DEFAULT 0,
                    [Status] nvarchar(24) NOT NULL,
                    [CreatedAtUtc] datetime2 NOT NULL,
                    [CancelledAtUtc] datetime2 NULL,
                    [RefundedAtUtc] datetime2 NULL,
                    [AdminNote] nvarchar(260) NULL
                );
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[ProductPurchases]', N'ProductType') IS NULL
            BEGIN
                ALTER TABLE [ProductPurchases] ADD [ProductType] nvarchar(32) NOT NULL CONSTRAINT [DF_ProductPurchases_ProductType] DEFAULT N'';
            END
            IF COL_LENGTH(N'[ProductPurchases]', N'PhoneNumber') IS NULL
            BEGIN
                ALTER TABLE [ProductPurchases] ADD [PhoneNumber] nvarchar(32) NOT NULL CONSTRAINT [DF_ProductPurchases_PhoneNumber] DEFAULT N'';
            END
            IF COL_LENGTH(N'[ProductPurchases]', N'PhonePrefix') IS NULL
            BEGIN
                ALTER TABLE [ProductPurchases] ADD [PhonePrefix] nvarchar(2) NOT NULL CONSTRAINT [DF_ProductPurchases_PhonePrefix] DEFAULT N'';
            END
            IF COL_LENGTH(N'[ProductPurchases]', N'HasStaticIp') IS NULL
            BEGIN
                ALTER TABLE [ProductPurchases] ADD [HasStaticIp] bit NOT NULL CONSTRAINT [DF_ProductPurchases_HasStaticIp] DEFAULT CAST(0 AS bit);
            END
            IF COL_LENGTH(N'[ProductPurchases]', N'StaticIpRate') IS NULL
            BEGIN
                ALTER TABLE [ProductPurchases] ADD [StaticIpRate] decimal(8,4) NOT NULL CONSTRAINT [DF_ProductPurchases_StaticIpRate] DEFAULT 0;
            END
            IF COL_LENGTH(N'[ProductPurchases]', N'StaticIpFeeAzn') IS NULL
            BEGIN
                ALTER TABLE [ProductPurchases] ADD [StaticIpFeeAzn] decimal(18,2) NOT NULL CONSTRAINT [DF_ProductPurchases_StaticIpFeeAzn] DEFAULT 0;
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[WalletTransactions]', N'U') IS NULL
            BEGIN
                CREATE TABLE [WalletTransactions] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_WalletTransactions] PRIMARY KEY,
                    [UserId] int NOT NULL,
                    [ProductPurchaseId] int NULL,
                    [Type] nvarchar(24) NOT NULL,
                    [Status] nvarchar(24) NOT NULL,
                    [AmountAzn] decimal(18,2) NOT NULL,
                    [BalanceAfterAzn] decimal(18,2) NOT NULL,
                    [StripeSessionId] nvarchar(128) NULL,
                    [Description] nvarchar(260) NOT NULL,
                    [CreatedAtUtc] datetime2 NOT NULL
                );
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[PaymentReceipts]', N'U') IS NULL
            BEGIN
                CREATE TABLE [PaymentReceipts] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_PaymentReceipts] PRIMARY KEY,
                    [UserId] int NOT NULL,
                    [ProductPurchaseId] int NULL,
                    [WalletTransactionId] int NULL,
                    [ReceiptNumber] nvarchar(40) NOT NULL,
                    [Type] nvarchar(24) NOT NULL,
                    [Status] nvarchar(24) NOT NULL,
                    [Currency] nvarchar(3) NOT NULL,
                    [OriginalAmount] decimal(18,2) NOT NULL,
                    [AmountAzn] decimal(18,2) NOT NULL,
                    [ExchangeRate] decimal(18,6) NULL,
                    [CommissionRate] decimal(8,4) NOT NULL,
                    [Description] nvarchar(260) NOT NULL,
                    [PayloadJson] nvarchar(max) NOT NULL,
                    [CreatedAtUtc] datetime2 NOT NULL
                );
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[AppSettings]', N'U') IS NULL
            BEGIN
                CREATE TABLE [AppSettings] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_AppSettings] PRIMARY KEY,
                    [Key] nvarchar(80) NOT NULL,
                    [Value] nvarchar(260) NOT NULL
                );
            END
            """);

        await EnsureIndex("AppSettings", "IX_AppSettings_Key", "[Key]", true, null);
        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM [AppSettings] WHERE [Key] = N'WifiStaticIpPercent')
            BEGIN
                INSERT INTO [AppSettings] ([Key], [Value]) VALUES (N'WifiStaticIpPercent', N'5');
            END
            """);

        foreach (var prefix in PhoneNumberService.BasicPrefixes)
        {
            await EnsureSetting(PhoneNumberService.SettingKey(prefix, false), "5");
            await EnsureSetting(PhoneNumberService.CurrencySettingKey(prefix, false), "AZN");
        }

        await EnsureSetting(PhoneNumberService.SettingKey(PhoneNumberService.GlobalPrefix, true), "10");
        await EnsureSetting(PhoneNumberService.CurrencySettingKey(PhoneNumberService.GlobalPrefix, true), "AZN");

        await EnsureForeignKey("ProductPurchases", "Users", "FK_ProductPurchases_Users_UserId", "UserId", "Id", "CASCADE");
        await EnsureForeignKey("ProductPurchases", "Products", "FK_ProductPurchases_Products_ProductId", "ProductId", "Id", "NO ACTION");
        await EnsureForeignKey("WalletTransactions", "Users", "FK_WalletTransactions_Users_UserId", "UserId", "Id", "CASCADE");
        await EnsureForeignKey("WalletTransactions", "ProductPurchases", "FK_WalletTransactions_ProductPurchases_ProductPurchaseId", "ProductPurchaseId", "Id", "NO ACTION");
        await EnsureForeignKey("PaymentReceipts", "Users", "FK_PaymentReceipts_Users_UserId", "UserId", "Id", "CASCADE");
        await EnsureForeignKey("PaymentReceipts", "ProductPurchases", "FK_PaymentReceipts_ProductPurchases_ProductPurchaseId", "ProductPurchaseId", "Id", "NO ACTION");
        await EnsureForeignKey("PaymentReceipts", "WalletTransactions", "FK_PaymentReceipts_WalletTransactions_WalletTransactionId", "WalletTransactionId", "Id", "NO ACTION");

        await EnsureIndex("ProductPurchases", "IX_ProductPurchases_UserId_Category_Status", "[UserId], [Category], [Status]", false, null);
        await EnsureIndex("ProductPurchases", "IX_ProductPurchases_PhoneNumber", "[PhoneNumber]", true, "[PhoneNumber] <> N''");
        await EnsureIndex("WalletTransactions", "IX_WalletTransactions_UserId_CreatedAtUtc", "[UserId], [CreatedAtUtc]", false, null);
        await EnsureIndex("WalletTransactions", "IX_WalletTransactions_StripeSessionId", "[StripeSessionId]", true, "[StripeSessionId] IS NOT NULL");
        await EnsureIndex("PaymentReceipts", "IX_PaymentReceipts_ReceiptNumber", "[ReceiptNumber]", true, null);
        await EnsureIndex("PaymentReceipts", "IX_PaymentReceipts_UserId_CreatedAtUtc", "[UserId], [CreatedAtUtc]", false, null);
    }

    private async Task EnsureForeignKey(string table, string principalTable, string name, string column, string principalColumn, string onDelete)
    {
        var sql = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'{name}')
            BEGIN
                ALTER TABLE [{table}] ADD CONSTRAINT [{name}]
                    FOREIGN KEY ([{column}]) REFERENCES [{principalTable}] ([{principalColumn}]) ON DELETE {onDelete};
            END
            """;

        await _dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    private async Task EnsureIndex(string table, string name, string columns, bool unique, string? filter)
    {
        var uniqueSql = unique ? "UNIQUE " : string.Empty;
        var filterSql = string.IsNullOrWhiteSpace(filter) ? string.Empty : $" WHERE {filter}";
        var sql = $"""
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'{name}' AND [object_id] = OBJECT_ID(N'[{table}]'))
            BEGIN
                CREATE {uniqueSql}INDEX [{name}] ON [{table}] ({columns}){filterSql};
            END
            """;

        await _dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    private async Task EnsureSetting(string key, string value)
    {
        var sql = $"""
            IF NOT EXISTS (SELECT 1 FROM [AppSettings] WHERE [Key] = N'{key}')
            BEGIN
                INSERT INTO [AppSettings] ([Key], [Value]) VALUES (N'{key}', N'{value}');
            END
            """;

        await _dbContext.Database.ExecuteSqlRawAsync(sql);
    }
}
