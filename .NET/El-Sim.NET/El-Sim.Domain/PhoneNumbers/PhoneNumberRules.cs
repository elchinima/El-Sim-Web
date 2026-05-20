namespace El_Sim.Domain.PhoneNumbers;

public static class PhoneNumberRules
{
    public static readonly string[] BasicPrefixes = ["33", "30", "37", "35"];
    public const string GlobalPrefix = "35";

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
}
