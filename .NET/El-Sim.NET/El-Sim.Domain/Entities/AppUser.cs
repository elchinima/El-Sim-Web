namespace El_Sim.Domain.Entities;

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
    public ICollection<ProductPurchase> ProductPurchases { get; set; } = new List<ProductPurchase>();
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    public ICollection<PaymentReceipt> PaymentReceipts { get; set; } = new List<PaymentReceipt>();
}

