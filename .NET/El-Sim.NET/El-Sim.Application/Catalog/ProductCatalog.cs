namespace El_Sim.Application.Catalog;

public static class ProductCatalog
{
    public static readonly IReadOnlyList<ProductCategoryMetadata> Categories =
    [
        new("esim", "eSIM", "Home eSIM products", "digital sim", "Mobile connection without a plastic SIM card"),
        new("pass", "Pass", "Monthly membership products", "membership", "Unlock monthly El-Sim benefits, bonus data opportunities, partner rewards, and stronger network access."),
        new("tariffs", "Tariffs", "Mobile tariff products", "tariffs", "Choose a monthly package with local minutes, local SMS, and mobile internet for everyday connection."),
        new("global", "Global", "Global data products", "global beta", "Travel data packages for early users with flexible 7-day and 30-day options."),
        new("wifi", "Wi-Fi", "Optical internet products", "fiber internet", "High-speed home and business connectivity with modern Wi-Fi standards and optional static IP.")
    ];

    public static ProductCategoryMetadata? Find(string category)
    {
        return Categories.FirstOrDefault(item => string.Equals(item.Key, category, StringComparison.OrdinalIgnoreCase));
    }
}

public record ProductCategoryMetadata(string Key, string Title, string Description, string Eyebrow, string PageDescription);
