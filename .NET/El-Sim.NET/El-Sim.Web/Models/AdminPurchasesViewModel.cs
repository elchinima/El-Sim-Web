namespace El_Sim.Web.Models;

public class AdminPurchasesViewModel
{
    public string SearchQuery { get; set; } = string.Empty;
    public int? SelectedUserId { get; set; }
    public List<AdminPurchaseUserViewModel> Users { get; set; } = [];
    public List<AdminPurchaseRowViewModel> Purchases { get; set; } = [];
}

public class AdminPurchaseUserViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Fin { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int PurchaseCount { get; set; }
    public int ReceiptCount { get; set; }
    public DateTime LastReceiptAtUtc { get; set; }
}

public class AdminPurchaseRowViewModel
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string ReceiptType { get; set; } = string.Empty;
    public string ReceiptStatus { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserFin { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCurrency { get; set; } = "AZN";
    public decimal ProductAmount { get; set; }
    public decimal TotalAzn { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal CommissionRate { get; set; }
    public bool HasStaticIp { get; set; }
    public decimal StaticIpRate { get; set; }
    public decimal StaticIpFeeAzn { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}
