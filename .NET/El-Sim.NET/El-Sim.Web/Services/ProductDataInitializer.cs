namespace El_Sim.Web.Services;

public class ProductDataInitializer
{
    private readonly ElSimDbContext _dbContext;

    public ProductDataInitializer(ElSimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task InitializeAsync()
    {
        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[Products]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Products] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Products] PRIMARY KEY,
                    [Category] nvarchar(32) NOT NULL,
                    [ProductType] nvarchar(32) NOT NULL CONSTRAINT [DF_Products_ProductType] DEFAULT N'',
                    [Name] nvarchar(80) NOT NULL,
                    [NameRu] nvarchar(80) NOT NULL CONSTRAINT [DF_Products_NameRu] DEFAULT N'',
                    [NameAz] nvarchar(80) NOT NULL CONSTRAINT [DF_Products_NameAz] DEFAULT N'',
                    [Price] nvarchar(40) NOT NULL,
                    [PriceRu] nvarchar(40) NOT NULL CONSTRAINT [DF_Products_PriceRu] DEFAULT N'',
                    [PriceAz] nvarchar(40) NOT NULL CONSTRAINT [DF_Products_PriceAz] DEFAULT N'',
                    [Currency] nvarchar(3) NOT NULL CONSTRAINT [DF_Products_Currency] DEFAULT N'AZN',
                    [Period] nvarchar(40) NOT NULL CONSTRAINT [DF_Products_Period] DEFAULT N'',
                    [PeriodRu] nvarchar(40) NOT NULL CONSTRAINT [DF_Products_PeriodRu] DEFAULT N'',
                    [PeriodAz] nvarchar(40) NOT NULL CONSTRAINT [DF_Products_PeriodAz] DEFAULT N'',
                    [Description] nvarchar(260) NOT NULL CONSTRAINT [DF_Products_Description] DEFAULT N'',
                    [DescriptionRu] nvarchar(260) NOT NULL CONSTRAINT [DF_Products_DescriptionRu] DEFAULT N'',
                    [DescriptionAz] nvarchar(260) NOT NULL CONSTRAINT [DF_Products_DescriptionAz] DEFAULT N'',
                    [Features] nvarchar(max) NOT NULL,
                    [FeaturesRu] nvarchar(max) NOT NULL CONSTRAINT [DF_Products_FeaturesRu] DEFAULT N'',
                    [FeaturesAz] nvarchar(max) NOT NULL CONSTRAINT [DF_Products_FeaturesAz] DEFAULT N'',
                    [ButtonText] nvarchar(80) NOT NULL,
                    [ButtonTextRu] nvarchar(80) NOT NULL CONSTRAINT [DF_Products_ButtonTextRu] DEFAULT N'',
                    [ButtonTextAz] nvarchar(80) NOT NULL CONSTRAINT [DF_Products_ButtonTextAz] DEFAULT N'',
                    [ButtonUrl] nvarchar(2048) NOT NULL CONSTRAINT [DF_Products_ButtonUrl] DEFAULT N'',
                    [IsFeatured] bit NOT NULL CONSTRAINT [DF_Products_IsFeatured] DEFAULT CAST(0 AS bit),
                    [IsFavorite] bit NOT NULL CONSTRAINT [DF_Products_IsFavorite] DEFAULT CAST(0 AS bit),
                    [SortOrder] int NOT NULL
                );

                CREATE INDEX [IX_Products_Category_SortOrder] ON [Products] ([Category], [SortOrder]);
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'[HomeSliders]', N'U') IS NULL
            BEGIN
                CREATE TABLE [HomeSliders] (
                    [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_HomeSliders] PRIMARY KEY,
                    [ImagePath] nvarchar(260) NOT NULL,
                    [AltText] nvarchar(160) NOT NULL CONSTRAINT [DF_HomeSliders_AltText] DEFAULT N'',
                    [Language] nvarchar(2) NOT NULL CONSTRAINT [DF_HomeSliders_Language] DEFAULT N'en',
                    [IsMobile] bit NOT NULL CONSTRAINT [DF_HomeSliders_IsMobile] DEFAULT CAST(0 AS bit),
                    [SortOrder] int NOT NULL,
                    [CreatedAtUtc] datetime2 NOT NULL
                );

                CREATE INDEX [IX_HomeSliders_Language_IsMobile_SortOrder] ON [HomeSliders] ([Language], [IsMobile], [SortOrder]);
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[Products]', N'Currency') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [Currency] nvarchar(3) NOT NULL CONSTRAINT [DF_Products_Currency] DEFAULT N'AZN';
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[Products]', N'ProductType') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [ProductType] nvarchar(32) NOT NULL CONSTRAINT [DF_Products_ProductType] DEFAULT N'';
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            UPDATE [Products]
            SET [Currency] = N'USD'
            WHERE ([Price] LIKE N'%$%' OR [Price] LIKE N'%USD%') AND [Currency] <> N'USD';
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            UPDATE [Products]
            SET
                [Price] = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE([Price], N'$', N''), N'USD', N''), N'AZN', N''))),
                [PriceRu] = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE([PriceRu], N'$', N''), N'USD', N''), N'AZN', N''))),
                [PriceAz] = LTRIM(RTRIM(REPLACE(REPLACE(REPLACE([PriceAz], N'$', N''), N'USD', N''), N'AZN', N'')))
            WHERE [Price] LIKE N'%$%' OR [Price] LIKE N'%USD%' OR [Price] LIKE N'%AZN%'
                OR [PriceRu] LIKE N'%$%' OR [PriceRu] LIKE N'%USD%' OR [PriceRu] LIKE N'%AZN%'
                OR [PriceAz] LIKE N'%$%' OR [PriceAz] LIKE N'%USD%' OR [PriceAz] LIKE N'%AZN%';
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[HomeSliders]', N'Language') IS NULL
            BEGIN
                ALTER TABLE [HomeSliders] ADD [Language] nvarchar(2) NOT NULL CONSTRAINT [DF_HomeSliders_Language] DEFAULT N'en';
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[Products]', N'IsFavorite') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [IsFavorite] bit NOT NULL CONSTRAINT [DF_Products_IsFavorite] DEFAULT CAST(0 AS bit);
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            IF COL_LENGTH(N'[Products]', N'ButtonUrl') IS NULL
            BEGIN
                ALTER TABLE [Products] ADD [ButtonUrl] nvarchar(2048) NOT NULL CONSTRAINT [DF_Products_ButtonUrl] DEFAULT N'';
            END
            """);

        await _dbContext.Database.ExecuteSqlRawAsync("""
            UPDATE [Products] SET [IsFavorite] = [IsFeatured] WHERE [IsFeatured] = CAST(1 AS bit) AND [IsFavorite] = CAST(0 AS bit);
            """);

        foreach (var column in LocalizedColumns())
        {
            var addColumnSql = $"""
                IF COL_LENGTH(N'[Products]', N'{column.Name}') IS NULL
                BEGIN
                    ALTER TABLE [Products] ADD [{column.Name}] {column.Type} NOT NULL CONSTRAINT [DF_Products_{column.Name}] DEFAULT N'';
                END
                """;

            await _dbContext.Database.ExecuteSqlRawAsync(addColumnSql);
        }

        var existingCategories = await _dbContext.Products
            .Select(product => product.Category)
            .Distinct()
            .ToListAsync();

        var seedProducts = SeedProducts();
        var missingProducts = seedProducts
            .Where(product => !existingCategories.Contains(product.Category))
            .ToList();

        if (missingProducts.Count > 0)
        {
            _dbContext.Products.AddRange(missingProducts);
            await _dbContext.SaveChangesAsync();
        }

        await ApplyMissingTranslations(seedProducts);
    }

    private static List<Product> SeedProducts()
    {
        List<Product> products =
        [
            new Product { Category = "esim", ProductType = "basic", Name = "Start", Price = "3", Currency = "AZN", Description = "5 GB of mobile data", Features = "5 GB of mobile data", ButtonText = "Choose", SortOrder = 10 },
            new Product { Category = "esim", ProductType = "basic", Name = "Smart", Price = "5", Currency = "AZN", Description = "10 GB, calls and SMS", Features = "10 GB, calls and SMS", ButtonText = "Choose", IsFeatured = true, IsFavorite = true, SortOrder = 20 },
            new Product { Category = "esim", ProductType = "basic", Name = "Max", Price = "9", Currency = "AZN", Description = "Unlimited mobile data", Features = "Unlimited mobile data", ButtonText = "Choose", SortOrder = 30 },

            new Product { Category = "pass", Name = "Pass", Price = "4.00", Currency = "AZN", Period = "30 days", Features = "Up to 50 free calls\nBonuses from our partners\nOpportunity to earn up to 5GB\nConnection to next-generation 5G internet\nUp to 50% ad blocking", ButtonText = "Choose Pass", IsFeatured = true, IsFavorite = true, SortOrder = 10 },
            new Product { Category = "pass", Name = "Pass Plus", Price = "7.00", Currency = "AZN", Period = "30 days", Features = "5G+ internet connection\nUp to 100 free calls\nOpportunity to earn up to 10GB\nHigher bonuses from partners\nUp to 70% ad blocking", ButtonText = "Choose Plus", SortOrder = 20 },
            new Product { Category = "pass", Name = "Pass Ultra", Price = "11.00", Currency = "AZN", Period = "30 days", Features = "VIP customer service\n5G+ internet connection\nUp to 150 free calls\nOpportunity to earn up to 15GB\nUltra bonuses from partners\nAbility to change the app icon\nUp to 90% ad blocking\nNetwork access anywhere in the Republic of Azerbaijan", ButtonText = "Choose Ultra", SortOrder = 30 },

            new Product { Category = "tariffs", Name = "Eco", Price = "3.00", Currency = "AZN", Period = "30 days", Features = "1GB internet\n30 free domestic minutes\n30 free domestic SMS", ButtonText = "Choose Eco", SortOrder = 10 },
            new Product { Category = "tariffs", Name = "Standard", Price = "8.00", Currency = "AZN", Period = "30 days", Features = "5GB internet\n50 free domestic minutes\n50 free domestic SMS", ButtonText = "Choose Standard", IsFeatured = true, IsFavorite = true, SortOrder = 20 },
            new Product { Category = "tariffs", Name = "Plus", Price = "18.00", Currency = "AZN", Period = "30 days", Features = "15GB internet\n150 free domestic minutes\n150 free domestic SMS", ButtonText = "Choose Plus", SortOrder = 30 },
            new Product { Category = "tariffs", Name = "Pro", Price = "28.00", Currency = "AZN", Period = "30 days", Features = "35GB internet\n350 free domestic minutes\n350 free domestic SMS", ButtonText = "Choose Pro", IsFeatured = true, IsFavorite = true, SortOrder = 40 },
            new Product { Category = "tariffs", Name = "Premium", Price = "48.00", Currency = "AZN", Period = "30 days", Features = "80GB internet\n800 free domestic minutes\n800 free domestic SMS", ButtonText = "Choose Premium", SortOrder = 50 },
            new Product { Category = "tariffs", Name = "Ultra", Price = "78.00", Currency = "AZN", Period = "30 days", Features = "120GB internet\n1200 free domestic minutes\n1200 free domestic SMS", ButtonText = "Choose Ultra", SortOrder = 60 },

            new Product { Category = "global", Name = "Eco G", Price = "3.00", Currency = "USD", Period = "30 days", Features = "1GB internet", ButtonText = "Choose Eco G", SortOrder = 10 },
            new Product { Category = "global", Name = "Eco G2", Price = "1.50", Currency = "USD", Period = "7 days", Features = "1GB internet", ButtonText = "Choose Eco G2", SortOrder = 20 },
            new Product { Category = "global", Name = "Standard G", Price = "8.00", Currency = "USD", Period = "30 days", Features = "5GB internet", ButtonText = "Choose Standard G", SortOrder = 30 },
            new Product { Category = "global", Name = "Standard G2", Price = "4.00", Currency = "USD", Period = "7 days", Features = "5GB internet", ButtonText = "Choose Standard G2", SortOrder = 40 },
            new Product { Category = "global", Name = "Plus G", Price = "18.00", Currency = "USD", Period = "30 days", Features = "15GB internet", ButtonText = "Choose Plus G", IsFeatured = true, IsFavorite = true, SortOrder = 50 },
            new Product { Category = "global", Name = "Plus G2", Price = "9.00", Currency = "USD", Period = "7 days", Features = "15GB internet", ButtonText = "Choose Plus G2", SortOrder = 60 },
            new Product { Category = "global", Name = "Pro G", Price = "28.00", Currency = "USD", Period = "30 days", Features = "35GB internet", ButtonText = "Choose Pro G", SortOrder = 70 },
            new Product { Category = "global", Name = "Pro G2", Price = "14.00", Currency = "USD", Period = "7 days", Features = "35GB internet", ButtonText = "Choose Pro G2", SortOrder = 80 },
            new Product { Category = "global", Name = "Premium G", Price = "48.00", Currency = "USD", Period = "30 days", Features = "80GB internet", ButtonText = "Choose Premium G", SortOrder = 90 },
            new Product { Category = "global", Name = "Premium G2", Price = "24.00", Currency = "USD", Period = "7 days", Features = "80GB internet", ButtonText = "Choose Premium G2", SortOrder = 100 },
            new Product { Category = "global", Name = "Ultra G", Price = "78.00", Currency = "USD", Period = "30 days", Features = "120GB internet", ButtonText = "Choose Ultra G", IsFeatured = true, IsFavorite = true, SortOrder = 110 },
            new Product { Category = "global", Name = "Ultra G2", Price = "39.00", Currency = "USD", Period = "7 days", Features = "120GB internet", ButtonText = "Choose Ultra G2", SortOrder = 120 },

            new Product { Category = "wifi", Name = "Optic 1S", Price = "100.00", Currency = "AZN", Features = "Up to 1 GBit/s\nMinimum Wi-Fi 6", ButtonText = "Choose 1S", SortOrder = 10 },
            new Product { Category = "wifi", Name = "Optic 1.5S", Price = "230.00", Currency = "AZN", Features = "Up to 5 GBit/s\nMinimum Wi-Fi 6E", ButtonText = "Choose 1.5S", SortOrder = 20 },
            new Product { Category = "wifi", Name = "Optic 2S", Price = "450.00", Currency = "AZN", Features = "Up to 10 GBit/s\nMinimum Wi-Fi 7", ButtonText = "Choose 2S", IsFeatured = true, IsFavorite = true, SortOrder = 30 },
            new Product { Category = "wifi", Name = "Optic 2S+", Price = "800.00", Currency = "AZN", Features = "Up to 20 GBit/s\nMinimum Wi-Fi 7", ButtonText = "Choose 2S+", SortOrder = 40 }
        ];

        foreach (var product in products)
        {
            ApplySeedTranslations(product);
        }

        return products;
    }

    private async Task ApplyMissingTranslations(List<Product> seedProducts)
    {
        var products = await _dbContext.Products.ToListAsync();

        foreach (var product in products)
        {
            var seedProduct = seedProducts.FirstOrDefault(item => item.Category == product.Category && item.Name == product.Name);

            if (seedProduct is null)
            {
                continue;
            }

            product.NameRu = Fill(product.NameRu, seedProduct.NameRu);
            product.NameAz = Fill(product.NameAz, seedProduct.NameAz);
            product.ProductType = Fill(product.ProductType, seedProduct.ProductType);
            product.PriceRu = Fill(product.PriceRu, seedProduct.PriceRu);
            product.PriceAz = Fill(product.PriceAz, seedProduct.PriceAz);
            product.Currency = Fill(product.Currency, seedProduct.Currency);
            product.PeriodRu = Fill(product.PeriodRu, seedProduct.PeriodRu);
            product.PeriodAz = Fill(product.PeriodAz, seedProduct.PeriodAz);
            product.DescriptionRu = Fill(product.DescriptionRu, seedProduct.DescriptionRu);
            product.DescriptionAz = Fill(product.DescriptionAz, seedProduct.DescriptionAz);
            product.Features = Fill(product.Features, seedProduct.Features);
            product.FeaturesRu = Fill(product.FeaturesRu, seedProduct.FeaturesRu);
            product.FeaturesAz = Fill(product.FeaturesAz, seedProduct.FeaturesAz);
            product.ButtonTextRu = Fill(product.ButtonTextRu, seedProduct.ButtonTextRu);
            product.ButtonTextAz = Fill(product.ButtonTextAz, seedProduct.ButtonTextAz);
            product.ButtonUrl = Fill(product.ButtonUrl, seedProduct.ButtonUrl);
        }

        await _dbContext.SaveChangesAsync();
    }

    private static string Fill(string current, string seed)
    {
        return string.IsNullOrWhiteSpace(current) ? seed : current;
    }

    private static void ApplySeedTranslations(Product product)
    {
        product.NameRu = ProductNameRu(product.Name);
        product.NameAz = ProductNameAz(product.Name);
        product.PriceRu = product.Price;
        product.PriceAz = product.Price;
        product.PeriodRu = Translate(product.Period, RuTranslations);
        product.PeriodAz = Translate(product.Period, AzTranslations);
        product.DescriptionRu = Translate(product.Description, RuTranslations);
        product.DescriptionAz = Translate(product.Description, AzTranslations);
        product.FeaturesRu = TranslateLines(product.Features, RuTranslations);
        product.FeaturesAz = TranslateLines(product.Features, AzTranslations);
        product.ButtonTextRu = TranslateButton(product.ButtonText, "Выбрать");
        product.ButtonTextAz = TranslateButton(product.ButtonText, "seç");
        product.ButtonUrl = string.Empty;
    }

    private static string Translate(string value, IReadOnlyDictionary<string, string> translations)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return translations.TryGetValue(value, out var translated) ? translated : value;
    }

    private static string TranslateLines(string value, IReadOnlyDictionary<string, string> translations)
    {
        return string.Join('\n', value
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(line => Translate(line, translations)));
    }

    private static string TranslateButton(string value, string chooseText)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (value == "Choose")
        {
            return chooseText == "seç" ? "Seç" : chooseText;
        }

        const string prefix = "Choose ";

        return value.StartsWith(prefix, StringComparison.Ordinal)
            ? chooseText == "seç"
                ? $"{value[prefix.Length..]} {chooseText}"
                : $"{chooseText} {value[prefix.Length..]}"
            : value;
    }

    private static string ProductNameRu(string name)
    {
        return name;
    }

    private static string ProductNameAz(string name)
    {
        return name;
    }

    private static readonly IReadOnlyDictionary<string, string> RuTranslations = new Dictionary<string, string>
    {
        ["5 GB of mobile data"] = "5 GB мобильного интернета",
        ["10 GB, calls and SMS"] = "10 GB, звонки и SMS",
        ["Unlimited mobile data"] = "Безлимитный мобильный интернет",
        ["30 days"] = "30 дней",
        ["7 days"] = "7 дней",
        ["Up to 50 free calls"] = "До 50 бесплатных звонков",
        ["Bonuses from our partners"] = "Бонусы от наших партнеров",
        ["Opportunity to earn up to 5GB"] = "Возможность получить до 5GB",
        ["Connection to next-generation 5G internet"] = "Подключение к интернету 5G нового поколения",
        ["Up to 50% ad blocking"] = "Блокировка рекламы до 50%",
        ["5G+ internet connection"] = "Подключение к интернету 5G+",
        ["Up to 100 free calls"] = "До 100 бесплатных звонков",
        ["Opportunity to earn up to 10GB"] = "Возможность получить до 10GB",
        ["Higher bonuses from partners"] = "Повышенные бонусы от партнеров",
        ["Up to 70% ad blocking"] = "Блокировка рекламы до 70%",
        ["VIP customer service"] = "VIP обслуживание клиентов",
        ["Up to 150 free calls"] = "До 150 бесплатных звонков",
        ["Opportunity to earn up to 15GB"] = "Возможность получить до 15GB",
        ["Ultra bonuses from partners"] = "Ultra бонусы от партнеров",
        ["Ability to change the app icon"] = "Возможность менять иконку приложения",
        ["Up to 90% ad blocking"] = "Блокировка рекламы до 90%",
        ["Network access anywhere in the Republic of Azerbaijan"] = "Доступ к сети в любой точке Азербайджанской Республики",
        ["1GB internet"] = "1GB интернета",
        ["5GB internet"] = "5GB интернета",
        ["15GB internet"] = "15GB интернета",
        ["35GB internet"] = "35GB интернета",
        ["80GB internet"] = "80GB интернета",
        ["120GB internet"] = "120GB интернета",
        ["30 free domestic minutes"] = "30 бесплатных минут внутри страны",
        ["30 free domestic SMS"] = "30 бесплатных SMS внутри страны",
        ["50 free domestic minutes"] = "50 бесплатных минут внутри страны",
        ["50 free domestic SMS"] = "50 бесплатных SMS внутри страны",
        ["150 free domestic minutes"] = "150 бесплатных минут внутри страны",
        ["150 free domestic SMS"] = "150 бесплатных SMS внутри страны",
        ["350 free domestic minutes"] = "350 бесплатных минут внутри страны",
        ["350 free domestic SMS"] = "350 бесплатных SMS внутри страны",
        ["800 free domestic minutes"] = "800 бесплатных минут внутри страны",
        ["800 free domestic SMS"] = "800 бесплатных SMS внутри страны",
        ["1200 free domestic minutes"] = "1200 бесплатных минут внутри страны",
        ["1200 free domestic SMS"] = "1200 бесплатных SMS внутри страны",
        ["Up to 1 GBit/s"] = "До 1 Гбит/с",
        ["Up to 5 GBit/s"] = "До 5 Гбит/с",
        ["Up to 10 GBit/s"] = "До 10 Гбит/с",
        ["Up to 20 GBit/s"] = "До 20 Гбит/с",
        ["Minimum Wi-Fi 6"] = "Минимум Wi-Fi 6",
        ["Minimum Wi-Fi 6E"] = "Минимум Wi-Fi 6E",
        ["Minimum Wi-Fi 7"] = "Минимум Wi-Fi 7"
    };

    private static readonly IReadOnlyDictionary<string, string> AzTranslations = new Dictionary<string, string>
    {
        ["5 GB of mobile data"] = "5 GB mobil internet",
        ["10 GB, calls and SMS"] = "10 GB, zənglər və SMS",
        ["Unlimited mobile data"] = "Limitsiz mobil internet",
        ["30 days"] = "30 gün",
        ["7 days"] = "7 gün",
        ["Up to 50 free calls"] = "50-dək pulsuz zənglər",
        ["Bonuses from our partners"] = "Partnyorlarımızdan bonuslar",
        ["Opportunity to earn up to 5GB"] = "5GB-dək qazanmaq imkanı",
        ["Connection to next-generation 5G internet"] = "5G nəsil internetə qoşulma",
        ["Up to 50% ad blocking"] = "50%-dək reklamların bloklanması",
        ["5G+ internet connection"] = "5G+ internetə qoşulma",
        ["Up to 100 free calls"] = "100-dək pulsuz zənglər",
        ["Opportunity to earn up to 10GB"] = "10GB-dək qazanmaq imkanı",
        ["Higher bonuses from partners"] = "Partnyorlardan yüksək bonuslar",
        ["Up to 70% ad blocking"] = "70%-dək reklamların bloklanması",
        ["VIP customer service"] = "VIP müştəri xidməti",
        ["Up to 150 free calls"] = "150-dək pulsuz zənglər",
        ["Opportunity to earn up to 15GB"] = "15GB-dək qazanmaq imkanı",
        ["Ultra bonuses from partners"] = "Partnyorlardan ultra bonuslar",
        ["Ability to change the app icon"] = "App icon-u dəyişdirmək imkanı",
        ["Up to 90% ad blocking"] = "90%-dək reklamların bloklanması",
        ["Network access anywhere in the Republic of Azerbaijan"] = "AR istənilən ərazisində şəbəkə imkanı",
        ["1GB internet"] = "1GB internet",
        ["5GB internet"] = "5GB internet",
        ["15GB internet"] = "15GB internet",
        ["35GB internet"] = "35GB internet",
        ["80GB internet"] = "80GB internet",
        ["120GB internet"] = "120GB internet",
        ["30 free domestic minutes"] = "30 dəq ölkədaxili pulsuz",
        ["30 free domestic SMS"] = "30 SMS ölkədaxili pulsuz",
        ["50 free domestic minutes"] = "50 dəq ölkədaxili pulsuz",
        ["50 free domestic SMS"] = "50 SMS ölkədaxili pulsuz",
        ["150 free domestic minutes"] = "150 dəq ölkədaxili pulsuz",
        ["150 free domestic SMS"] = "150 SMS ölkədaxili pulsuz",
        ["350 free domestic minutes"] = "350 dəq ölkədaxili pulsuz",
        ["350 free domestic SMS"] = "350 SMS ölkədaxili pulsuz",
        ["800 free domestic minutes"] = "800 dəq ölkədaxili pulsuz",
        ["800 free domestic SMS"] = "800 SMS ölkədaxili pulsuz",
        ["1200 free domestic minutes"] = "1200 dəq ölkədaxili pulsuz",
        ["1200 free domestic SMS"] = "1200 SMS ölkədaxili pulsuz",
        ["Up to 1 GBit/s"] = "1 Gbit/s-dək",
        ["Up to 5 GBit/s"] = "5 Gbit/s-dək",
        ["Up to 10 GBit/s"] = "10 Gbit/s-dək",
        ["Up to 20 GBit/s"] = "20 Gbit/s-dək",
        ["Minimum Wi-Fi 6"] = "Min. Wi-Fi 6",
        ["Minimum Wi-Fi 6E"] = "Min. Wi-Fi 6E",
        ["Minimum Wi-Fi 7"] = "Min. Wi-Fi 7"
    };

    private static List<(string Name, string Type)> LocalizedColumns()
    {
        return
        [
            ("NameRu", "nvarchar(80)"),
            ("NameAz", "nvarchar(80)"),
            ("PriceRu", "nvarchar(40)"),
            ("PriceAz", "nvarchar(40)"),
            ("PeriodRu", "nvarchar(40)"),
            ("PeriodAz", "nvarchar(40)"),
            ("DescriptionRu", "nvarchar(260)"),
            ("DescriptionAz", "nvarchar(260)"),
            ("FeaturesRu", "nvarchar(max)"),
            ("FeaturesAz", "nvarchar(max)"),
            ("ButtonTextRu", "nvarchar(80)"),
            ("ButtonTextAz", "nvarchar(80)")
        ];
    }
}
