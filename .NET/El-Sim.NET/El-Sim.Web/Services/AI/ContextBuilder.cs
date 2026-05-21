namespace El_Sim.Web.Services.AI;

public class ContextBuilder
{
    private readonly ElSimDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public ContextBuilder(ElSimDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task<string> BuildAsync(Intent intent, string agentName)
    {
        var system = (await ReadAiFileAsync("system.md")).Replace("{{name}}", agentName, StringComparison.OrdinalIgnoreCase);
        var data = intent switch
        {
            Intent.Activation => await ReadAiFileAsync("activation.md"),
            Intent.Faq => await ReadAiFileAsync("faq.md"),
            Intent.Plans => await BuildProductsMarkdown("tariffs"),
            Intent.Pass => await BuildProductsMarkdown("pass"),
            Intent.Global => await BuildProductsMarkdown("global"),
            Intent.Wifi => await BuildProductsMarkdown("wifi"),
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(data))
        {
            return system;
        }

        return $"{system}{Environment.NewLine}{Environment.NewLine}--- DATA ---{Environment.NewLine}{data}";
    }

    private async Task<string> ReadAiFileAsync(string fileName)
    {
        var path = GetAiFilePath(fileName);

        if (!File.Exists(path))
        {
            return string.Empty;
        }

        return await File.ReadAllTextAsync(path, Encoding.UTF8);
    }

    private string GetAiFilePath(string fileName)
    {
        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, "..", "El-Sim.Persistence", "Data", "AI", fileName));
    }

    private async Task<string> BuildProductsMarkdown(string category)
    {
        var products = await _dbContext.Products
            .AsNoTracking()
            .Where(product => product.Category == category)
            .OrderBy(product => product.SortOrder)
            .ThenBy(product => product.Id)
            .ToListAsync();

        if (products.Count == 0)
        {
            return "No products found.";
        }

        var builder = new StringBuilder();
        builder.AppendLine("| Name | Price | Traffic | Period |");
        builder.AppendLine("|---|---:|---|---|");

        foreach (var product in products)
        {
            var price = string.Join(' ', new[] { product.Price, product.Currency }.Where(item => !string.IsNullOrWhiteSpace(item)));
            var traffic = string.IsNullOrWhiteSpace(product.Features) ? product.Description : product.Features.Replace("\r\n", ", ").Replace('\n', ',');

            builder.AppendLine($"| {EscapeCell(product.Name)} | {EscapeCell(price)} | {EscapeCell(traffic)} | {EscapeCell(product.Period)} |");
        }

        return builder.ToString();
    }

    private static string EscapeCell(string? value)
    {
        return (value ?? string.Empty).Replace("|", "\\|").Trim();
    }
}
