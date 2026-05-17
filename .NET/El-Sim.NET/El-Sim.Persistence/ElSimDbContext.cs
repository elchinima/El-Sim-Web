namespace El_Sim.Persistence;

public class ElSimDbContext : DbContext
{
    public ElSimDbContext(DbContextOptions<ElSimDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TwoFactorCode> TwoFactorCodes => Set<TwoFactorCode>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<HomeSlider> HomeSliders => Set<HomeSlider>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Id).ValueGeneratedOnAdd();
            entity.Property(user => user.Name).HasMaxLength(25).IsRequired();
            entity.Property(user => user.Fin).HasMaxLength(7).IsRequired();
            entity.HasIndex(user => user.Fin).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(254);
            entity.Property(user => user.ProfileImagePath).HasMaxLength(260);
            entity.Property(user => user.IsAdmin).HasDefaultValue(false);
            entity.Property(user => user.IsBlocked).HasDefaultValue(false);
            entity.Property(user => user.IsTwoFactorEnabled).HasDefaultValue(false);
            entity.Property(user => user.IsEmailNotificationsEnabled).HasDefaultValue(false);
            entity.Property(user => user.PasswordHash).HasMaxLength(128).IsRequired();
            entity.Property(user => user.PasswordSalt).HasMaxLength(128).IsRequired();
            entity.Property(user => user.CreatedDate).HasColumnType("date").IsRequired();
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
