namespace El_Sim.Persistence;

public class ElSimDbContext : DbContext
{
    public ElSimDbContext(DbContextOptions<ElSimDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TwoFactorCode> TwoFactorCodes => Set<TwoFactorCode>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<HomeSlider> HomeSliders => Set<HomeSlider>();
    public DbSet<UserAssets> UserAssets => Set<UserAssets>();
    public DbSet<ProductPurchase> ProductPurchases => Set<ProductPurchase>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    public DbSet<PaymentReceipt> PaymentReceipts => Set<PaymentReceipt>();
    public DbSet<AppSetting> AppSettings => Set<AppSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).ValueGeneratedOnAdd();
            entity.Property(user => user.AccountId).IsRequired();
            entity.Property(user => user.UserAssetsId);
            entity.Property(user => user.Name).HasMaxLength(25).IsRequired();
            entity.Property(user => user.Fin).HasMaxLength(7).IsRequired();
            entity.HasIndex(user => user.Fin).IsUnique();
            entity.Property(user => user.PasswordHash).HasMaxLength(128).IsRequired();
            entity.Property(user => user.PasswordSalt).HasMaxLength(128).IsRequired();
            entity.Property(user => user.CreatedDate).HasColumnType("date").IsRequired();
            entity.HasOne(user => user.Account)
                .WithOne(account => account.User)
                .HasForeignKey<AppUser>(user => user.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(user => user.AccountId).IsUnique();
            entity.HasOne(user => user.UserAssets)
                .WithOne(userAssets => userAssets.User)
                .HasForeignKey<AppUser>(user => user.UserAssetsId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(user => user.UserAssetsId).IsUnique()
                .HasFilter("[UserAssetsId] IS NOT NULL");
        });

