namespace El_Sim.Web.Models;

public class ProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Fin { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfileImagePath { get; set; } = string.Empty;
    public UserAssetsViewModel? UserAssets { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsEmailNotificationsEnabled { get; set; }
}
