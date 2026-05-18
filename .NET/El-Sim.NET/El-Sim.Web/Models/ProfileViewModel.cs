namespace El_Sim.Web.Models;

public class ProfileViewModel
{
    public string Name { get; set; } = string.Empty;
    public string Fin { get; set; } = string.Empty;
    public string CreatedDate { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ProfileImagePath { get; set; } = string.Empty;
    public decimal BalanceAzn { get; set; }
    public UserAssetsViewModel? UserAssets { get; set; }
    public List<UserPurchaseViewModel> Purchases { get; set; } = [];
    public List<UserReceiptViewModel> Receipts { get; set; } = [];
    public bool IsTwoFactorEnabled { get; set; }
    public bool IsEmailNotificationsEnabled { get; set; }
}

public class UserPurchaseViewModel
{
    public string Category { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAzn { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class UserReceiptViewModel
{
    public string ReceiptNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal AmountAzn { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class TopUpBalanceViewModel
{
    [Range(1, 10000)]
    public decimal AmountAzn { get; set; }
}
