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

}
