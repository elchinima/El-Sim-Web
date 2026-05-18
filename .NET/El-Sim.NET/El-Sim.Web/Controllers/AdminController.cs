namespace El_Sim.Web.Controllers;

public class AdminController : Controller
{
    private const int MaxSlidersPerType = 6;

    private readonly ElSimDbContext _dbContext;
    private readonly SliderImageProcessor _sliderImageProcessor;
    private readonly IWebHostEnvironment _environment;

    public AdminController(
        ElSimDbContext dbContext,
        SliderImageProcessor sliderImageProcessor,
        IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _sliderImageProcessor = sliderImageProcessor;
        _environment = environment;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await BuildDashboardModel(null);

        return View(nameof(Dashboard), model);
    }

    public async Task<IActionResult> Users(string? search)
    {
        var model = await BuildDashboardModel(search);

        return View(model);
    }

    public async Task<IActionResult> Products()
    {
        var counts = await _dbContext.Products
            .AsNoTracking()
            .GroupBy(product => product.Category)
            .Select(group => new { Category = group.Key, Count = group.Count() })
            .ToListAsync();

        return View(new AdminProductCategoryListViewModel
        {
            Categories = ProductCatalog.Categories
                .Select(category => new AdminProductCategoryViewModel
                {
                    Key = category.Key,
                    Title = category.Title,
                    Description = category.Description,
                    Count = counts.FirstOrDefault(item => item.Category == category.Key)?.Count ?? 0
                })
                .ToList()
        });
    }

    public async Task<IActionResult> ProductCategory(string id)
    {
        var metadata = ProductCatalog.Find(id);

        if (metadata is null)
        {
            return RedirectToAction(nameof(Products));
        }

        return View(BuildProductEditor(metadata, await GetAdminProducts(metadata.Key)));
    }

