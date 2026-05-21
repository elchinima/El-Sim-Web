namespace El_Sim.Persistence.Initialization;

public class SupportChatDataInitializer
{
    private readonly ElSimDbContext _dbContext;

    public SupportChatDataInitializer(ElSimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[SupportChats]', N'U') IS NULL
            BEGIN
                CREATE TABLE [SupportChats] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SupportChats] PRIMARY KEY,
                    [UserId] int NULL,
                    [SessionId] nvarchar(88) NOT NULL,
                    [AgentName] nvarchar(40) NOT NULL,
                    [IsClosed] bit NOT NULL CONSTRAINT [DF_SupportChats_IsClosed] DEFAULT CAST(0 AS bit),
                    [CreatedAtUtc] datetime2 NOT NULL,
                    [UpdatedAtUtc] datetime2 NOT NULL
                );
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[SupportChats]', N'IsClosed') IS NULL
            BEGIN
                ALTER TABLE [SupportChats] ADD [IsClosed] bit NOT NULL CONSTRAINT [DF_SupportChats_IsClosed] DEFAULT CAST(0 AS bit);
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[SupportChatMessages]', N'U') IS NULL
            BEGIN
                CREATE TABLE [SupportChatMessages] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SupportChatMessages] PRIMARY KEY,
                    [SupportChatId] int NOT NULL,
                    [Role] nvarchar(16) NOT NULL,
                    [Text] nvarchar(max) NOT NULL,
                    [CreatedAtUtc] datetime2 NOT NULL
                );
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[SupportChatImages]', N'U') IS NULL
            BEGIN
                CREATE TABLE [SupportChatImages] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_SupportChatImages] PRIMARY KEY,
                    [UserId] int NULL,
                    [SupportChatMessageId] int NULL,
                    [SessionId] nvarchar(88) NOT NULL,
                    [FilePath] nvarchar(260) NOT NULL,
                    [CreatedAtUtc] datetime2 NOT NULL
                );
            END
            """);

        await EnsureForeignKey("SupportChats", "Users", "FK_SupportChats_Users_UserId", "UserId", "Id", "CASCADE");
        await EnsureForeignKey("SupportChatMessages", "SupportChats", "FK_SupportChatMessages_SupportChats_SupportChatId", "SupportChatId", "Id", "CASCADE");
        await EnsureForeignKey("SupportChatImages", "Users", "FK_SupportChatImages_Users_UserId", "UserId", "Id", "NO ACTION");
        await EnsureForeignKey("SupportChatImages", "SupportChatMessages", "FK_SupportChatImages_SupportChatMessages_SupportChatMessageId", "SupportChatMessageId", "Id", "CASCADE");
        await EnsureIndex("SupportChats", "IX_SupportChats_UserId_CreatedAtUtc", "[UserId], [CreatedAtUtc]", false, null);
        await EnsureIndex("SupportChats", "IX_SupportChats_SessionId", "[SessionId]", false, null);
        await EnsureIndex("SupportChatMessages", "IX_SupportChatMessages_SupportChatId_CreatedAtUtc", "[SupportChatId], [CreatedAtUtc]", false, null);
        await EnsureIndex("SupportChatImages", "IX_SupportChatImages_CreatedAtUtc", "[CreatedAtUtc]", false, null);
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
}
