namespace El_Sim.Domain.Entities;

public class PaymentReceipt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? ProductPurchaseId { get; set; }
    public int? WalletTransactionId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Currency { get; set; } = "AZN";
    public decimal OriginalAmount { get; set; }
    public decimal AmountAzn { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal CommissionRate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public AppUser? User { get; set; }
    public ProductPurchase? ProductPurchase { get; set; }
    public WalletTransaction? WalletTransaction { get; set; }
}

