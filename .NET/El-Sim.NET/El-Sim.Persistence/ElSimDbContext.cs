namespace El_Sim.Persistence;

public class ElSimDbContext : DbContext
{
    public ElSimDbContext(DbContextOptions<ElSimDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TwoFactorCode> TwoFactorCodes => Set<TwoFactorCode>();

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
    }
}
