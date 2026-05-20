var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ElSimDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.Configure<EmailOptions>(options =>
{
    var emailSection = builder.Configuration.GetSection("Email");
    var legacyEmailSection = builder.Configuration.GetSection("EmailSettings");

    (emailSection.Exists() ? emailSection : legacyEmailSection).Bind(options);
});
builder.Services.AddScoped<EmailSender>();
builder.Services.AddScoped<ProfileImageProcessor>();
builder.Services.AddScoped<SliderImageProcessor>();
builder.Services.AddScoped<AccountDataInitializer>();
builder.Services.AddScoped<ProductDataInitializer>();
builder.Services.AddScoped<PurchaseDataInitializer>();
builder.Services.AddScoped<ProductPricingService>();
builder.Services.AddScoped<PhoneNumberService>();
builder.Services.AddScoped<StripePaymentService>();
builder.Services.AddHttpClient<ExchangeRateService>();
builder.Services.Configure<StripeOptions>(builder.Configuration.GetSection("Stripe"));
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
    await scope.ServiceProvider.GetRequiredService<AccountDataInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<ProductDataInitializer>().InitializeAsync();
    await scope.ServiceProvider.GetRequiredService<PurchaseDataInitializer>().InitializeAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");

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
    name: "activation-html",
    pattern: "activation.html",
    defaults: new { controller = "Home", action = "Activation" });

app.MapControllerRoute(
    name: "login-html",
    pattern: "login.html",
    defaults: new { controller = "Home", action = "Login" });

app.MapControllerRoute(
    name: "profile-html",
    pattern: "profile.html",
    defaults: new { controller = "Home", action = "Profile" });

app.MapControllerRoute(
    name: "profile-purchases",
    pattern: "profile/purchases",
    defaults: new { controller = "Home", action = "Purchases" });

app.MapControllerRoute(
    name: "profile-receipts",
    pattern: "profile/receipts",
    defaults: new { controller = "Home", action = "Receipts" });

app.MapControllerRoute(
    name: "wallet-topup-success",
    pattern: "wallet/topup/success",
    defaults: new { controller = "Home", action = "TopUpSuccess" });

app.MapControllerRoute(
    name: "admin",
    pattern: "admin/{action=Index}/{id?}",
    defaults: new { controller = "Admin" });

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
