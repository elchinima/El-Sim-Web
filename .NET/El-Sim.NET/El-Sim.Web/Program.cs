var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

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
    name: "pages",
    pattern: "{action=Index}",
    defaults: new { controller = "Home" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
