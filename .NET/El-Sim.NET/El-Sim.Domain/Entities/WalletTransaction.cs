namespace El_Sim.Domain.Entities;

public class WalletTransaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ProductPurchaseId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal AmountAzn { get; set; }
    public decimal BalanceAfterAzn { get; set; }
    public string? StripeSessionId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public AppUser? User { get; set; }
    public ProductPurchase? ProductPurchase { get; set; }
    public ICollection<PaymentReceipt> Receipts { get; set; } = new List<PaymentReceipt>();
}

