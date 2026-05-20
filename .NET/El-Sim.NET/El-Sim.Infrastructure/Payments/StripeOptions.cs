namespace El_Sim.Infrastructure.Payments;

public class StripeOptions
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Currency { get; set; } = "usd";
}
