var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ElSimDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("Email"));
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<ProfileImageProcessor>();
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/login";
        options.Cookie.Name = "ElSim.Auth";
        options.SlidingExpiration = true;
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ElSimDbContext>();
    dbContext.Database.EnsureCreated();
    dbContext.Database.ExecuteSqlRaw("""
        IF COL_LENGTH('Users', 'Email') IS NULL ALTER TABLE [Users] ADD [Email] nvarchar(254) NULL;
        IF COL_LENGTH('Users', 'ProfileImagePath') IS NULL ALTER TABLE [Users] ADD [ProfileImagePath] nvarchar(260) NULL;
        IF COL_LENGTH('Users', 'IsTwoFactorEnabled') IS NULL ALTER TABLE [Users] ADD [IsTwoFactorEnabled] bit NOT NULL CONSTRAINT [DF_Users_IsTwoFactorEnabled] DEFAULT CAST(0 AS bit);
        IF COL_LENGTH('Users', 'IsEmailNotificationsEnabled') IS NULL ALTER TABLE [Users] ADD [IsEmailNotificationsEnabled] bit NOT NULL CONSTRAINT [DF_Users_IsEmailNotificationsEnabled] DEFAULT CAST(0 AS bit);
        IF OBJECT_ID('TwoFactorCodes', 'U') IS NULL
        BEGIN
            CREATE TABLE [TwoFactorCodes] (
                [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT [PK_TwoFactorCodes] PRIMARY KEY,
                [AppUserId] int NOT NULL,
                [Code] nvarchar(7) NOT NULL,
                [RememberMe] bit NOT NULL,
                [ExpiresAtUtc] datetime2 NOT NULL,
                [CreatedAtUtc] datetime2 NOT NULL,
                CONSTRAINT [FK_TwoFactorCodes_Users_AppUserId] FOREIGN KEY ([AppUserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
            );
        END
        """);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "index-html",
    pattern: "index.html",
    defaults: new { controller = "Home", action = "Index" });

app.MapControllerRoute(
    name: "plans-html",
    pattern: "plans.html",
    defaults: new { controller = "Home", action = "Plans" });

app.MapControllerRoute(
    name: "pass-html",
    pattern: "pass.html",
    defaults: new { controller = "Home", action = "Pass" });

app.MapControllerRoute(
    name: "global-html",
    pattern: "global.html",
    defaults: new { controller = "Home", action = "Global" });

app.MapControllerRoute(
    name: "wifi-html",
    pattern: "wifi.html",
    defaults: new { controller = "Home", action = "Wifi" });

app.MapControllerRoute(
    name: "login-html",
    pattern: "login.html",
    defaults: new { controller = "Home", action = "Login" });

app.MapControllerRoute(
    name: "profile-html",
    pattern: "profile.html",
    defaults: new { controller = "Home", action = "Profile" });

app.MapControllerRoute(
    name: "logout-html",
    pattern: "logout",
    defaults: new { controller = "Home", action = "Logout" });

app.MapControllerRoute(
    name: "pages",
    pattern: "{action=Index}",
    defaults: new { controller = "Home" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
