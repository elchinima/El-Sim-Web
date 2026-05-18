namespace El_Sim.Web.Models;

public class AdminPurchasesViewModel
{
    public string SearchQuery { get; set; } = string.Empty;
    public List<AdminPurchaseRowViewModel> Purchases { get; set; } = [];
}

public class AdminPurchaseRowViewModel
{
    public int Id { get; set; }
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
