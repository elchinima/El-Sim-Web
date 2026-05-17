namespace El_Sim.Web.Models;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int AdminUsers { get; set; }
    public int BlockedUsers { get; set; }
    public int TwoFactorUsers { get; set; }
    public int EmailNotificationUsers { get; set; }
    public int PendingTwoFactorCodes { get; set; }
    public string SearchQuery { get; set; } = string.Empty;
    public List<AdminUserRowViewModel> Users { get; set; } = [];
}

public class AdminUserRowViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Fin { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string ProfileImagePath { get; set; } = string.Empty;
    public bool IsAdmin { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsEmailNotificationsEnabled { get; set; }
}
