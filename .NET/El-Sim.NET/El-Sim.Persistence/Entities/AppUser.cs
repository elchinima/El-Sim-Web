namespace El_Sim.Persistence.Entities;

public class AppUser
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int? UserAssetsId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Fin { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public DateOnly CreatedDate { get; set; }
    public Account Account { get; set; } = new();
    public UserAssets? UserAssets { get; set; }
    public ICollection<TwoFactorCode> TwoFactorCodes { get; set; } = new List<TwoFactorCode>();
}
