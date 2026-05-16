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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Plans()
        {
            return View();
        }

        public IActionResult Pass()
        {
            return View();
        }

        public IActionResult Global()
        {
            return View();
        }

        public IActionResult Wifi()
        {
            return View();
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
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private async Task SignInUser(AppUser user, bool rememberMe)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Name, user.Name),
                new("fin", user.Fin)
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
                await _emailSender.SendAsync(user.Email!, "El-Sim verification code", $"Your El-Sim verification code is {code}. It expires in 15 minutes.");
            }
            catch
            {
                var codes = _dbContext.TwoFactorCodes.Where(item => item.AppUserId == user.Id);
                _dbContext.TwoFactorCodes.RemoveRange(codes);
                await _dbContext.SaveChangesAsync();
                throw;
            }
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
