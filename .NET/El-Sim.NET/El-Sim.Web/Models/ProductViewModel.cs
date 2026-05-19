namespace El_Sim.Web.Models;

public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string ProductType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string NameAz { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string PriceRu { get; set; } = string.Empty;
    public string PriceAz { get; set; } = string.Empty;
    public string Currency { get; set; } = "AZN";
    public decimal Amount { get; set; }
    public decimal TotalAzn { get; set; }
    public decimal? ExchangeRate { get; set; }
    public decimal CommissionRate { get; set; }
    public string DisplayPrice => FormatPrice(Price, Currency);
    public string DisplayPriceRu => FormatPrice(string.IsNullOrWhiteSpace(PriceRu) ? Price : PriceRu, Currency);
    public string DisplayPriceAz => FormatPrice(string.IsNullOrWhiteSpace(PriceAz) ? Price : PriceAz, Currency);
    public string Period { get; set; } = string.Empty;
    public string PeriodRu { get; set; } = string.Empty;
    public string PeriodAz { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionRu { get; set; } = string.Empty;
    public string DescriptionAz { get; set; } = string.Empty;
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonTextRu { get; set; } = string.Empty;
    public string ButtonTextAz { get; set; } = string.Empty;
    public string ButtonUrl { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsFavorite { get; set; }
    public int SortOrder { get; set; }
    public List<string> Features { get; set; } = [];
    public List<string> FeaturesRu { get; set; } = [];
    public List<string> FeaturesAz { get; set; } = [];

    private static string FormatPrice(string price, string currency)
    {
        var cleanPrice = (price ?? string.Empty)
            .Replace("$", string.Empty)
            .Replace("USD", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("AZN", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        return string.IsNullOrWhiteSpace(cleanPrice) ? string.Empty : $"{cleanPrice} {currency}";
    }
}

public class HomeProductsViewModel
{
    public List<ProductCardViewModel> EsimProducts { get; set; } = [];
    public List<HomeSliderViewModel> DesktopSliders { get; set; } = [];
    public List<HomeSliderViewModel> MobileSliders { get; set; } = [];
    public ExchangeRateResult? ExchangeRate { get; set; }
}

public class HomeSliderViewModel
{
    public int Id { get; set; }
    public string ImagePath { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public bool IsMobile { get; set; }
    public int SortOrder { get; set; }
}

public class ProductCategoryPageViewModel
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Eyebrow { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ExchangeRateResult? ExchangeRate { get; set; }
    public decimal WifiStaticIpPercent { get; set; } = 5m;
    public List<ProductCardViewModel> Products { get; set; } = [];
}

public class AdminProductCategoryListViewModel
{
    public List<AdminProductCategoryViewModel> Categories { get; set; } = [];
}

public class AdminProductCategoryViewModel
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class AdminProductEditorViewModel
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal WifiStaticIpPercent { get; set; } = 5m;
    public List<PrefixPriceViewModel> PrefixPrices { get; set; } = [];
    public List<AdminProductItemViewModel> Products { get; set; } = [];
}

public class AdminProductItemViewModel
{
    public int Id { get; set; }
    public string ProductType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string NameAz { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string PriceRu { get; set; } = string.Empty;
    public string PriceAz { get; set; } = string.Empty;
    public string Currency { get; set; } = "AZN";
    public string DisplayPrice => FormatPrice(Price, Currency);
    public string Period { get; set; } = string.Empty;
    public string PeriodRu { get; set; } = string.Empty;
    public string PeriodAz { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string DescriptionRu { get; set; } = string.Empty;
    public string DescriptionAz { get; set; } = string.Empty;
    public string Features { get; set; } = string.Empty;
    public string FeaturesRu { get; set; } = string.Empty;
    public string FeaturesAz { get; set; } = string.Empty;
    public string ButtonText { get; set; } = string.Empty;
    public string ButtonTextRu { get; set; } = string.Empty;
    public string ButtonTextAz { get; set; } = string.Empty;
    public string ButtonUrl { get; set; } = string.Empty;
    public bool IsFeatured { get; set; }
    public bool IsFavorite { get; set; }
    public int SortOrder { get; set; }

    private static string FormatPrice(string price, string currency)
    {
        var cleanPrice = (price ?? string.Empty)
            .Replace("$", string.Empty)
            .Replace("USD", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("AZN", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();

        return string.IsNullOrWhiteSpace(cleanPrice) ? string.Empty : $"{cleanPrice} {currency}";
    }
}

public class PrefixPriceViewModel
{
    public string Prefix { get; set; } = string.Empty;
    public decimal BasicPrice { get; set; }
    public string BasicCurrency { get; set; } = "AZN";
    public decimal GlobalPrice { get; set; }
    public string GlobalCurrency { get; set; } = "AZN";
    public bool SupportsGlobal { get; set; }
}

public class ActivationPageViewModel
{
    public decimal BalanceAzn { get; set; }
    public ExchangeRateResult? ExchangeRate { get; set; }
    public List<PrefixPriceViewModel> PrefixPrices { get; set; } = [];
}

public class AdminSliderListViewModel
{
    public List<HomeSliderViewModel> DesktopSliders { get; set; } = [];
    public List<HomeSliderViewModel> MobileSliders { get; set; } = [];
}

public class AdminSliderUpdateViewModel
{
    public List<HomeSliderViewModel> DesktopSliders { get; set; } = [];
    public List<HomeSliderViewModel> MobileSliders { get; set; } = [];
}
