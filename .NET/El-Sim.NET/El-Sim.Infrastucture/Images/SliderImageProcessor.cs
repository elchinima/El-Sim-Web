namespace El_Sim.Infrastucture.Images;

public class SliderImageProcessor
{
    private const int DesktopWidth = 1600;
    private const int DesktopHeight = 900;
    private const int MobileWidth = 900;
    private const int MobileHeight = 1200;

    public async Task SaveWebpAsync(Stream input, string outputPath, bool isMobile, CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(input, cancellationToken);
        var targetWidth = isMobile ? MobileWidth : DesktopWidth;
        var targetHeight = isMobile ? MobileHeight : DesktopHeight;
        var sourceRatio = (double)image.Width / image.Height;
        var targetRatio = (double)targetWidth / targetHeight;
        int cropWidth;
        int cropHeight;

        if (sourceRatio > targetRatio)
        {
            cropHeight = image.Height;
            cropWidth = (int)Math.Round(cropHeight * targetRatio);
        }
        else
        {
            cropWidth = image.Width;
            cropHeight = (int)Math.Round(cropWidth / targetRatio);
        }

        var x = (image.Width - cropWidth) / 2;
        var y = (image.Height - cropHeight) / 2;

        image.Mutate(context => context.Crop(new Rectangle(x, y, cropWidth, cropHeight)).Resize(targetWidth, targetHeight));

        await image.SaveAsWebpAsync(outputPath, new WebpEncoder { Quality = 50 }, cancellationToken);
    }
}
