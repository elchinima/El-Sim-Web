namespace El_Sim.Infrastructure.Images;

public class SupportImageProcessor
{
    public async Task SaveWebpAsync(Stream input, string outputPath, CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(input, cancellationToken);
        await image.SaveAsWebpAsync(outputPath, new WebpEncoder { Quality = 10 }, cancellationToken);
    }
}
