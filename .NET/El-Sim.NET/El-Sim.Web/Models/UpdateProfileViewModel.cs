namespace El_Sim.Web.Models;

public class UpdateProfileViewModel
{
    [Required]
    [StringLength(25, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; set; }

    public bool IsTwoFactorEnabled { get; set; }
    public bool IsEmailNotificationsEnabled { get; set; }
}
