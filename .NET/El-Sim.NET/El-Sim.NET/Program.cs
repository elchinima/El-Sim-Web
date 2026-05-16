var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

IResult HtmlPage(IWebHostEnvironment environment, string fileName)
{
    var filePath = Path.Combine(environment.WebRootPath, fileName);

    return File.Exists(filePath)
        ? Results.File(filePath, "text/html")
        : Results.NotFound();
}

var pages = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    ["/index"] = "index.html",
    ["/plans"] = "plans.html",
    ["/pass"] = "pass.html",
    ["/global"] = "global.html",
    ["/wifi"] = "wifi.html",
    ["/login"] = "login.html"
};

foreach (var page in pages)
{
    app.MapGet(page.Key, (IWebHostEnvironment environment) => HtmlPage(environment, page.Value));
}

app.Run();
