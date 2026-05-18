namespace El_Sim.Persistence.Entities;

public class Account
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? ProfileImagePath { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsEmailNotificationsEnabled { get; set; }
    public AppUser? User { get; set; }
}
