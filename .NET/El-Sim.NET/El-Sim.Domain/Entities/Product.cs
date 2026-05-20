namespace El_Sim.Domain.Entities;

public class Product
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
    public ICollection<ProductPurchase> Purchases { get; set; } = new List<ProductPurchase>();
}

