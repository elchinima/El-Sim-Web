namespace El_Sim.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ElSimDbContext _dbContext;
        private readonly ProfileImageProcessor _profileImageProcessor;
        private readonly EmailSender _emailSender;
        private readonly IWebHostEnvironment _environment;
        private readonly IExchangeRateService _exchangeRateService;
        private readonly ProductPricingService _productPricingService;
        private readonly PhoneNumberService _phoneNumberService;
        private readonly StripePaymentService _stripePaymentService;
        private static readonly string[] SupportAgentNames = ["Aylin", "Sevda", "Leyla"];

        public HomeController(
            ElSimDbContext dbContext,
            ProfileImageProcessor profileImageProcessor,
            EmailSender emailSender,
            IWebHostEnvironment environment,
            IExchangeRateService exchangeRateService,
            ProductPricingService productPricingService,
            PhoneNumberService phoneNumberService,
            StripePaymentService stripePaymentService)
        {
            _dbContext = dbContext;
            _profileImageProcessor = profileImageProcessor;
            _emailSender = emailSender;
            _environment = environment;
            _exchangeRateService = exchangeRateService;
            _productPricingService = productPricingService;
            _phoneNumberService = phoneNumberService;
            _stripePaymentService = stripePaymentService;
        }

        public async Task<IActionResult> Index()
        {
            return View(new HomeProductsViewModel
            {
                EsimProducts = await GetProducts("esim"),
                DesktopSliders = await GetSliders(false),
                MobileSliders = await GetSliders(true),
                ExchangeRate = await _exchangeRateService.GetUsdToAznAsync(HttpContext.RequestAborted)
            });
        }

        public async Task<IActionResult> Plans()
        {
            return View(await BuildCategoryPageWithRate("tariffs", await GetProducts("tariffs")));
        }

        public async Task<IActionResult> Pass()
        {
            return View(await BuildCategoryPageWithRate("pass", await GetProducts("pass")));
        }

        public async Task<IActionResult> Global()
        {
            return View(await BuildCategoryPageWithRate("global", await GetProducts("global")));
        }

        public async Task<IActionResult> Wifi()
        {
            return View(await BuildCategoryPageWithRate("wifi", await GetProducts("wifi")));
        }

        [Authorize]
        public IActionResult Support()
        {
            if (!HttpContext.Session.GetInt32("BotQuestionCount").HasValue)
            {
                HttpContext.Session.SetInt32("BotQuestionCount", 0);
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> Activation()
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new ActivationPageViewModel
            {
                BalanceAzn = user.Account.BalanceAzn,
                ExchangeRate = await _exchangeRateService.GetUsdToAznAsync(HttpContext.RequestAborted),
                PrefixPrices = await GetPrefixPrices()
            });
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
            var user = await _dbContext.Users
                .Include(item => item.Account)
                .FirstOrDefaultAsync(item => item.Fin == login.Fin);

            if (user is null || !PasswordHasher.VerifyPassword(login.Password, user.PasswordHash, user.PasswordSalt))
            {
                ModelState.AddModelError(string.Empty, "FIN or password is incorrect.");
                return View(new AuthPageViewModel { Login = login, ActiveForm = "login" });
            }

            if (user.Account.IsBlocked)
            {
                ModelState.AddModelError(string.Empty, "Your account is blocked.");
                return View(new AuthPageViewModel { Login = login, ActiveForm = "login" });
            }

            if (user.Account.IsTwoFactorEnabled)
            {
                if (string.IsNullOrWhiteSpace(user.Account.Email))
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
                Account = new Account(),
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

            var user = await _dbContext.Users
                .Include(item => item.Account)
                .Include(item => item.UserAssets)
                .FirstOrDefaultAsync(item => item.Id == userId);

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
                Email = user.Account.Email ?? string.Empty,
                ProfileImagePath = user.Account.ProfileImagePath ?? string.Empty,
                BalanceAzn = user.Account.BalanceAzn,
                UserAssets = ToUserAssetsViewModel(user.UserAssets),
                IsTwoFactorEnabled = user.Account.IsTwoFactorEnabled,
                IsEmailNotificationsEnabled = user.Account.IsEmailNotificationsEnabled
            });
        }

        [Authorize]
        public async Task<IActionResult> Purchases()
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(await GetUserPurchases(user.Id, null));
        }

        [Authorize]
        public async Task<IActionResult> Receipts()
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(await GetUserReceipts(user.Id, null));
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
                    ProfileImagePath = user.Account.ProfileImagePath ?? string.Empty,
                    BalanceAzn = user.Account.BalanceAzn,
                    UserAssets = ToUserAssetsViewModel(user.UserAssets),
                    IsTwoFactorEnabled = profile.IsTwoFactorEnabled,
                    IsEmailNotificationsEnabled = profile.IsEmailNotificationsEnabled
                });
            }

            user.Name = profile.Name.Trim();
            user.Account.Email = string.IsNullOrWhiteSpace(profile.Email) ? null : profile.Email.Trim();
            user.Account.IsTwoFactorEnabled = profile.IsTwoFactorEnabled;
            user.Account.IsEmailNotificationsEnabled = profile.IsEmailNotificationsEnabled;

            await _dbContext.SaveChangesAsync();
            await SignInUser(user, true);

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TopUpBalance(TopUpBalanceViewModel topUp)
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid || topUp.AmountAzn < 1m)
            {
                TempData["ProfileError"] = "Enter a valid top-up amount.";
                return RedirectToAction(nameof(Profile));
            }

            var amount = decimal.Round(topUp.AmountAzn, 2, MidpointRounding.AwayFromZero);
            var session = await _stripePaymentService.CreateBalanceTopUpSessionAsync(user, amount, Request, HttpContext.RequestAborted);

            _dbContext.WalletTransactions.Add(new WalletTransaction
            {
                UserId = user.Id,
                Type = "TopUp",
                Status = "Pending",
                AmountAzn = amount,
                BalanceAfterAzn = user.Account.BalanceAzn,
                StripeSessionId = session.Id,
                Description = "Balance top-up",
                CreatedAtUtc = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            return Redirect(session.Url);
        }

        [Authorize]
        public async Task<IActionResult> TopUpSuccess(string session_id)
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            if (string.IsNullOrWhiteSpace(session_id))
            {
                return RedirectToAction(nameof(Profile));
            }

            var transaction = await _dbContext.WalletTransactions
                .Include(item => item.User)
                .ThenInclude(item => item!.Account)
                .FirstOrDefaultAsync(item => item.StripeSessionId == session_id && item.UserId == user.Id);

            if (transaction is null)
            {
                TempData["ProfileError"] = "Top-up session was not found.";
                return RedirectToAction(nameof(Profile));
            }

            if (transaction.Status == "Paid")
            {
                return RedirectToAction(nameof(Profile));
            }

            var session = await _stripePaymentService.GetSessionAsync(session_id, HttpContext.RequestAborted);

            if (!string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ProfileError"] = "Payment was not completed.";
                return RedirectToAction(nameof(Profile));
            }

            user.Account.BalanceAzn += transaction.AmountAzn;
            transaction.Status = "Paid";
            transaction.BalanceAfterAzn = user.Account.BalanceAzn;

            _dbContext.PaymentReceipts.Add(BuildReceipt(
                user.Id,
                null,
                transaction.Id,
                "TopUp",
                "Paid",
                "AZN",
                transaction.AmountAzn,
                transaction.AmountAzn,
                null,
                0m,
                "Balance top-up",
                new { transaction.StripeSessionId, transaction.AmountAzn }));

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PurchaseProduct(int productId, bool includeStaticIp)
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var product = await _dbContext.Products.FirstOrDefaultAsync(item => item.Id == productId);

            if (product is null)
            {
                return RedirectToAction(nameof(Profile));
            }

            var productType = ResolveProductType(product);

            if (RequiresBasicNumber(productType, product.Category) && string.IsNullOrWhiteSpace(user.UserAssets?.BasicNumber))
            {
                TempData["ProfileError"] = "Buy a regular number first.";
                return RedirectToAction(nameof(Profile));
            }

            if (RequiresGlobalNumber(productType, product.Category) && string.IsNullOrWhiteSpace(user.UserAssets?.GlobalNumber))
            {
                TempData["ProfileError"] = "Buy a global number first.";
                return RedirectToAction(nameof(Profile));
            }

            var calculation = await _productPricingService.CalculateAsync(product, _exchangeRateService, HttpContext.RequestAborted);
            var staticIpRate = product.Category == "wifi" && includeStaticIp
                ? await GetDecimalSetting("WifiStaticIpPercent", 5m) / 100m
                : 0m;
            var staticIpFeeAzn = decimal.Round(calculation.TotalAzn * staticIpRate, 2, MidpointRounding.AwayFromZero);
            var totalAzn = calculation.TotalAzn + staticIpFeeAzn;

            if (user.Account.BalanceAzn < totalAzn)
            {
                TempData["ProfileError"] = "Not enough balance.";
                return RedirectToAction(nameof(Profile));
            }

            var oldPurchases = await _dbContext.ProductPurchases
                .Where(item => item.UserId == user.Id
                    && item.Category == product.Category
                    && item.ProductType == productType
                    && item.Status == "Active")
                .ToListAsync();

            foreach (var oldPurchase in oldPurchases)
            {
                oldPurchase.Status = "Cancelled";
                oldPurchase.CancelledAtUtc = DateTime.UtcNow;
                oldPurchase.AdminNote = "Replaced by a new purchase.";
            }

            user.Account.BalanceAzn -= totalAzn;

            var purchase = new ProductPurchase
            {
                UserId = user.Id,
                ProductId = product.Id,
                Category = product.Category,
                ProductType = productType,
                ProductName = includeStaticIp && product.Category == "wifi" ? $"{product.Name} + Static IP" : product.Name,
                ProductCurrency = calculation.Currency,
                ProductAmount = calculation.ProductAmount,
                TotalAzn = totalAzn,
                ExchangeRate = calculation.ExchangeRate,
                CommissionRate = calculation.CommissionRate,
                HasStaticIp = includeStaticIp && product.Category == "wifi",
                StaticIpRate = staticIpRate,
                StaticIpFeeAzn = staticIpFeeAzn,
                Status = "Active",
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.ProductPurchases.Add(purchase);
            await _dbContext.SaveChangesAsync();

            _dbContext.WalletTransactions.Add(new WalletTransaction
            {
                UserId = user.Id,
                ProductPurchaseId = purchase.Id,
                Type = "Purchase",
                Status = "Paid",
                AmountAzn = -totalAzn,
                BalanceAfterAzn = user.Account.BalanceAzn,
                Description = purchase.ProductName,
                CreatedAtUtc = DateTime.UtcNow
            });

            ApplyUserAsset(user, product, purchase.HasStaticIp);

            _dbContext.PaymentReceipts.Add(BuildReceipt(
                user.Id,
                purchase.Id,
                null,
                "Purchase",
                "Paid",
                calculation.Currency,
                calculation.ProductAmount,
                totalAzn,
                calculation.ExchangeRate,
                calculation.CommissionRate,
                purchase.ProductName,
                new
                {
                    product.Id,
                    product.Category,
                    product.Name,
                    calculation.Currency,
                    calculation.ProductAmount,
                    BaseTotalAzn = calculation.TotalAzn,
                    TotalAzn = totalAzn,
                    calculation.ExchangeRate,
                    calculation.CommissionRate,
                    purchase.HasStaticIp,
                    purchase.StaticIpRate,
                    purchase.StaticIpFeeAzn
                }));

            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Profile));
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PurchaseNumber(string prefix, bool isGlobal)
        {
            var user = await GetCurrentUser();

            if (user is null)
            {
                return RedirectToAction(nameof(Login));
            }

            var normalizedPrefix = PhoneNumberService.NormalizePrefix(prefix);

            if (!PhoneNumberService.IsPrefixAllowed(normalizedPrefix, isGlobal))
            {
                TempData["ActivationError"] = "Invalid prefix.";
                return RedirectToAction(nameof(Activation));
            }

            var productType = isGlobal ? "global" : "basic";

            if (isGlobal && !string.IsNullOrWhiteSpace(user.UserAssets?.GlobalNumber))
            {
                TempData["ActivationError"] = "You already have a global number.";
                return RedirectToAction(nameof(Activation));
            }

            if (!isGlobal && !string.IsNullOrWhiteSpace(user.UserAssets?.BasicNumber))
            {
                TempData["ActivationError"] = "You already have a regular number.";
                return RedirectToAction(nameof(Activation));
            }

            var product = await _dbContext.Products
                .Where(item => item.Category == "esim" && item.ProductType == productType)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .FirstOrDefaultAsync()
                ?? await _dbContext.Products
                    .Where(item => item.Category == "esim")
                    .OrderBy(item => item.SortOrder)
                    .ThenBy(item => item.Id)
                    .FirstOrDefaultAsync();

            if (product is null)
            {
                TempData["ActivationError"] = "Number product is not configured.";
                return RedirectToAction(nameof(Activation));
            }

            var basePrice = await GetDecimalSetting(PhoneNumberService.SettingKey(normalizedPrefix, isGlobal), isGlobal ? 10m : 5m);
            var currency = await GetStringSetting(PhoneNumberService.CurrencySettingKey(normalizedPrefix, isGlobal), "AZN");
            var generatedNumber = await _phoneNumberService.GenerateAsync(normalizedPrefix, isGlobal);
            var productAmount = decimal.Round(basePrice * generatedNumber.PriceMultiplier, 2, MidpointRounding.AwayFromZero);
            var exchangeRate = currency == "USD"
                ? await _exchangeRateService.GetUsdToAznAsync(HttpContext.RequestAborted)
                : null;
            var totalAzn = exchangeRate is null
                ? productAmount
                : decimal.Round(productAmount * exchangeRate.UsdToAzn * (1 + ProductPricingService.ConversionCommissionRate), 2, MidpointRounding.AwayFromZero);

            if (user.Account.BalanceAzn < totalAzn)
            {
                TempData["ActivationError"] = "Not enough balance.";
                return RedirectToAction(nameof(Activation));
            }

            var oldPurchases = await _dbContext.ProductPurchases
                .Where(item => item.UserId == user.Id && item.Category == "esim" && item.ProductType == productType && item.Status == "Active")
                .ToListAsync();

            foreach (var oldPurchase in oldPurchases)
            {
                oldPurchase.Status = "Cancelled";
                oldPurchase.CancelledAtUtc = DateTime.UtcNow;
                oldPurchase.AdminNote = "Replaced by a new number.";
            }

            user.UserAssets ??= new UserAssets();
            user.Account.BalanceAzn -= totalAzn;

            if (isGlobal)
            {
                user.UserAssets.GlobalNumber = generatedNumber.Number;
            }
            else
            {
                user.UserAssets.BasicNumber = generatedNumber.Number;
            }

            var productName = isGlobal ? $"Global number {generatedNumber.Number}" : $"Regular number {generatedNumber.Number}";
            var purchase = new ProductPurchase
            {
                UserId = user.Id,
                ProductId = product.Id,
                Category = "esim",
                ProductType = productType,
                ProductName = productName,
                PhoneNumber = generatedNumber.Number,
                PhonePrefix = generatedNumber.Prefix,
                ProductCurrency = currency,
                ProductAmount = productAmount,
                TotalAzn = totalAzn,
                ExchangeRate = exchangeRate?.UsdToAzn,
                CommissionRate = exchangeRate is null ? 0m : ProductPricingService.ConversionCommissionRate,
                Status = "Active",
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.ProductPurchases.Add(purchase);
            await _dbContext.SaveChangesAsync();

            _dbContext.WalletTransactions.Add(new WalletTransaction
            {
                UserId = user.Id,
                ProductPurchaseId = purchase.Id,
                Type = "Purchase",
                Status = "Paid",
                AmountAzn = -totalAzn,
                BalanceAfterAzn = user.Account.BalanceAzn,
                Description = purchase.ProductName,
                CreatedAtUtc = DateTime.UtcNow
            });

            _dbContext.PaymentReceipts.Add(BuildReceipt(
                user.Id,
                purchase.Id,
                null,
                "Purchase",
                "Paid",
                currency,
                productAmount,
                totalAzn,
                exchangeRate?.UsdToAzn,
                exchangeRate is null ? 0m : ProductPricingService.ConversionCommissionRate,
                purchase.ProductName,
                new
                {
                    purchase.PhoneNumber,
                    purchase.PhonePrefix,
                    purchase.ProductType,
                    BasePrice = basePrice,
                    Currency = currency,
                    generatedNumber.PriceMultiplier,
                    TotalAzn = totalAzn
                }));

            await _dbContext.SaveChangesAsync();

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

            DeleteOldProfileImage(user.Account.ProfileImagePath);
            user.Account.ProfileImagePath = $"/Uploads/Avatars/{fileName}";
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

            DeleteOldProfileImage(user.Account.ProfileImagePath);
            user.Account.ProfileImagePath = null;
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
                .ThenInclude(item => item!.Account)
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
                new("is-admin", user.Account.IsAdmin.ToString(CultureInfo.InvariantCulture))
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

            return int.TryParse(id, out var userId)
                ? await _dbContext.Users
                    .Include(item => item.Account)
                    .Include(item => item.UserAssets)
                    .FirstOrDefaultAsync(item => item.Id == userId)
                : null;
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

        private async Task<ProductCategoryPageViewModel> BuildCategoryPageWithRate(string category, List<ProductCardViewModel> products)
        {
            var page = BuildCategoryPage(category, products);
            page.ExchangeRate = await _exchangeRateService.GetUsdToAznAsync(HttpContext.RequestAborted);
            page.WifiStaticIpPercent = await GetDecimalSetting("WifiStaticIpPercent", 5m);

            return page;
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

            var normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim().ToUpperInvariant();

            return normalized == "USD" ? "USD" : "AZN";
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

        private static ProductCardViewModel ToProductCard(Product product)
        {
            return new ProductCardViewModel
            {
                Id = product.Id,
                Category = product.Category,
                ProductType = ResolveProductType(product),
                Name = product.Name,
                NameRu = product.NameRu,
                NameAz = product.NameAz,
                Price = product.Price,
                PriceRu = product.PriceRu,
                PriceAz = product.PriceAz,
                Currency = ProductPricingService.NormalizeCurrency(product.Currency, product.Price),
                Amount = ProductPricingService.ParseAmount(product.Price),
                TotalAzn = ProductPricingService.NormalizeCurrency(product.Currency, product.Price) == "AZN"
                    ? ProductPricingService.ParseAmount(product.Price)
                    : 0m,
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

        private static UserAssetsViewModel? ToUserAssetsViewModel(UserAssets? userAssets)
        {
            if (userAssets is null)
            {
                return null;
            }

            var viewModel = new UserAssetsViewModel
            {
                BasicNumber = userAssets.BasicNumber,
                GlobalNumber = userAssets.GlobalNumber,
                Pass = userAssets.Pass,
                BasicTariff = userAssets.BasicTariff,
                GlobalTariff = userAssets.GlobalTariff,
                WiFi = userAssets.WiFi
            };

            return viewModel.HasAnyValue ? viewModel : null;
        }

        private async Task<List<UserPurchaseViewModel>> GetUserPurchases(int userId, int? take = 12)
        {
            var query = _dbContext.ProductPurchases
                .AsNoTracking()
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CreatedAtUtc)
                .Select(item => new UserPurchaseViewModel
                {
                    Category = item.Category,
                    ProductName = item.ProductName,
                    Status = item.Status,
                    TotalAzn = item.TotalAzn,
                    CreatedAtUtc = item.CreatedAtUtc
                });

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }

        private async Task<List<UserReceiptViewModel>> GetUserReceipts(int userId, int? take = 12)
        {
            var query = _dbContext.PaymentReceipts
                .AsNoTracking()
                .Where(item => item.UserId == userId)
                .OrderByDescending(item => item.CreatedAtUtc)
                .Select(item => new UserReceiptViewModel
                {
                    ReceiptNumber = item.ReceiptNumber,
                    Type = item.Type,
                    Status = item.Status,
                    AmountAzn = item.AmountAzn,
                    CreatedAtUtc = item.CreatedAtUtc
                });

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return await query.ToListAsync();
        }

        private static PaymentReceipt BuildReceipt(
            int userId,
            int? purchaseId,
            int? transactionId,
            string type,
            string status,
            string currency,
            decimal originalAmount,
            decimal amountAzn,
            decimal? exchangeRate,
            decimal commissionRate,
            string description,
            object payload)
        {
            return new PaymentReceipt
            {
                UserId = userId,
                ProductPurchaseId = purchaseId,
                WalletTransactionId = transactionId,
                ReceiptNumber = $"ES-{DateTime.UtcNow:yyyyMMddHHmmss}-{RandomNumberGenerator.GetInt32(1000, 9999)}",
                Type = type,
                Status = status,
                Currency = currency,
                OriginalAmount = originalAmount,
                AmountAzn = amountAzn,
                ExchangeRate = exchangeRate,
                CommissionRate = commissionRate,
                Description = description,
                PayloadJson = JsonSerializer.Serialize(payload),
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        private static void ApplyUserAsset(AppUser user, Product product, bool hasStaticIp)
        {
            user.UserAssets ??= new UserAssets();
            var productName = hasStaticIp ? $"{product.Name} + Static IP" : product.Name;
            var productType = ResolveProductType(product);

            switch (productType)
            {
                case "basic":
                    user.UserAssets.BasicNumber = productName;
                    break;
                case "pass":
                    user.UserAssets.Pass = productName;
                    break;
                case "tariffs":
                    user.UserAssets.BasicTariff = productName;
                    break;
                case "global":
                    user.UserAssets.GlobalTariff = productName;
                    break;
                case "wifi":
                    user.UserAssets.WiFi = productName;
                    break;
            }
        }

        private static string ResolveProductType(Product product)
        {
            if (!string.IsNullOrWhiteSpace(product.ProductType))
            {
                return product.ProductType.Trim().ToLowerInvariant();
            }

            return product.Category switch
            {
                "pass" => "pass",
                "tariffs" => "tariff",
                "global" => "global",
                "wifi" => "wifi",
                _ => "basic"
            };
        }

        private static bool RequiresBasicNumber(string productType, string category)
        {
            return productType is "pass" or "tariff"
                || category is "pass" or "tariffs";
        }

        private static bool RequiresGlobalNumber(string productType, string category)
        {
            return productType == "global";
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
                await _emailSender.SendAsync(user.Account.Email!, "El-Sim verification code", BuildTwoFactorEmail(user.Name, code), true);
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
