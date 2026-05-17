namespace El_Sim.Web.Controllers;

public class AdminController : Controller
{
    private readonly ElSimDbContext _dbContext;

    public AdminController(ElSimDbContext dbContext)
    {
        _dbContext = dbContext;
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
    public async Task<IActionResult> ToggleBlock(int id, string? search)
    {
        var currentUser = await GetCurrentUser();

        var user = await _dbContext.Users.FindAsync(id);

        if (user is null)
        {
            return RedirectToAction(nameof(Users), new { search });
        }

        if (currentUser is not null && user.Id == currentUser.Id)
        {
            TempData["AdminError"] = "You cannot block your own account.";
            return RedirectToAction(nameof(Users), new { search });
        }

        user.IsBlocked = !user.IsBlocked;
        await _dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Users), new { search });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleTwoFactor(int id, string? search)
    {
        var user = await _dbContext.Users.FindAsync(id);

        if (user is null)
        {
            return RedirectToAction(nameof(Users), new { search });
        }

        user.IsTwoFactorEnabled = !user.IsTwoFactorEnabled;

        if (!user.IsTwoFactorEnabled)
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
                (user.Email != null && user.Email.Contains(normalizedSearch)));
        }

        var users = await userQuery
            .OrderByDescending(user => user.IsAdmin)
            .ThenBy(user => user.IsBlocked)
            .ThenByDescending(user => user.Id)
            .Select(user => new AdminUserRowViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Fin = user.Fin,
                Email = user.Email ?? string.Empty,
                CreatedDate = user.CreatedDate.ToString("dd.MM.yy", CultureInfo.InvariantCulture),
                ProfileImagePath = user.ProfileImagePath ?? string.Empty,
                IsAdmin = user.IsAdmin,
                IsBlocked = user.IsBlocked,
                IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                IsEmailNotificationsEnabled = user.IsEmailNotificationsEnabled
            })
            .ToListAsync();

        return new AdminDashboardViewModel
        {
            TotalUsers = await allUsers.CountAsync(),
            AdminUsers = await allUsers.CountAsync(user => user.IsAdmin),
            BlockedUsers = await allUsers.CountAsync(user => user.IsBlocked),
            TwoFactorUsers = await allUsers.CountAsync(user => user.IsTwoFactorEnabled),
            EmailNotificationUsers = await allUsers.CountAsync(user => user.IsEmailNotificationsEnabled),
            PendingTwoFactorCodes = await _dbContext.TwoFactorCodes.CountAsync(code => code.ExpiresAtUtc > DateTime.UtcNow),
            SearchQuery = normalizedSearch,
            Users = users
        };
    }

    private async Task<AppUser?> GetCurrentUser()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        return int.TryParse(id, out var userId) ? await _dbContext.Users.FindAsync(userId) : null;
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

}