    public async Task<IActionResult> Sliders()
    {
        return View(new AdminSliderListViewModel
        {
            DesktopSliders = await GetAdminSliders(false),
            MobileSliders = await GetAdminSliders(true)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProductCategory(AdminProductEditorViewModel model, string? saveProducts)
    {
        var metadata = ProductCatalog.Find(model.Category);

        if (metadata is null)
        {
            return RedirectToAction(nameof(Products));
        }

        if (!string.Equals(saveProducts, "true", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ProductCategory), new { id = metadata.Key });
        }

        var ids = model.Products.Select(product => product.Id).ToList();
        var products = await _dbContext.Products
            .Where(product => product.Category == metadata.Key && ids.Contains(product.Id))
            .ToListAsync();

        foreach (var editedProduct in model.Products)
        {
            var product = products.FirstOrDefault(item => item.Id == editedProduct.Id);

            if (product is null)
            {
                continue;
            }

            product.Name = (editedProduct.Name ?? string.Empty).Trim();
            product.NameRu = (editedProduct.NameRu ?? string.Empty).Trim();
            product.NameAz = (editedProduct.NameAz ?? string.Empty).Trim();
            product.Price = (editedProduct.Price ?? string.Empty).Trim();
            product.PriceRu = (editedProduct.PriceRu ?? string.Empty).Trim();
            product.PriceAz = (editedProduct.PriceAz ?? string.Empty).Trim();
            product.Period = (editedProduct.Period ?? string.Empty).Trim();
            product.PeriodRu = (editedProduct.PeriodRu ?? string.Empty).Trim();
            product.PeriodAz = (editedProduct.PeriodAz ?? string.Empty).Trim();
            product.Description = (editedProduct.Description ?? string.Empty).Trim();
            product.DescriptionRu = (editedProduct.DescriptionRu ?? string.Empty).Trim();
            product.DescriptionAz = (editedProduct.DescriptionAz ?? string.Empty).Trim();
            product.Features = NormalizeLines(editedProduct.Features);
            product.FeaturesRu = NormalizeLines(editedProduct.FeaturesRu);
            product.FeaturesAz = NormalizeLines(editedProduct.FeaturesAz);
            product.ButtonText = (editedProduct.ButtonText ?? string.Empty).Trim();
            product.ButtonTextRu = (editedProduct.ButtonTextRu ?? string.Empty).Trim();
            product.ButtonTextAz = (editedProduct.ButtonTextAz ?? string.Empty).Trim();
            product.ButtonUrl = (editedProduct.ButtonUrl ?? string.Empty).Trim();
            product.IsFeatured = editedProduct.IsFeatured;
            product.IsFavorite = editedProduct.IsFavorite;
            product.SortOrder = editedProduct.SortOrder;
        }

        await _dbContext.SaveChangesAsync();
        TempData["AdminMessage"] = "Products updated.";

        return RedirectToAction(nameof(ProductCategory), new { id = metadata.Key });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddProduct(string category)
    {
        var metadata = ProductCatalog.Find(category);

        if (metadata is null)
        {
            return RedirectToAction(nameof(Products));
        }

        var nextSortOrder = await _dbContext.Products
            .Where(product => product.Category == metadata.Key)
            .Select(product => (int?)product.SortOrder)
            .MaxAsync() ?? 0;

        _dbContext.Products.Add(new Product
        {
            Category = metadata.Key,
            Name = "New product",
            NameRu = "New product",
            NameAz = "New product",
            Price = "0.00",
            PriceRu = "0.00",
            PriceAz = "0.00",
            Period = string.Empty,
            Description = string.Empty,
            DescriptionRu = string.Empty,
            DescriptionAz = string.Empty,
            Features = "Feature",
            FeaturesRu = "Feature",
            FeaturesAz = "Feature",
            ButtonText = "Choose",
            ButtonTextRu = "Choose",
            ButtonTextAz = "Choose",
            ButtonUrl = "#",
            SortOrder = nextSortOrder + 10
        });

        await _dbContext.SaveChangesAsync();
        TempData["AdminMessage"] = "Product added.";

        return RedirectToAction(nameof(ProductCategory), new { id = metadata.Key });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id, string category)
    {
        var metadata = ProductCatalog.Find(category);

        if (metadata is null)
        {
            return RedirectToAction(nameof(Products));
        }

        var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == id && item.Category == metadata.Key);

        if (product is not null)
        {
            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            TempData["AdminMessage"] = "Product deleted.";
        }

        return RedirectToAction(nameof(ProductCategory), new { id = metadata.Key });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddSlider(IFormFile sliderImage, bool isMobile, string? language, string? altText)
    {
        var sliderLanguages = GetSliderLanguages(language);

        if (sliderImage is null || sliderImage.Length == 0)
        {
            TempData["AdminError"] = "Choose a slider image.";
            return RedirectToAction(nameof(Sliders));
        }

        if (sliderImage.Length > 3 * 1024 * 1024)
        {
            TempData["AdminError"] = "Slider image must be 3 MB or less.";
            return RedirectToAction(nameof(Sliders));
        }

        var extension = Path.GetExtension(sliderImage.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".gif", ".png", ".jpg", ".jpeg" };

        if (!allowedExtensions.Contains(extension))
        {
            TempData["AdminError"] = "Only gif, png, jpg images are accepted.";
            return RedirectToAction(nameof(Sliders));
        }

        var fullLanguage = await _dbContext.HomeSliders
            .Where(slider => sliderLanguages.Contains(slider.Language) && slider.IsMobile == isMobile)
            .GroupBy(slider => slider.Language)
            .Where(group => group.Count() >= MaxSlidersPerType)
            .Select(group => group.Key)
            .FirstOrDefaultAsync();

        if (!string.IsNullOrWhiteSpace(fullLanguage))
        {
            TempData["AdminError"] = isMobile
                ? "Mobile sliders limit is 6."
                : "Desktop sliders limit is 6.";
            return RedirectToAction(nameof(Sliders));
        }

        var nextSortOrders = await _dbContext.HomeSliders
            .Where(slider => sliderLanguages.Contains(slider.Language) && slider.IsMobile == isMobile)
            .GroupBy(slider => slider.Language)
            .Select(group => new { Language = group.Key, SortOrder = group.Max(slider => slider.SortOrder) })
            .ToListAsync();

        foreach (var sliderLanguage in sliderLanguages)
        {
            var uploadRoot = Path.Combine(_environment.WebRootPath, "Uploads", "Sliders", sliderLanguage, isMobile ? "Mobile" : "Desktop");
            Directory.CreateDirectory(uploadRoot);
            var fileName = $"{Guid.NewGuid():N}.webp";
            var outputPath = Path.Combine(uploadRoot, fileName);

            try
            {
                await using var stream = sliderImage.OpenReadStream();
                await _sliderImageProcessor.SaveWebpAsync(stream, outputPath, isMobile, HttpContext.RequestAborted);
            }
            catch
            {
                TempData["AdminError"] = "Slider image could not be processed.";
                return RedirectToAction(nameof(Sliders));
            }

            var nextSortOrder = nextSortOrders.FirstOrDefault(item => item.Language == sliderLanguage)?.SortOrder ?? 0;

            _dbContext.HomeSliders.Add(new HomeSlider
            {
                ImagePath = $"/Uploads/Sliders/{sliderLanguage}/{(isMobile ? "Mobile" : "Desktop")}/{fileName}",
                AltText = (altText ?? string.Empty).Trim(),
                Language = sliderLanguage,
                IsMobile = isMobile,
                SortOrder = nextSortOrder + 10,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await _dbContext.SaveChangesAsync();
        TempData["AdminMessage"] = "Slider added.";

        return RedirectToAction(nameof(Sliders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSliders(AdminSliderUpdateViewModel model)
    {
        var editedSliders = model.DesktopSliders.Concat(model.MobileSliders).ToList();
        var ids = editedSliders.Select(slider => slider.Id).ToList();
        var sliders = await _dbContext.HomeSliders
            .Where(slider => ids.Contains(slider.Id))
            .ToListAsync();

        foreach (var editedSlider in editedSliders)
        {
            var slider = sliders.FirstOrDefault(item => item.Id == editedSlider.Id);

            if (slider is null)
            {
                continue;
            }

            slider.AltText = (editedSlider.AltText ?? string.Empty).Trim();
            slider.SortOrder = editedSlider.SortOrder;
        }

        await _dbContext.SaveChangesAsync();
        TempData["AdminMessage"] = "Sliders updated.";

        return RedirectToAction(nameof(Sliders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteSlider(int id)
    {
        var slider = await _dbContext.HomeSliders.FindAsync(id);

        if (slider is not null)
        {
            DeleteSliderImage(slider.ImagePath);
            _dbContext.HomeSliders.Remove(slider);
            await _dbContext.SaveChangesAsync();
            TempData["AdminMessage"] = "Slider deleted.";
        }

        return RedirectToAction(nameof(Sliders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSliderImage(int id, IFormFile sliderImage)
    {
        var slider = await _dbContext.HomeSliders.FindAsync(id);

        if (slider is null)
        {
            return RedirectToAction(nameof(Sliders));
        }

        if (sliderImage is null || sliderImage.Length == 0)
        {
            TempData["AdminError"] = "Choose a slider image.";
            return RedirectToAction(nameof(Sliders));
        }

        if (sliderImage.Length > 3 * 1024 * 1024)
        {
            TempData["AdminError"] = "Slider image must be 3 MB or less.";
            return RedirectToAction(nameof(Sliders));
        }

        var extension = Path.GetExtension(sliderImage.FileName).ToLowerInvariant();
        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".gif", ".png", ".jpg", ".jpeg" };

        if (!allowedExtensions.Contains(extension))
        {
            TempData["AdminError"] = "Only gif, png, jpg images are accepted.";
            return RedirectToAction(nameof(Sliders));
        }

        var uploadRoot = Path.Combine(_environment.WebRootPath, "Uploads", "Sliders", slider.Language, slider.IsMobile ? "Mobile" : "Desktop");
        Directory.CreateDirectory(uploadRoot);
        var fileName = $"{Guid.NewGuid():N}.webp";
        var outputPath = Path.Combine(uploadRoot, fileName);
        var oldImagePath = slider.ImagePath;

        try
        {
            await using var stream = sliderImage.OpenReadStream();
            await _sliderImageProcessor.SaveWebpAsync(stream, outputPath, slider.IsMobile, HttpContext.RequestAborted);
        }
        catch
        {
            TempData["AdminError"] = "Slider image could not be processed.";
            return RedirectToAction(nameof(Sliders));
        }

        slider.ImagePath = $"/Uploads/Sliders/{slider.Language}/{(slider.IsMobile ? "Mobile" : "Desktop")}/{fileName}";
        await _dbContext.SaveChangesAsync();
        DeleteSliderImage(oldImagePath);
        TempData["AdminMessage"] = "Slider image updated.";

        return RedirectToAction(nameof(Sliders));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleBlock(int id, string? search)
    {
        var currentUser = await GetCurrentUser();

        var user = await _dbContext.Users
            .Include(item => item.Account)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
        {
            return RedirectToAction(nameof(Users), new { search });
        }

        if (currentUser is not null && user.Id == currentUser.Id)
        {
            TempData["AdminError"] = "You cannot block your own account.";
            return RedirectToAction(nameof(Users), new { search });
        }

        user.Account.IsBlocked = !user.Account.IsBlocked;
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Users), new { search });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTwoFactor(int id, string? search)
    {
        var user = await _dbContext.Users
            .Include(item => item.Account)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (user is null)
        {
            return RedirectToAction(nameof(Users), new { search });
        }

        user.Account.IsTwoFactorEnabled = !user.Account.IsTwoFactorEnabled;

        if (!user.Account.IsTwoFactorEnabled)
        {
            var codes = _dbContext.TwoFactorCodes.Where(code => code.AppUserId == user.Id);
            _dbContext.TwoFactorCodes.RemoveRange(codes);
        }

        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Users), new { search });
    }

    private async Task<AdminDashboardViewModel> BuildDashboardModel(string? search)
    {
        var allUsers = _dbContext.Users.AsNoTracking();
        var userQuery = allUsers;
        var normalizedSearch = search?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            userQuery = userQuery.Where(user =>
                user.Name.Contains(normalizedSearch) ||
                user.Fin.Contains(normalizedSearch) ||
                (user.Account.Email != null && user.Account.Email.Contains(normalizedSearch)));
        }

        var users = await userQuery
            .OrderByDescending(user => user.Account.IsAdmin)
            .ThenBy(user => user.Account.IsBlocked)
            .ThenByDescending(user => user.Id)
            .Select(user => new AdminUserRowViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Fin = user.Fin,
                Email = user.Account.Email ?? string.Empty,
                CreatedDate = user.CreatedDate.ToString("dd.MM.yy", CultureInfo.InvariantCulture),
                ProfileImagePath = user.Account.ProfileImagePath ?? string.Empty,
                IsAdmin = user.Account.IsAdmin,
                IsBlocked = user.Account.IsBlocked,
                IsTwoFactorEnabled = user.Account.IsTwoFactorEnabled,
                IsEmailNotificationsEnabled = user.Account.IsEmailNotificationsEnabled
            })
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            TotalUsers = await allUsers.CountAsync(),
            AdminUsers = await allUsers.CountAsync(user => user.Account.IsAdmin),
            BlockedUsers = await allUsers.CountAsync(user => user.Account.IsBlocked),
            TwoFactorUsers = await allUsers.CountAsync(user => user.Account.IsTwoFactorEnabled),
            EmailNotificationUsers = await allUsers.CountAsync(user => user.Account.IsEmailNotificationsEnabled),
            PendingTwoFactorCodes = await _dbContext.TwoFactorCodes.CountAsync(code => code.ExpiresAtUtc > DateTime.UtcNow),
            SearchQuery = normalizedSearch,
            Users = users
        };
    }

    private async Task<AppUser?> GetCurrentUser()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(id, out var userId)
            ? await _dbContext.Users
                .Include(item => item.Account)
                .FirstOrDefaultAsync(item => item.Id == userId)
            : null;
    }

    private async Task<List<AdminProductItemViewModel>> GetAdminProducts(string category)
    {
        return await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Category == category)
            .OrderBy(product => product.SortOrder)
            .ThenBy(product => product.Id)
            .Select(product => new AdminProductItemViewModel
            {
                Id = product.Id,
                Name = product.Name,
                NameRu = product.NameRu,
                NameAz = product.NameAz,
                Price = product.Price,
                PriceRu = product.PriceRu,
                PriceAz = product.PriceAz,
                Period = product.Period,
                PeriodRu = product.PeriodRu,
                PeriodAz = product.PeriodAz,
                Description = product.Description,
                DescriptionRu = product.DescriptionRu,
                DescriptionAz = product.DescriptionAz,
                Features = product.Features,
                FeaturesRu = product.FeaturesRu,
                FeaturesAz = product.FeaturesAz,
                ButtonText = product.ButtonText,
                ButtonTextRu = product.ButtonTextRu,
                ButtonTextAz = product.ButtonTextAz,
                ButtonUrl = product.ButtonUrl,
                IsFeatured = product.IsFeatured,
                IsFavorite = product.IsFavorite,
                SortOrder = product.SortOrder
            })
            .ToListAsync();
    }

    private async Task<List<HomeSliderViewModel>> GetAdminSliders(bool isMobile)
    {
        return await _dbContext.HomeSliders
            .AsNoTracking()
            .Where(slider => slider.IsMobile == isMobile)
            .OrderBy(slider => slider.Language == "en" ? 0 : slider.Language == "ru" ? 1 : 2)
            .ThenBy(slider => slider.SortOrder)
            .ThenBy(slider => slider.Id)
            .Select(slider => new HomeSliderViewModel
                {
                    Id = slider.Id,
                    ImagePath = slider.ImagePath,
                    AltText = slider.AltText,
                    Language = slider.Language,
                    IsMobile = slider.IsMobile,
                    SortOrder = slider.SortOrder
                })
            .ToListAsync();
    }

    private static AdminProductEditorViewModel BuildProductEditor(ProductCategoryMetadata metadata, List<AdminProductItemViewModel> products)
    {
        return new AdminProductEditorViewModel
        {
            Category = metadata.Key,
            Title = metadata.Title,
            Description = metadata.Description,
            Products = products
        };
    }

    private static string NormalizeLines(string? value)
    {
        return string.Join('\n', (value ?? string.Empty)
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string NormalizeSliderLanguage(string? language)
    {
        return language?.Trim().ToLowerInvariant() switch
        {
            "ru" => "ru",
            "az" => "az",
            _ => "en"
        };
    }

    private static List<string> GetSliderLanguages(string? language)
    {
        return string.Equals(language?.Trim(), "all", StringComparison.OrdinalIgnoreCase)
            ? ["en", "ru", "az"]
            : [NormalizeSliderLanguage(language)];
    }

    private void DeleteSliderImage(string? sliderImagePath)
    {
        if (string.IsNullOrWhiteSpace(sliderImagePath))
        {
            return;
        }

        var relativePath = sliderImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relativePath));
        var sliderRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "Uploads", "Sliders"));

        if (fullPath.StartsWith(sliderRoot, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}
