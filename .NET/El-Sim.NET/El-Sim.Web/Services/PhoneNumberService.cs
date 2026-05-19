namespace El_Sim.Web.Services;

public class PhoneNumberService
{
    public static readonly string[] BasicPrefixes = ["33", "30", "37", "35"];
    public const string GlobalPrefix = "35";

    private readonly ElSimDbContext _dbContext;

    public PhoneNumberService(ElSimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GeneratedPhoneNumber> GenerateAsync(string prefix, bool isGlobal)
    {
        var normalizedPrefix = NormalizePrefix(prefix);

        if (!IsPrefixAllowed(normalizedPrefix, isGlobal))
        {
            throw new InvalidOperationException("Invalid phone prefix.");
        }

        for (var attempt = 0; attempt < 500; attempt++)
        {
            var digits = isGlobal
                ? "1" + GenerateDigits(6)
                : GenerateDigits(7);

            if (IsBlocked(digits))
            {
                continue;
            }

            var phoneNumber = FormatPhoneNumber(normalizedPrefix, digits, isGlobal);

            if (await ExistsAsync(phoneNumber))
            {
                continue;
            }

            var maxRepeat = MaxRepeatRun(digits);
            var priceMultiplier = Math.Max(1, maxRepeat - 1);

            return new GeneratedPhoneNumber(phoneNumber, normalizedPrefix, priceMultiplier);
        }

        throw new InvalidOperationException("Could not generate a unique number.");
    }

    public static string SettingKey(string prefix, bool isGlobal)
    {
        return isGlobal
            ? $"ActivationGlobalPrefix{NormalizePrefix(prefix)}Price"
            : $"ActivationBasicPrefix{NormalizePrefix(prefix)}Price";
    }

    public static string CurrencySettingKey(string prefix, bool isGlobal)
    {
        return isGlobal
            ? $"ActivationGlobalPrefix{NormalizePrefix(prefix)}Currency"
            : $"ActivationBasicPrefix{NormalizePrefix(prefix)}Currency";
    }

    public static bool IsPrefixAllowed(string prefix, bool isGlobal)
    {
        var normalizedPrefix = NormalizePrefix(prefix);

        return isGlobal
            ? normalizedPrefix == GlobalPrefix
            : BasicPrefixes.Contains(normalizedPrefix);
    }

    public static string NormalizePrefix(string prefix)
    {
        return new string((prefix ?? string.Empty).Where(char.IsDigit).ToArray());
    }

    private async Task<bool> ExistsAsync(string phoneNumber)
    {
        return await _dbContext.UserAssets.AnyAsync(item => item.BasicNumber == phoneNumber || item.GlobalNumber == phoneNumber)
            || await _dbContext.ProductPurchases.AnyAsync(item => item.PhoneNumber == phoneNumber);
    }

    private static string GenerateDigits(int count)
    {
        var builder = new StringBuilder(count);

        for (var index = 0; index < count; index++)
        {
            builder.Append(RandomNumberGenerator.GetInt32(0, 10));
        }

        return builder.ToString();
    }

    private static bool IsBlocked(string digits)
    {
        return digits.StartsWith("211", StringComparison.Ordinal)
            || digits.StartsWith("100", StringComparison.Ordinal)
            || digits.EndsWith("0000", StringComparison.Ordinal)
            || (digits.Length >= 3 && digits[0] == digits[1] && digits[1] == digits[2]);
    }

    private static int MaxRepeatRun(string digits)
    {
        var maxRun = 0;
        var run = 0;
        char? previous = null;

        foreach (var digit in digits)
        {
            run = previous == digit ? run + 1 : 1;
            maxRun = Math.Max(maxRun, run);
            previous = digit;
        }

        return maxRun;
    }

    private static string FormatPhoneNumber(string prefix, string digits, bool isGlobal)
    {
        var formatted = digits.Length == 7
            ? $"{digits[..3]} {digits.Substring(3, 2)} {digits.Substring(5, 2)}"
            : digits;

        return isGlobal ? $"+994 {prefix} {formatted} G" : $"+994 {prefix} {formatted}";
    }
}

public record GeneratedPhoneNumber(string Number, string Prefix, int PriceMultiplier);
