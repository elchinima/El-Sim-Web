namespace El_Sim.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ElSimDbContext _dbContext;
        private readonly ProfileImageProcessor _profileImageProcessor;
        private readonly EmailSender _emailSender;
        private readonly IWebHostEnvironment _environment;

        public HomeController(
            ElSimDbContext dbContext,
            ProfileImageProcessor profileImageProcessor,
            EmailSender emailSender,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _profileImageProcessor = profileImageProcessor;
            _emailSender = emailSender;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            return View(new HomeProductsViewModel
            {
                EsimProducts = await GetProducts("esim"),
                DesktopSliders = await GetSliders(false),
                MobileSliders = await GetSliders(true)
            });
        }

        public async Task<IActionResult> Plans()
        {
            return View(BuildCategoryPage("tariffs", await GetProducts("tariffs")));
        }

        public async Task<IActionResult> Pass()
        {
            return View(BuildCategoryPage("pass", await GetProducts("pass")));
        }

        public async Task<IActionResult> Global()
        {
            return View(BuildCategoryPage("global", await GetProducts("global")));
        }

        public async Task<IActionResult> Wifi()
        {
            return View(BuildCategoryPage("wifi", await GetProducts("wifi")));
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction(nameof(Profile));
            }

            return View(new AuthPageViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel login)
        {
            if (!ModelState.IsValid)
            {
                return View(new AuthPageViewModel { Login = login, ActiveForm = "login" });
            }

            login.Fin = login.Fin.Trim().ToUpperInvariant();
            var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Fin == login.Fin);

            if (user is null || !PasswordHasher.VerifyPassword(login.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError(string.Empty, "FIN or password is incorrect.");
                return View(new AuthPageViewModel { Login = login, ActiveForm = "login" });
            }

            if (user.IsBlocked)
            {
                ModelState.AddModelError(string.Empty, "Your account is blocked.");
                return View(new AuthPageViewModel { Login = login, ActiveForm = "login" });
            }

            if (user.IsTwoFactorEnabled)
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    ModelState.AddModelError(string.Empty, "Email is required for two-factor verification.");
                    return View(new AuthPageViewModel { Login = login, ActiveForm = "login" });
                }

                try
                {
                    await SendTwoFactorCode(user, login.RememberMe);
                }
                catch (InvalidOperationException)
                {
                    ModelState.AddModelError(string.Empty, "Email verification is not configured.");
                    return View(new AuthPageViewModel { Login = login, ActiveForm = "login" });
                }
                catch (SmtpException)
                {
                    ModelState.AddModelError(string.Empty, "Verification email could not be sent.");
                    return View(new AuthPageViewModel { Login = login, ActiveForm = "login" });
                }

                return RedirectToAction(nameof(VerifyTwoFactor), new { userId = user.Id });
            }

            await SignInUser(user, login.RememberMe);

            return RedirectToAction(nameof(Profile));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel register)
        {
            if (!ModelState.IsValid)
            {
                return View(nameof(Login), new AuthPageViewModel { Register = register, ActiveForm = "register" });
            }

            register.Fin = register.Fin.Trim().ToUpperInvariant();
            var exists = await _dbContext.Users.AnyAsync(user => user.Fin == register.Fin);

            if (exists)
            {
                ModelState.AddModelError(string.Empty, "This FIN is already registered.");
                return View(nameof(Login), new AuthPageViewModel { Register = register, ActiveForm = "register" });
            }

            var user = new AppUser
            {
                Name = register.Name.Trim(),
                Fin = register.Fin,
                CreatedDate = DateOnly.FromDateTime(DateTime.Today)
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            user.PasswordSalt = $"{user.Id}/{user.CreatedDate:dd.MM.yy}/El-Sim/Elsim";
            user.PasswordHash = PasswordHasher.HashPassword(register.Password, user.PasswordSalt);
            await _dbContext.SaveChangesAsync();
            await SignInUser(user, false);

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(id, out var userId))
            {
                return RedirectToAction(nameof(Login));
            }

            var user = await _dbContext.Users.FindAsync(userId);

            if (user is null)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToAction(nameof(Login));
            }

            return View(new ProfileViewModel
            {
                Name = user.Name,
                Fin = user.Fin,
                CreatedDate = user.CreatedDate.ToString("dd.MM.yy", CultureInfo.InvariantCulture),
                Email = user.Email ?? string.Empty,
                ProfileImagePath = user.ProfileImagePath ?? string.Empty,
                IsTwoFactorEnabled = user.IsTwoFactorEnabled,
                IsEmailNotificationsEnabled = user.IsEmailNotificationsEnabled
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(UpdateProfileViewModel profile)
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (profile.IsTwoFactorEnabled && string.IsNullOrWhiteSpace(profile.Email))
            {
                ModelState.AddModelError(nameof(profile.Email), "Email is required for two-factor verification.");
            }

            if (!ModelState.IsValid)
            {
                return View(new ProfileViewModel
                {
                    Name = profile.Name,
                    Fin = user.Fin,
                    CreatedDate = user.CreatedDate.ToString("dd.MM.yy", CultureInfo.InvariantCulture),
                    Email = profile.Email ?? string.Empty,
                    ProfileImagePath = user.ProfileImagePath ?? string.Empty,
                    IsTwoFactorEnabled = profile.IsTwoFactorEnabled,
                    IsEmailNotificationsEnabled = profile.IsEmailNotificationsEnabled
                });
            }

            user.Name = profile.Name.Trim();
            user.Email = string.IsNullOrWhiteSpace(profile.Email) ? null : profile.Email.Trim();
            user.IsTwoFactorEnabled = profile.IsTwoFactorEnabled;
            user.IsEmailNotificationsEnabled = profile.IsEmailNotificationsEnabled;

            await _dbContext.SaveChangesAsync();
            await SignInUser(user, true);

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadProfileImage(IFormFile profileImage)
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (profileImage is null || profileImage.Length == 0)
            {
                TempData["ProfileError"] = "Choose an image file.";
                return RedirectToAction(nameof(Profile));
            }

            if (profileImage.Length > 2 * 1024 * 1024)
            {
                TempData["ProfileError"] = "Profile image must be 2 MB or less.";
                return RedirectToAction(nameof(Profile));
            }

            var extension = Path.GetExtension(profileImage.FileName).ToLowerInvariant();
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".gif", ".png", ".jpg", ".jpeg" };

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ProfileError"] = "Only gif, png, jpg images are accepted.";
                return RedirectToAction(nameof(Profile));
            }

            var uploadRoot = Path.Combine(_environment.WebRootPath, "Uploads", "Avatars");
            Directory.CreateDirectory(uploadRoot);
            var fileName = $"{user.Id}-{Guid.NewGuid():N}.webp";
            var outputPath = Path.Combine(uploadRoot, fileName);

            try
            {
                await using var stream = profileImage.OpenReadStream();
                await _profileImageProcessor.SaveWebpAsync(stream, outputPath, HttpContext.RequestAborted);
            }
            catch
            {
                TempData["ProfileError"] = "Image could not be processed.";
                return RedirectToAction(nameof(Profile));
            }

            DeleteOldProfileImage(user.ProfileImagePath);
            user.ProfileImagePath = $"/Uploads/Avatars/{fileName}";
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfileImage()
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            DeleteOldProfileImage(user.ProfileImagePath);
            user.ProfileImagePath = null;
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Profile));
        }

        public async Task<IActionResult> VerifyTwoFactor(int userId)
        {
            await DeleteExpiredTwoFactorCodes();

            return View(new VerifyTwoFactorViewModel { UserId = userId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorViewModel verification)
        {
            await DeleteExpiredTwoFactorCodes();

            if (!ModelState.IsValid)
            {
                return View(verification);
            }

            var code = await _dbContext.TwoFactorCodes
                .Include(item => item.AppUser)
                .FirstOrDefaultAsync(item => item.AppUserId == verification.UserId && item.Code == verification.Code);

            if (code is null || code.AppUser is null || code.ExpiresAtUtc <= DateTime.UtcNow)
            {
                ModelState.AddModelError(string.Empty, "Verification code is incorrect or expired.");
                return View(verification);
            }

            var user = code.AppUser;
            var rememberMe = code.RememberMe;
            var userCodes = _dbContext.TwoFactorCodes.Where(item => item.AppUserId == user.Id);

            _dbContext.TwoFactorCodes.RemoveRange(userCodes);
            await _dbContext.SaveChangesAsync();
            await SignInUser(user, rememberMe);

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? id)
        {
            var exceptionFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            var statusCodeFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IStatusCodeReExecuteFeature>();
            var statusCode = id
                ?? statusCodeFeature?.OriginalStatusCode
                ?? (exceptionFeature is not null ? StatusCodes.Status500InternalServerError : HttpContext.Response.StatusCode);

            if (statusCode < StatusCodes.Status400BadRequest)
            {
                statusCode = StatusCodes.Status500InternalServerError;
            }

            HttpContext.Response.StatusCode = statusCode;

            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                StatusCode = statusCode,
                Title = GetErrorTitle(statusCode),
                Message = GetErrorMessage(statusCode)
            });
        }

        private static string GetErrorTitle(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "Bad signal",
                StatusCodes.Status401Unauthorized => "Login required",
                StatusCodes.Status403Forbidden => "Access blocked",
                StatusCodes.Status404NotFound => "Page not found",
                StatusCodes.Status500InternalServerError => "Connection interrupted",
                StatusCodes.Status503ServiceUnavailable => "Service unavailable",
                _ => "Unexpected error"
            };
        }

        private static string GetErrorMessage(int statusCode)
        {
            return statusCode switch
            {
                StatusCodes.Status400BadRequest => "The request reached El-Sim with missing or invalid data.",
                StatusCodes.Status401Unauthorized => "Please sign in again to continue using this page.",
                StatusCodes.Status403Forbidden => "Your account does not have access to this area.",
                StatusCodes.Status404NotFound => "The page may have moved, expired, or never existed.",
                StatusCodes.Status500InternalServerError => "Our server could not finish the request. The team can use the code below to trace it.",
                StatusCodes.Status503ServiceUnavailable => "El-Sim is temporarily unavailable. Please try again in a moment.",
                _ => "The request could not be completed. Please try again or return home."
            };
        }

        private async Task SignInUser(AppUser user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, user.Name),
                new("fin", user.Fin),
                new("is-admin", user.IsAdmin.ToString(CultureInfo.InvariantCulture))
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var properties = new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe ? DateTimeOffset.UtcNow.AddDays(30) : null
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                properties);
        }

        private async Task<AppUser?> GetCurrentUser()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return int.TryParse(id, out var userId) ? await _dbContext.Users.FindAsync(userId) : null;
        }

        private async Task<List<ProductCardViewModel>> GetProducts(string category)
        {
            var products = await _dbContext.Products
                .AsNoTracking()
                .Where(product => product.Category == category)
                .OrderBy(product => product.SortOrder)
                .ThenBy(product => product.Id)
                .ToListAsync();

            return products.Select(ToProductCard).ToList();
        }

        private async Task<List<HomeSliderViewModel>> GetSliders(bool isMobile)
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

        private static ProductCategoryPageViewModel BuildCategoryPage(string category, List<ProductCardViewModel> products)
        {
            var metadata = ProductCatalog.Find(category)!;

            return new ProductCategoryPageViewModel
            {
                Category = metadata.Key,
                Title = metadata.Title,
                Eyebrow = metadata.Eyebrow,
                Description = metadata.PageDescription,
                Products = products
            };
        }

        private static ProductCardViewModel ToProductCard(Product product)
        {
            return new ProductCardViewModel
            {
                Id = product.Id,
                Category = product.Category,
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
                ButtonText = product.ButtonText,
                ButtonTextRu = product.ButtonTextRu,
                ButtonTextAz = product.ButtonTextAz,
                ButtonUrl = product.ButtonUrl,
                IsFeatured = product.IsFeatured,
                IsFavorite = product.IsFavorite,
                SortOrder = product.SortOrder,
                Features = product.Features
                    .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList(),
                FeaturesRu = product.FeaturesRu
                    .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList(),
                FeaturesAz = product.FeaturesAz
                    .Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList()
            };
        }

        private async Task SendTwoFactorCode(AppUser user, bool rememberMe)
        {
            var oldCodes = _dbContext.TwoFactorCodes.Where(item => item.AppUserId == user.Id);
            _dbContext.TwoFactorCodes.RemoveRange(oldCodes);

            var code = RandomNumberGenerator.GetInt32(0, 10_000_000).ToString("D7", CultureInfo.InvariantCulture);

            _dbContext.TwoFactorCodes.Add(new TwoFactorCode
            {
                AppUserId = user.Id,
                Code = code,
                RememberMe = rememberMe,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(15)
            });

            await _dbContext.SaveChangesAsync();

            try
            {
                await _emailSender.SendAsync(user.Email!, "El-Sim verification code", BuildTwoFactorEmail(user.Name, code), true);
            }
            catch
            {
                var codes = _dbContext.TwoFactorCodes.Where(item => item.AppUserId == user.Id);
                _dbContext.TwoFactorCodes.RemoveRange(codes);
                await _dbContext.SaveChangesAsync();
                throw;
            }
        }

        private static string BuildTwoFactorEmail(string name, string code)
        {
            var safeName = WebUtility.HtmlEncode(name);
            var safeCode = WebUtility.HtmlEncode(code);

            return $"""
                <!DOCTYPE html>
                <html lang="en">
                <head>
                  <meta charset="UTF-8">
                  <meta name="viewport" content="width=device-width, initial-scale=1.0">
                  <title>El-Sim verification</title>
                </head>
                <body style="margin:0;background:#14181c;color:#f7fbff;font-family:Segoe UI,Arial,sans-serif;">
                  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background:linear-gradient(135deg,#34393a,#14181c);padding:32px 14px;">
                    <tr>
                      <td align="center">
                        <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;border:1px solid rgba(255,255,255,.16);border-radius:24px;overflow:hidden;background:rgba(255,255,255,.08);">
                          <tr>
                            <td style="height:5px;background:linear-gradient(90deg,#37d9ff,#1ca8ff,#ff3f73);"></td>
                          </tr>
                          <tr>
                            <td style="padding:34px 30px 28px;">
                              <div style="font-size:13px;font-weight:800;text-transform:uppercase;color:#37d9ff;">El-Sim security</div>
                              <h1 style="margin:10px 0 12px;font-size:32px;line-height:1.12;color:#f7fbff;">Verification code</h1>
                              <p style="margin:0 0 24px;color:#b9c5cc;font-size:16px;line-height:1.6;">Hi {safeName}, use this code to finish signing in to your El-Sim account.</p>
                              <div style="padding:18px 20px;border-radius:18px;background:#0d1116;border:1px solid rgba(55,217,255,.28);text-align:center;">
                                <div style="font-size:38px;line-height:1.1;font-weight:800;letter-spacing:6px;color:#37d9ff;">{safeCode}</div>
                              </div>
                              <p style="margin:22px 0 0;color:#b9c5cc;font-size:14px;line-height:1.6;">This code expires in 15 minutes. If you did not request it, you can ignore this message.</p>
                            </td>
                          </tr>
                        </table>
                      </td>
                    </tr>
                  </table>
                </body>
                </html>
                """;
        }

        private async Task DeleteExpiredTwoFactorCodes()
        {
            var expiredCodes = _dbContext.TwoFactorCodes.Where(item => item.ExpiresAtUtc <= DateTime.UtcNow);
            _dbContext.TwoFactorCodes.RemoveRange(expiredCodes);
            await _dbContext.SaveChangesAsync();
        }

        private void DeleteOldProfileImage(string? profileImagePath)
        {
            if (string.IsNullOrWhiteSpace(profileImagePath))
            {
                return;
            }

            var relativePath = profileImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relativePath));
            var avatarRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "Uploads", "Avatars"));

            if (fullPath.StartsWith(avatarRoot, StringComparison.OrdinalIgnoreCase) && System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
