namespace El_Sim.Persistence.Entities;

public class AppUser
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Fin { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsEmailNotificationsEnabled { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public DateOnly CreatedDate { get; set; }
    public ICollection<TwoFactorCode> TwoFactorCodes { get; set; } = new List<TwoFactorCode>();
}
