namespace El_Sim.Web.Services;

public class SupportChatCleanupService
{
    private readonly ElSimDbContext _dbContext;
    private readonly IWebHostEnvironment _environment;

    public SupportChatCleanupService(ElSimDbContext dbContext, IWebHostEnvironment environment)
    {
        _dbContext = dbContext;
        _environment = environment;
    }

    public async Task DeleteExpiredAsync(CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-7);
        var images = await _dbContext.SupportChatImages
            .Where(item => item.CreatedAtUtc < cutoff || (item.SupportChatMessage != null && item.SupportChatMessage.SupportChat.CreatedAtUtc < cutoff))
            .ToListAsync(cancellationToken);

        foreach (var image in images)
        {
            DeleteImageFile(image.FilePath);
        }

        if (images.Count > 0)
        {
            _dbContext.SupportChatImages.RemoveRange(images);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var chats = await _dbContext.SupportChats
            .Where(item => item.CreatedAtUtc < cutoff)
            .ToListAsync(cancellationToken);

        if (chats.Count > 0)
        {
            _dbContext.SupportChats.RemoveRange(chats);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private void DeleteImageFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        var webRoot = Path.GetFullPath(_environment.WebRootPath);
        var relativePath = filePath.TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRoot, relativePath));

        if (fullPath.StartsWith(webRoot, StringComparison.OrdinalIgnoreCase) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
