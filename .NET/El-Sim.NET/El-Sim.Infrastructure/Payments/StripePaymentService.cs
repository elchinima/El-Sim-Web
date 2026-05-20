namespace El_Sim.Infrastructure.Payments;

public class StripePaymentService
{
    private readonly StripeOptions _options;
    private readonly IExchangeRateService _exchangeRateService;

    public StripePaymentService(IOptions<StripeOptions> options, IExchangeRateService exchangeRateService)
    {
        _options = options.Value;
        _exchangeRateService = exchangeRateService;
    }

    public async Task<Session> CreateBalanceTopUpSessionAsync(AppUser user, decimal amountAzn, HttpRequest request, CancellationToken cancellationToken)
    {
        Stripe.StripeConfiguration.ApiKey = _options.SecretKey;
        var currency = string.IsNullOrWhiteSpace(_options.Currency) ? "usd" : _options.Currency.Trim().ToLowerInvariant();
        var chargeAmount = amountAzn;

        if (currency == "usd")
        {
            var rate = await _exchangeRateService.GetUsdToAznAsync(cancellationToken);
            chargeAmount = decimal.Round(amountAzn / rate.UsdToAzn, 2, MidpointRounding.AwayFromZero);
        }

        var sessionOptions = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = BuildAbsoluteUrl(request, "/wallet/topup/success?session_id={CHECKOUT_SESSION_ID}"),
            CancelUrl = BuildAbsoluteUrl(request, "/profile"),
            ClientReferenceId = user.Id.ToString(CultureInfo.InvariantCulture),
            CustomerEmail = user.Account.Email,
            Metadata = new Dictionary<string, string>
            {
                ["userId"] = user.Id.ToString(CultureInfo.InvariantCulture),
                ["amountAzn"] = amountAzn.ToString("F2", CultureInfo.InvariantCulture)
            },
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = currency,
                        UnitAmount = ToMinorUnits(chargeAmount),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "El-Sim balance top-up",
                            Description = $"{amountAzn:F2} AZN wallet balance"
                        }
                    }
                }
            ]
        };

        return await new SessionService().CreateAsync(sessionOptions, cancellationToken: cancellationToken);
    }

    public async Task<Session> GetSessionAsync(string sessionId, CancellationToken cancellationToken)
    {
        Stripe.StripeConfiguration.ApiKey = _options.SecretKey;

        return await new SessionService().GetAsync(sessionId, cancellationToken: cancellationToken);
    }

    private static long ToMinorUnits(decimal amount)
    {
        return (long)decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
    }

    private static string BuildAbsoluteUrl(HttpRequest request, string path)
    {
        return $"{request.Scheme}://{request.Host}{path}";
    }
}