        modelBuilder.Entity<UserAssets>(entity =>
        {
            entity.ToTable("UserAssets");
            entity.HasKey(userAssets => userAssets.Id);
            entity.Property(userAssets => userAssets.Id).ValueGeneratedOnAdd();
            entity.Property(userAssets => userAssets.BasicNumber).HasMaxLength(32);
            entity.Property(userAssets => userAssets.GlobalNumber).HasMaxLength(32);
            entity.Property(userAssets => userAssets.Pass).HasMaxLength(80);
            entity.Property(userAssets => userAssets.BasicTariff).HasMaxLength(80);
            entity.Property(userAssets => userAssets.GlobalTariff).HasMaxLength(80);
            entity.Property(userAssets => userAssets.WiFi).HasMaxLength(80);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.Id).ValueGeneratedOnAdd();
            entity.Property(account => account.Email).HasMaxLength(254);
            entity.Property(account => account.ProfileImagePath).HasMaxLength(260);
            entity.Property(account => account.BalanceAzn).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(account => account.IsAdmin).HasDefaultValue(false);
            entity.Property(account => account.IsBlocked).HasDefaultValue(false);
            entity.Property(account => account.IsTwoFactorEnabled).HasDefaultValue(false);
            entity.Property(account => account.IsEmailNotificationsEnabled).HasDefaultValue(false);
        });

        modelBuilder.Entity<TwoFactorCode>(entity =>
        {
            entity.ToTable("TwoFactorCodes");
            entity.HasKey(code => code.Id);
            entity.Property(code => code.Id).ValueGeneratedOnAdd();
            entity.Property(code => code.Code).HasMaxLength(7).IsRequired();
            entity.Property(code => code.RememberMe).IsRequired();
            entity.Property(code => code.ExpiresAtUtc).IsRequired();
            entity.Property(code => code.CreatedAtUtc).IsRequired();
            entity.HasOne(code => code.AppUser)
                .WithMany(user => user.TwoFactorCodes)
                .HasForeignKey(code => code.AppUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(product => product.Id);
            entity.Property(product => product.Id).ValueGeneratedOnAdd();
            entity.Property(product => product.Category).HasMaxLength(32).IsRequired();
            entity.Property(product => product.Name).HasMaxLength(80).IsRequired();
            entity.Property(product => product.NameRu).HasMaxLength(80);
            entity.Property(product => product.NameAz).HasMaxLength(80);
            entity.Property(product => product.Price).HasMaxLength(40).IsRequired();
            entity.Property(product => product.PriceRu).HasMaxLength(40);
            entity.Property(product => product.PriceAz).HasMaxLength(40);
            entity.Property(product => product.Currency).HasMaxLength(3).HasDefaultValue("AZN").IsRequired();
            entity.Property(product => product.Period).HasMaxLength(40);
            entity.Property(product => product.PeriodRu).HasMaxLength(40);
            entity.Property(product => product.PeriodAz).HasMaxLength(40);
            entity.Property(product => product.Description).HasMaxLength(260);
            entity.Property(product => product.DescriptionRu).HasMaxLength(260);
            entity.Property(product => product.DescriptionAz).HasMaxLength(260);
            entity.Property(product => product.Features).IsRequired();
            entity.Property(product => product.FeaturesRu);
            entity.Property(product => product.FeaturesAz);
            entity.Property(product => product.ButtonText).HasMaxLength(80).IsRequired();
            entity.Property(product => product.ButtonTextRu).HasMaxLength(80);
            entity.Property(product => product.ButtonTextAz).HasMaxLength(80);
            entity.Property(product => product.ButtonUrl).HasMaxLength(2048);
            entity.Property(product => product.IsFeatured).HasDefaultValue(false);
            entity.Property(product => product.IsFavorite).HasDefaultValue(false);
            entity.Property(product => product.SortOrder).IsRequired();
            entity.HasIndex(product => new { product.Category, product.SortOrder });
        });

        modelBuilder.Entity<ProductPurchase>(entity =>
        {
            entity.ToTable("ProductPurchases");
            entity.HasKey(purchase => purchase.Id);
            entity.Property(purchase => purchase.Id).ValueGeneratedOnAdd();
            entity.Property(purchase => purchase.Category).HasMaxLength(32).IsRequired();
            entity.Property(purchase => purchase.ProductName).HasMaxLength(80).IsRequired();
            entity.Property(purchase => purchase.ProductCurrency).HasMaxLength(3).IsRequired();
            entity.Property(purchase => purchase.ProductAmount).HasColumnType("decimal(18,2)");
            entity.Property(purchase => purchase.TotalAzn).HasColumnType("decimal(18,2)");
            entity.Property(purchase => purchase.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.Property(purchase => purchase.CommissionRate).HasColumnType("decimal(8,4)");
            entity.Property(purchase => purchase.HasStaticIp).HasDefaultValue(false);
            entity.Property(purchase => purchase.StaticIpRate).HasColumnType("decimal(8,4)");
            entity.Property(purchase => purchase.StaticIpFeeAzn).HasColumnType("decimal(18,2)");
            entity.Property(purchase => purchase.Status).HasMaxLength(24).IsRequired();
            entity.Property(purchase => purchase.CreatedAtUtc).IsRequired();
            entity.Property(purchase => purchase.AdminNote).HasMaxLength(260);
            entity.HasOne(purchase => purchase.User)
                .WithMany(user => user.ProductPurchases)
                .HasForeignKey(purchase => purchase.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(purchase => purchase.Product)
                .WithMany(product => product.Purchases)
                .HasForeignKey(purchase => purchase.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(purchase => new { purchase.UserId, purchase.Category, purchase.Status });
        });

        modelBuilder.Entity<WalletTransaction>(entity =>
        {
            entity.ToTable("WalletTransactions");
            entity.HasKey(transaction => transaction.Id);
            entity.Property(transaction => transaction.Id).ValueGeneratedOnAdd();
            entity.Property(transaction => transaction.Type).HasMaxLength(24).IsRequired();
            entity.Property(transaction => transaction.Status).HasMaxLength(24).IsRequired();
            entity.Property(transaction => transaction.AmountAzn).HasColumnType("decimal(18,2)");
            entity.Property(transaction => transaction.BalanceAfterAzn).HasColumnType("decimal(18,2)");
            entity.Property(transaction => transaction.StripeSessionId).HasMaxLength(128);
            entity.Property(transaction => transaction.Description).HasMaxLength(260).IsRequired();
            entity.Property(transaction => transaction.CreatedAtUtc).IsRequired();
            entity.HasOne(transaction => transaction.User)
                .WithMany(user => user.WalletTransactions)
                .HasForeignKey(transaction => transaction.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(transaction => transaction.ProductPurchase)
                .WithMany()
                .HasForeignKey(transaction => transaction.ProductPurchaseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(transaction => transaction.StripeSessionId).IsUnique()
                .HasFilter("[StripeSessionId] IS NOT NULL");
            entity.HasIndex(transaction => new { transaction.UserId, transaction.CreatedAtUtc });
        });

        modelBuilder.Entity<PaymentReceipt>(entity =>
        {
            entity.ToTable("PaymentReceipts");
            entity.HasKey(receipt => receipt.Id);
            entity.Property(receipt => receipt.Id).ValueGeneratedOnAdd();
            entity.Property(receipt => receipt.ReceiptNumber).HasMaxLength(40).IsRequired();
            entity.Property(receipt => receipt.Type).HasMaxLength(24).IsRequired();
            entity.Property(receipt => receipt.Status).HasMaxLength(24).IsRequired();
            entity.Property(receipt => receipt.Currency).HasMaxLength(3).IsRequired();
            entity.Property(receipt => receipt.OriginalAmount).HasColumnType("decimal(18,2)");
            entity.Property(receipt => receipt.AmountAzn).HasColumnType("decimal(18,2)");
            entity.Property(receipt => receipt.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.Property(receipt => receipt.CommissionRate).HasColumnType("decimal(8,4)");
            entity.Property(receipt => receipt.Description).HasMaxLength(260).IsRequired();
            entity.Property(receipt => receipt.PayloadJson).IsRequired();
            entity.Property(receipt => receipt.CreatedAtUtc).IsRequired();
            entity.HasOne(receipt => receipt.User)
                .WithMany(user => user.PaymentReceipts)
                .HasForeignKey(receipt => receipt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(receipt => receipt.ProductPurchase)
                .WithMany(purchase => purchase.Receipts)
                .HasForeignKey(receipt => receipt.ProductPurchaseId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(receipt => receipt.WalletTransaction)
                .WithMany(transaction => transaction.Receipts)
                .HasForeignKey(receipt => receipt.WalletTransactionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(receipt => receipt.ReceiptNumber).IsUnique();
            entity.HasIndex(receipt => new { receipt.UserId, receipt.CreatedAtUtc });
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.ToTable("AppSettings");
            entity.HasKey(setting => setting.Id);
            entity.Property(setting => setting.Id).ValueGeneratedOnAdd();
            entity.Property(setting => setting.Key).HasMaxLength(80).IsRequired();
            entity.Property(setting => setting.Value).HasMaxLength(260).IsRequired();
            entity.HasIndex(setting => setting.Key).IsUnique();
        });

        modelBuilder.Entity<HomeSlider>(entity =>
        {
            entity.ToTable("HomeSliders");
            entity.HasKey(slider => slider.Id);
            entity.Property(slider => slider.Id).ValueGeneratedOnAdd();
            entity.Property(slider => slider.ImagePath).HasMaxLength(260).IsRequired();
            entity.Property(slider => slider.AltText).HasMaxLength(160);
            entity.Property(slider => slider.Language).HasMaxLength(2).HasDefaultValue("en").IsRequired();
            entity.Property(slider => slider.IsMobile).HasDefaultValue(false);
            entity.Property(slider => slider.SortOrder).IsRequired();
            entity.Property(slider => slider.CreatedAtUtc).IsRequired();
            entity.HasIndex(slider => new { slider.Language, slider.IsMobile, slider.SortOrder });
        });
    }
}
