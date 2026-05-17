namespace El_Sim.Web.Models;

public class ProductCardViewModel
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string NameAz { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string PriceRu { get; set; } = string.Empty;
    public string PriceAz { get; set; } = string.Empty;
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
}

public class HomeProductsViewModel
{
    public List<ProductCardViewModel> EsimProducts { get; set; } = [];
    public List<HomeSliderViewModel> DesktopSliders { get; set; } = [];
    public List<HomeSliderViewModel> MobileSliders { get; set; } = [];
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
    public List<AdminProductItemViewModel> Products { get; set; } = [];
}

public class AdminProductItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string NameAz { get; set; } = string.Empty;
    public string Price { get; set; } = string.Empty;
    public string PriceRu { get; set; } = string.Empty;
    public string PriceAz { get; set; } = string.Empty;
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
