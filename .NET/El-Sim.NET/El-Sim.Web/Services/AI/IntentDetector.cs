namespace El_Sim.Web.Services.AI;

public class IntentDetector
{
    private static readonly Dictionary<Intent, string[]> Keywords = new()
    {
        [Intent.Plans] = ["tariff", "tariffs", "plan", "plans", "qiym\u0259t", "qiymet", "tarif", "\u0442\u0430\u0440\u0438\u0444", "\u0442\u0430\u0440\u0438\u0444\u044b", "\u0446\u0435\u043d\u0430", "\u0446\u0435\u043d\u044b", "gb", "\u0433\u0431"],
        [Intent.Pass] = ["pass", "monthly", "ayl\u0131q", "ayliq", "\u043c\u0435\u0441\u044f\u0446", "\u043c\u0435\u0441\u044f\u0447\u043d\u044b\u0439", "\u043f\u043e\u0434\u043f\u0438\u0441\u043a\u0430"],
        [Intent.Global] = ["global", "travel", "\u00f6lk\u0259", "olke", "\u0441\u0442\u0440\u0430\u043d\u0430", "\u0441\u0442\u0440\u0430\u043d\u044b", "s\u0259yah\u0259t", "seyahat", "\u043f\u0443\u0442\u0435\u0448\u0435\u0441\u0442\u0432\u0438\u0435"],
        [Intent.Wifi] = ["wifi", "wi-fi", "\u043e\u043f\u0442\u0438\u043a\u0430", "internet", "\u0438\u043d\u0442\u0435\u0440\u043d\u0435\u0442", "optika", "\u0434\u043e\u043c\u043e\u0439", "home", "fiber"],
        [Intent.Activation] = ["aktivasiya", "\u0430\u043a\u0442\u0438\u0432\u0430\u0446\u0438\u044f", "qr", "esim", "\u0443\u0441\u0442\u0430\u043d\u043e\u0432\u043a\u0430", "qura\u015fd\u0131rma", "qurasdirma", "install", "activate"],
        [Intent.Faq] = ["\u043f\u043e\u043c\u043e\u0449\u044c", "k\u00f6m\u0259k", "komek", "problem", "\u043f\u0440\u043e\u0431\u043b\u0435\u043c\u0430", "help", "faq"]
    };

    public Intent Detect(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return Intent.Unknown;
        }

        var normalized = message.Trim().ToLowerInvariant();

        foreach (var pair in Keywords)
        {
            if (pair.Value.Any(keyword => normalized.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
            {
                return pair.Key;
            }
        }

        return Intent.Unknown;
    }
}
