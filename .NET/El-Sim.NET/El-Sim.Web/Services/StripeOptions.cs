namespace El_Sim.Web.Services;

public class StripeOptions
{
    public string PublishableKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Currency { get; set; } = "usd";
}
