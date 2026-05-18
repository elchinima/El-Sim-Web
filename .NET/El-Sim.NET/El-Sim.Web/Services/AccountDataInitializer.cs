namespace El_Sim.Web.Services;

public class AccountDataInitializer
{
    private readonly ElSimDbContext _dbContext;

    public AccountDataInitializer(ElSimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[Account]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Account] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Account] PRIMARY KEY,
                    [Email] nvarchar(254) NULL,
                    [ProfileImagePath] nvarchar(260) NULL,
                    [IsTwoFactorEnabled] bit NOT NULL CONSTRAINT [DF_Account_IsTwoFactorEnabled] DEFAULT CAST(0 AS bit),
                    [IsEmailNotificationsEnabled] bit NOT NULL CONSTRAINT [DF_Account_IsEmailNotificationsEnabled] DEFAULT CAST(0 AS bit),
                    [IsAdmin] bit NOT NULL CONSTRAINT [DF_Account_IsAdmin] DEFAULT CAST(0 AS bit),
                    [IsBlocked] bit NOT NULL CONSTRAINT [DF_Account_IsBlocked] DEFAULT CAST(0 AS bit)
                );
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[UserAssets]', N'U') IS NULL
            BEGIN
                CREATE TABLE [UserAssets] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_UserAssets] PRIMARY KEY,
                    [BasicNumber] nvarchar(32) NULL,
                    [GlobalNumber] nvarchar(32) NULL,
                    [Pass] nvarchar(80) NULL,
                    [BasicTariff] nvarchar(80) NULL,
                    [GlobalTariff] nvarchar(80) NULL,
                    [WiFi] nvarchar(80) NULL
                );
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[Users]', N'UserAssetsId') IS NULL
            BEGIN
                ALTER TABLE [Users] ADD [UserAssetsId] int NULL;
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Users_UserAssetsId' AND [object_id] = OBJECT_ID(N'[Users]'))
            BEGIN
                CREATE UNIQUE INDEX [IX_Users_UserAssetsId] ON [Users] ([UserAssetsId]) WHERE [UserAssetsId] IS NOT NULL;
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Users_UserAssets_UserAssetsId')
            BEGIN
                ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_UserAssets_UserAssetsId]
                    FOREIGN KEY ([UserAssetsId]) REFERENCES [UserAssets] ([Id]) ON DELETE SET NULL;
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[Users]', N'AccountId') IS NULL
            BEGIN
                ALTER TABLE [Users] ADD [AccountId] int NULL;
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            DECLARE @EmailExpression nvarchar(max) = CASE WHEN COL_LENGTH(N'[Users]', N'Email') IS NULL THEN N'NULL' ELSE N'[Email]' END;
            DECLARE @ProfileImagePathExpression nvarchar(max) = CASE WHEN COL_LENGTH(N'[Users]', N'ProfileImagePath') IS NULL THEN N'NULL' ELSE N'[ProfileImagePath]' END;
            DECLARE @IsTwoFactorEnabledExpression nvarchar(max) = CASE WHEN COL_LENGTH(N'[Users]', N'IsTwoFactorEnabled') IS NULL THEN N'CAST(0 AS bit)' ELSE N'[IsTwoFactorEnabled]' END;
            DECLARE @IsEmailNotificationsEnabledExpression nvarchar(max) = CASE WHEN COL_LENGTH(N'[Users]', N'IsEmailNotificationsEnabled') IS NULL THEN N'CAST(0 AS bit)' ELSE N'[IsEmailNotificationsEnabled]' END;
            DECLARE @IsAdminExpression nvarchar(max) = CASE WHEN COL_LENGTH(N'[Users]', N'IsAdmin') IS NULL THEN N'CAST(0 AS bit)' ELSE N'[IsAdmin]' END;
            DECLARE @IsBlockedExpression nvarchar(max) = CASE WHEN COL_LENGTH(N'[Users]', N'IsBlocked') IS NULL THEN N'CAST(0 AS bit)' ELSE N'[IsBlocked]' END;
            DECLARE @Sql nvarchar(max) = N'
                DECLARE @CreatedAccounts TABLE ([UserId] int NOT NULL, [AccountId] int NOT NULL);

                MERGE [Account] AS [Target]
                USING (
                    SELECT
                        [Id],
                        ' + @EmailExpression + N' AS [Email],
                        ' + @ProfileImagePathExpression + N' AS [ProfileImagePath],
                        ' + @IsTwoFactorEnabledExpression + N' AS [IsTwoFactorEnabled],
                        ' + @IsEmailNotificationsEnabledExpression + N' AS [IsEmailNotificationsEnabled],
                        ' + @IsAdminExpression + N' AS [IsAdmin],
                        ' + @IsBlockedExpression + N' AS [IsBlocked]
                    FROM [Users]
                    WHERE [AccountId] IS NULL
                ) AS [Source]
                ON 1 = 0
                WHEN NOT MATCHED THEN
                    INSERT ([Email], [ProfileImagePath], [IsTwoFactorEnabled], [IsEmailNotificationsEnabled], [IsAdmin], [IsBlocked])
                    VALUES ([Source].[Email], [Source].[ProfileImagePath], [Source].[IsTwoFactorEnabled], [Source].[IsEmailNotificationsEnabled], [Source].[IsAdmin], [Source].[IsBlocked])
                OUTPUT [Source].[Id], inserted.[Id] INTO @CreatedAccounts;

                UPDATE [Users]
                SET [AccountId] = [CreatedAccounts].[AccountId]
                FROM [Users]
                INNER JOIN @CreatedAccounts AS [CreatedAccounts] ON [Users].[Id] = [CreatedAccounts].[UserId];';

            EXEC sp_executesql @Sql;
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[Users]', N'AccountId') IS NOT NULL
                AND COLUMNPROPERTY(OBJECT_ID(N'[Users]'), N'AccountId', 'AllowsNull') = 1
                AND NOT EXISTS (SELECT 1 FROM [Users] WHERE [AccountId] IS NULL)
            BEGIN
                ALTER TABLE [Users] ALTER COLUMN [AccountId] int NOT NULL;
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE [name] = N'IX_Users_AccountId' AND [object_id] = OBJECT_ID(N'[Users]'))
            BEGIN
                CREATE UNIQUE INDEX [IX_Users_AccountId] ON [Users] ([AccountId]);
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE [name] = N'FK_Users_Account_AccountId')
            BEGIN
                ALTER TABLE [Users] ADD CONSTRAINT [FK_Users_Account_AccountId]
                    FOREIGN KEY ([AccountId]) REFERENCES [Account] ([Id]) ON DELETE CASCADE;
            END
            """);

        foreach (var column in UserAccountColumns())
        {
            await DropUserColumn(column);
        }
    }

    private async Task DropUserColumn(string column)
    {
        var sql = $"""
            IF COL_LENGTH(N'[Users]', N'{column}') IS NOT NULL
            BEGIN
                DECLARE @ConstraintName sysname;

                SELECT @ConstraintName = [default_constraints].[name]
                FROM sys.default_constraints
                INNER JOIN sys.columns ON [default_constraints].[parent_object_id] = [columns].[object_id]
                    AND [default_constraints].[parent_column_id] = [columns].[column_id]
                WHERE [default_constraints].[parent_object_id] = OBJECT_ID(N'[Users]')
                    AND [columns].[name] = N'{column}';

                IF @ConstraintName IS NOT NULL
                BEGIN
                    EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @ConstraintName + N']');
                END

                ALTER TABLE [Users] DROP COLUMN [{column}];
            END
            """;

        await _dbContext.Database.ExecuteSqlRawAsync(sql);
    }

    private static List<string> UserAccountColumns()
    {
        return
        [
            "Email",
            "ProfileImagePath",
            "IsTwoFactorEnabled",
            "IsEmailNotificationsEnabled",
            "IsAdmin",
            "IsBlocked"
        ];
    }
}
