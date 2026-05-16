namespace El_Sim.Persistence.Entities;

public class TwoFactorCode
{
    public int Id { get; set; }
    public int AppUserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool RememberMe { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public AppUser? AppUser { get; set; }
}
