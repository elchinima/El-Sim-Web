namespace El_Sim.Web.Services;

public class ProductPricingService
{
    public const decimal ConversionCommissionRate = 0.03m;

    public async Task<ProductPriceCalculation> CalculateAsync(Product product, ExchangeRateService exchangeRateService, CancellationToken cancellationToken = default)
    {
        var currency = NormalizeCurrency(product.Currency, product.Price);
        var amount = ParseAmount(product.Price);

        if (currency == "USD")
        {
            var rate = await exchangeRateService.GetUsdToAznAsync(cancellationToken);
            var total = decimal.Round(amount * rate.UsdToAzn * (1 + ConversionCommissionRate), 2, MidpointRounding.AwayFromZero);

            return new ProductPriceCalculation(currency, amount, total, rate.UsdToAzn, ConversionCommissionRate, rate.Date, rate.IsFallback);
        }

        return new ProductPriceCalculation("AZN", amount, decimal.Round(amount, 2, MidpointRounding.AwayFromZero), null, 0m, null, false);
    }

    public static string NormalizeCurrency(string? currency, string price)
    {
        if (string.Equals(currency?.Trim(), "USD", StringComparison.OrdinalIgnoreCase)
            || price.Contains('$')
            || price.Contains("USD", StringComparison.OrdinalIgnoreCase))
        {
            return "USD";
        }

        return "AZN";
    }

    public static decimal ParseAmount(string price)
    {
        var match = Regex.Match(price.Replace(',', '.'), @"\d+(\.\d+)?");

        return match.Success && decimal.TryParse(match.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var amount)
            ? amount
            : 0m;
    }
}

public record ProductPriceCalculation(
    string Currency,
    decimal ProductAmount,
    decimal TotalAzn,
    decimal? ExchangeRate,
    decimal CommissionRate,
    DateTime? RateDate,
    bool IsFallbackRate);
