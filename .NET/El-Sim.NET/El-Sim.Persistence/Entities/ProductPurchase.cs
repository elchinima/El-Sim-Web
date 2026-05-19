namespace El_Sim.Persistence.Entities;

public class ProductPurchase
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PhonePrefix { get; set; } = string.Empty;
    public string ProductCurrency { get; set; } = "AZN";
    public decimal ProductAmount { get; set; }
    public decimal TotalAzn { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal CommissionRate { get; set; }
    public bool HasStaticIp { get; set; }
    public decimal StaticIpRate { get; set; }
    public decimal StaticIpFeeAzn { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? CancelledAtUtc { get; set; }
    public DateTime? RefundedAtUtc { get; set; }
    public string? AdminNote { get; set; }
    public AppUser? User { get; set; }
    public Product? Product { get; set; }
    public ICollection<PaymentReceipt> Receipts { get; set; } = new List<PaymentReceipt>();
}
