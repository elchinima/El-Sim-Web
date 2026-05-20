namespace El_Sim.Infrastructure.ExchangeRates;

public class ExchangeRateService : IExchangeRateService
{
    private const decimal FallbackUsdRate = 1.7m;
    private readonly HttpClient _httpClient;

    public ExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExchangeRateResult> GetUsdToAznAsync(CancellationToken cancellationToken = default)
    {
        for (var date = DateTime.Today; date >= DateTime.Today.AddDays(-10); date = date.AddDays(-1))
        {
            try
            {
                var url = $"https://cbar.az/currencies/{date:dd.MM.yyyy}.xml";
                var xml = await _httpClient.GetStringAsync(url, cancellationToken);
                var document = XDocument.Parse(xml);
                var usd = document.Descendants("Valute")
                    .FirstOrDefault(element => string.Equals(element.Attribute("Code")?.Value, "USD", StringComparison.OrdinalIgnoreCase));
                var value = usd?.Element("Value")?.Value;

                if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var rate) && rate > 0)
                {
                    return new ExchangeRateResult(rate, date, false);
                }
            }
            catch
            {
            }
        }

        return new ExchangeRateResult(FallbackUsdRate, DateTime.Today, true);
    }
}
