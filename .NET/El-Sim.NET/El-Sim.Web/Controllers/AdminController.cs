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

        return View(await BuildProductEditorWithSettings(metadata, await GetAdminProducts(metadata.Key)));
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
            product.ProductType = NormalizeProductType(metadata.Key, editedProduct.ProductType);
            product.NameRu = (editedProduct.NameRu ?? string.Empty).Trim();
            product.NameAz = (editedProduct.NameAz ?? string.Empty).Trim();
            product.Price = NormalizePrice(editedProduct.Price);
            product.PriceRu = NormalizePrice(editedProduct.PriceRu);
            product.PriceAz = NormalizePrice(editedProduct.PriceAz);
            product.Currency = NormalizeCurrency(editedProduct.Currency);
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

        if (metadata.Key == "wifi")
        {
            var staticIpPercent = Math.Clamp(model.WifiStaticIpPercent, 0m, 100m);
            await SaveSetting("WifiStaticIpPercent", staticIpPercent.ToString("0.####", CultureInfo.InvariantCulture));
        }

        if (metadata.Key == "esim")
        {
            foreach (var prefixPrice in model.PrefixPrices)
            {
                var prefix = PhoneNumberService.NormalizePrefix(prefixPrice.Prefix);

                if (!PhoneNumberService.IsPrefixAllowed(prefix, false))
                {
                    continue;
                }

                await SaveSetting(PhoneNumberService.SettingKey(prefix, false), Math.Max(0m, prefixPrice.BasicPrice).ToString("0.##", CultureInfo.InvariantCulture));
                await SaveSetting(PhoneNumberService.CurrencySettingKey(prefix, false), NormalizeCurrency(prefixPrice.BasicCurrency));

                if (prefix == PhoneNumberService.GlobalPrefix)
                {
                    await SaveSetting(PhoneNumberService.SettingKey(prefix, true), Math.Max(0m, prefixPrice.GlobalPrice).ToString("0.##", CultureInfo.InvariantCulture));
                    await SaveSetting(PhoneNumberService.CurrencySettingKey(prefix, true), NormalizeCurrency(prefixPrice.GlobalCurrency));
                }
            }
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
            ProductType = metadata.Key == "esim" ? "basic" : NormalizeProductType(metadata.Key, null),
            Name = "New product",
            NameRu = "New product",
            NameAz = "New product",
            Price = "0.00",
            PriceRu = "0.00",
            PriceAz = "0.00",
            Currency = "AZN",
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

    public async Task<IActionResult> Purchases(string? search)
    {
        var normalizedSearch = search?.Trim() ?? string.Empty;
        var query = _dbContext.ProductPurchases
            .AsNoTracking()
            .Include(item => item.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(item =>
                item.ProductName.Contains(normalizedSearch) ||
                item.Category.Contains(normalizedSearch) ||
                (item.User != null && (item.User.Name.Contains(normalizedSearch) || item.User.Fin.Contains(normalizedSearch))));
        }

        var purchases = await query
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(200)
            .Select(item => new AdminPurchaseRowViewModel
            {
                Id = item.Id,
                UserName = item.User != null ? item.User.Name : string.Empty,
                UserFin = item.User != null ? item.User.Fin : string.Empty,
                Category = item.Category,
                ProductName = item.ProductName,
                ProductCurrency = item.ProductCurrency,
                ProductAmount = item.ProductAmount,
                TotalAzn = item.TotalAzn,
                ExchangeRate = item.ExchangeRate,
                CommissionRate = item.CommissionRate,
                HasStaticIp = item.HasStaticIp,
                StaticIpRate = item.StaticIpRate,
                StaticIpFeeAzn = item.StaticIpFeeAzn,
                Status = item.Status,
                CreatedAtUtc = item.CreatedAtUtc
            })
            .ToListAsync();

        return View(new AdminPurchasesViewModel
        {
            SearchQuery = normalizedSearch,
            Purchases = purchases
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelPurchase(int id, string? search)
    {
        var purchase = await _dbContext.ProductPurchases
            .Include(item => item.User)
            .ThenInclude(item => item!.UserAssets)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (purchase is not null && purchase.Status == "Active")
        {
            purchase.Status = "Cancelled";
            purchase.CancelledAtUtc = DateTime.UtcNow;
            purchase.AdminNote = "Cancelled by admin.";
            await RefreshUserAsset(purchase.User, purchase.Category, purchase.ProductType, purchase.Id);

            _dbContext.PaymentReceipts.Add(BuildAdminReceipt(purchase, "Cancel", "Cancelled", 0m));
            await _dbContext.SaveChangesAsync();
            TempData["AdminMessage"] = "Purchase cancelled.";
        }

        return RedirectToAction(nameof(Purchases), new { search });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RefundPurchase(int id, string? search)
    {
        var purchase = await _dbContext.ProductPurchases
            .Include(item => item.User)
            .ThenInclude(item => item!.Account)
            .Include(item => item.User)
            .ThenInclude(item => item!.UserAssets)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (purchase?.User is not null && purchase.Status != "Refunded")
        {
            purchase.Status = "Refunded";
            purchase.RefundedAtUtc = DateTime.UtcNow;
            purchase.CancelledAtUtc ??= DateTime.UtcNow;
            purchase.AdminNote = "Refunded by admin.";
            purchase.User.Account.BalanceAzn += purchase.TotalAzn;

            _dbContext.WalletTransactions.Add(new WalletTransaction
            {
                UserId = purchase.UserId,
                ProductPurchaseId = purchase.Id,
                Type = "Refund",
                Status = "Paid",
                AmountAzn = purchase.TotalAzn,
                BalanceAfterAzn = purchase.User.Account.BalanceAzn,
                Description = purchase.ProductName,
                CreatedAtUtc = DateTime.UtcNow
            });

            await RefreshUserAsset(purchase.User, purchase.Category, purchase.ProductType, purchase.Id);
            _dbContext.PaymentReceipts.Add(BuildAdminReceipt(purchase, "Refund", "Refunded", purchase.TotalAzn));
            await _dbContext.SaveChangesAsync();
            TempData["AdminMessage"] = "Purchase refunded.";
        }

        return RedirectToAction(nameof(Purchases), new { search });
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
            var hasPurchases = await _dbContext.ProductPurchases.AnyAsync(item => item.ProductId == product.Id);

            if (hasPurchases)
            {
                TempData["AdminError"] = "Products with purchase history cannot be deleted.";
                return RedirectToAction(nameof(ProductCategory), new { id = metadata.Key });
            }

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
                ProductType = product.ProductType,
                Name = product.Name,
                NameRu = product.NameRu,
                NameAz = product.NameAz,
                Price = product.Price,
                PriceRu = product.PriceRu,
                PriceAz = product.PriceAz,
                Currency = product.Currency,
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

    private async Task<AdminProductEditorViewModel> BuildProductEditorWithSettings(ProductCategoryMetadata metadata, List<AdminProductItemViewModel> products)
    {
        var model = BuildProductEditor(metadata, products);

        if (metadata.Key == "wifi")
        {
            model.WifiStaticIpPercent = await GetDecimalSetting("WifiStaticIpPercent", 5m);
        }

        if (metadata.Key == "esim")
        {
            model.PrefixPrices = await GetPrefixPrices();
        }

        return model;
    }

    private async Task<List<PrefixPriceViewModel>> GetPrefixPrices()
    {
        var result = new List<PrefixPriceViewModel>();

        foreach (var prefix in PhoneNumberService.BasicPrefixes)
        {
            result.Add(new PrefixPriceViewModel
            {
                Prefix = prefix,
                BasicPrice = await GetDecimalSetting(PhoneNumberService.SettingKey(prefix, false), 5m),
                BasicCurrency = await GetStringSetting(PhoneNumberService.CurrencySettingKey(prefix, false), "AZN"),
                GlobalPrice = prefix == PhoneNumberService.GlobalPrefix
                    ? await GetDecimalSetting(PhoneNumberService.SettingKey(prefix, true), 10m)
                    : 0m,
                GlobalCurrency = prefix == PhoneNumberService.GlobalPrefix
                    ? await GetStringSetting(PhoneNumberService.CurrencySettingKey(prefix, true), "AZN")
                    : "AZN",
                SupportsGlobal = prefix == PhoneNumberService.GlobalPrefix
            });
        }

        return result;
    }

    private async Task<decimal> GetDecimalSetting(string key, decimal fallback)
    {
        var value = await _dbContext.AppSettings
            .AsNoTracking()
            .Where(setting => setting.Key == key)
            .Select(setting => setting.Value)
            .FirstOrDefaultAsync();

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }

    private async Task<string> GetStringSetting(string key, string fallback)
    {
        var value = await _dbContext.AppSettings
            .AsNoTracking()
            .Where(setting => setting.Key == key)
            .Select(setting => setting.Value)
            .FirstOrDefaultAsync();

        return NormalizeCurrency(string.IsNullOrWhiteSpace(value) ? fallback : value);
    }

    private async Task SaveSetting(string key, string value)
    {
        var setting = await _dbContext.AppSettings.FirstOrDefaultAsync(item => item.Key == key);

        if (setting is null)
        {
            _dbContext.AppSettings.Add(new AppSetting { Key = key, Value = value });
            return;
        }

        setting.Value = value;
    }

    private static string NormalizeLines(string? value)
    {
        return string.Join('\n', (value ?? string.Empty)
            .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));
    }

    private static string NormalizeCurrency(string? currency)
    {
        return string.Equals(currency?.Trim(), "USD", StringComparison.OrdinalIgnoreCase) ? "USD" : "AZN";
    }

    private static string NormalizePrice(string? price)
    {
        return (price ?? string.Empty)
            .Replace("$", string.Empty)
            .Replace("USD", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("AZN", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    private static string NormalizeProductType(string category, string? productType)
    {
        var normalizedType = (productType ?? string.Empty).Trim().ToLowerInvariant();

        if (category == "esim")
        {
            return normalizedType is "global" or "pass" or "tariff" or "basic" ? normalizedType : "basic";
        }

        return category switch
        {
            "pass" => "pass",
            "tariffs" => "tariff",
            "global" => "global",
            "wifi" => "wifi",
            _ => normalizedType
        };
    }

    private async Task RefreshUserAsset(AppUser? user, string category, string productType, int ignoredPurchaseId)
    {
        if (user?.UserAssets is null)
        {
            return;
        }

        var replacement = await _dbContext.ProductPurchases
            .AsNoTracking()
            .Where(item => item.UserId == user.Id
                && item.Category == category
                && item.ProductType == productType
                && item.Id != ignoredPurchaseId
                && item.Status == "Active")
            .OrderByDescending(item => item.CreatedAtUtc)
            .FirstOrDefaultAsync();

        var value = replacement?.ProductName;

        switch (productType)
        {
            case "basic":
                user.UserAssets.BasicNumber = replacement?.PhoneNumber ?? value;
                break;
            case "global" when category == "esim":
                user.UserAssets.GlobalNumber = replacement?.PhoneNumber ?? value;
                break;
            case "global":
                user.UserAssets.GlobalTariff = value;
                break;
            case "pass":
                user.UserAssets.Pass = value;
                break;
            case "tariff":
                user.UserAssets.BasicTariff = value;
                break;
            case "wifi":
                user.UserAssets.WiFi = value;
                break;
        }

        if (string.IsNullOrWhiteSpace(productType))
        {
            switch (category)
            {
                case "esim":
                user.UserAssets.BasicNumber = value;
                break;
                case "pass":
                user.UserAssets.Pass = value;
                break;
                case "tariffs":
                user.UserAssets.BasicTariff = value;
                break;
                case "global":
                user.UserAssets.GlobalTariff = value;
                break;
                case "wifi":
                user.UserAssets.WiFi = value;
                break;
            }
        }
    }

    private static PaymentReceipt BuildAdminReceipt(ProductPurchase purchase, string type, string status, decimal amountAzn)
    {
        return new PaymentReceipt
        {
            UserId = purchase.UserId,
            ProductPurchaseId = purchase.Id,
            ReceiptNumber = $"ES-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(1000, 9999)}",
            Type = type,
            Status = status,
            Currency = "AZN",
            OriginalAmount = amountAzn,
            AmountAzn = amountAzn,
            CommissionRate = 0m,
            Description = purchase.ProductName,
            PayloadJson = JsonSerializer.Serialize(new { purchase.Id, purchase.ProductName, purchase.Category, amountAzn }),
            CreatedAtUtc = DateTime.UtcNow
        };
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
